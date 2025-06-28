// See https://aka.ms/new-console-template for more information
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Data.Sqlite;
using System.Globalization;
using Common.Models;

Console.WriteLine("Hello CompIQ!");
string csvFilePath = "\"C:\\data.csv\"";
string dbPath = Path.Combine("..", "..", "..", "..", "Data", "database.db");

try
{
    if (!File.Exists(csvFilePath))
    {
        Console.WriteLine("CSV file not found!");
        return;
    }

    // Ensure the Data directory exists
    Directory.CreateDirectory(Path.GetDirectoryName(dbPath) ?? throw new InvalidOperationException("Invalid db path"));

    // Initialize SQLite database
    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    // Create tables if they don't exist
    string createProviderTable = @"
                    CREATE TABLE IF NOT EXISTS Provider (
                        NPI TEXT PRIMARY KEY,
                        ProviderName TEXT,
                        Specialty TEXT,
                        State TEXT
                    )";
    string createBillingRecordTable = @"
                    CREATE TABLE IF NOT EXISTS BillingRecord (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        NPI TEXT,
                        HCPCScode TEXT,
                        PlaceOfService TEXT,
                        NumberOfServices INTEGER,
                        TotalMedicarePayment REAL,
                        FOREIGN KEY (NPI) REFERENCES Provider(NPI)
                    )";

    using var command = new SqliteCommand(createProviderTable, connection);
    command.ExecuteNonQuery();
    command.CommandText = createBillingRecordTable;
    command.ExecuteNonQuery();

    // Configure CsvHelper
    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        IgnoreBlankLines = true,
        TrimOptions = TrimOptions.Trim
    };

    // Parse CSV and insert data
    using var stream = new FileStream(csvFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
    using var reader = new StreamReader(stream);
    using var csv = new CsvReader(reader, config);
    csv.Context.RegisterClassMap<BillingRecordMap>();

    var records = new List<BillingRecord>();
    await foreach (var record in csv.GetRecordsAsync<BillingRecord>())
    {
        // Insert into Provider table
        command.CommandText = @"
                        INSERT OR IGNORE INTO Provider (NPI, ProviderName, Specialty, State)
                        VALUES (@NPI, @ProviderName, @Specialty, @State)";
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@NPI", record.NPI);
        command.Parameters.AddWithValue("@ProviderName", record.ProviderName);
        command.Parameters.AddWithValue("@Specialty", record.Specialty ?? string.Empty);
        command.Parameters.AddWithValue("@State", record.State);
        command.ExecuteNonQuery();

        // Insert into BillingRecord table
        command.CommandText = @"
                        INSERT INTO BillingRecord (NPI, HCPCScode, PlaceOfService, NumberOfServices, TotalMedicarePayment)
                        VALUES (@NPI, @HCPCScode, @PlaceOfService, @NumberOfServices, @TotalMedicarePayment)";
        command.Parameters.Clear();
        command.Parameters.AddWithValue("@NPI", record.NPI);
        command.Parameters.AddWithValue("@HCPCScode", record.HCPCScode);
        command.Parameters.AddWithValue("@PlaceOfService", record.PlaceOfService);
        command.Parameters.AddWithValue("@NumberOfServices", record.NumberOfServices);
        command.Parameters.AddWithValue("@TotalMedicarePayment", record.TotalMedicarePayment);
        command.ExecuteNonQuery();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error processing data: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();