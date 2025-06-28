using Common.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Data.Sqlite;
using System.Globalization;

namespace DataIngestionConsole
{
    public class SQLiteUtil
    {
        private readonly string _csvFilePath;
        private readonly string _dbPath;

        public SQLiteUtil(string csvFilePath, string dbPath)
        {
            _csvFilePath = csvFilePath;
            _dbPath = dbPath;
        }
        public bool CreateDb()
        {
            if (!File.Exists(_csvFilePath))
            {
                Console.WriteLine("CSV file not found!");
                // Throw proper error here
                return false;
            }

            // Ensure the Data directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_dbPath) ?? throw new InvalidOperationException("Invalid db path"));

            // Initialize SQLite database
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
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

            return true;
        }

        public async Task<bool> HydrateDbAsync()
        {
            // Configure CsvHelper
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim
            };

            // Parse CSV and insert data
            using var stream = new FileStream(_csvFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<BillingRecordMap>();

            // Open connection
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();
            using var command = new SqliteCommand();
            command.Connection = connection;

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

            return true;
        }
    }
}
