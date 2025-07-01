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
                        HCPCSdesc TEXT,
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
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim
            };

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            // Prepare provider cache to avoid duplicate inserts
            var providerNpis = new HashSet<string>();

            // Set PRAGMA settings for performance
            var pragmaCommand = connection.CreateCommand();
            pragmaCommand.CommandText = "PRAGMA journal_mode = WAL; PRAGMA synchronous = OFF; PRAGMA cache_size = 1000000; PRAGMA temp_store = MEMORY";
            await pragmaCommand.ExecuteNonQueryAsync();

            // Prepare commands
            using var providerCmd = connection.CreateCommand();
            providerCmd.CommandText = @"
        INSERT OR IGNORE INTO Provider (NPI, ProviderName, Specialty, State)
        VALUES (@NPI, @ProviderName, @Specialty, @State)";
            var npiParam = providerCmd.Parameters.Add("@NPI", SqliteType.Text);
            var nameParam = providerCmd.Parameters.Add("@ProviderName", SqliteType.Text);
            var specParam = providerCmd.Parameters.Add("@Specialty", SqliteType.Text);
            var stateParam = providerCmd.Parameters.Add("@State", SqliteType.Text);

            using var billingCmd = connection.CreateCommand();
            billingCmd.CommandText = @"
        INSERT INTO BillingRecord (NPI, HCPCScode, HCPCSdesc, PlaceOfService, NumberOfServices, TotalMedicarePayment)
        VALUES (@NPI, @HCPCScode, @HCPCSdesc, @PlaceOfService, @NumberOfServices, @TotalMedicarePayment)";
            var bNpiParam = billingCmd.Parameters.Add("@NPI", SqliteType.Text);
            var hcpcsParam = billingCmd.Parameters.Add("@HCPCScode", SqliteType.Text);
            var hcpcsDescParam = billingCmd.Parameters.Add("@HCPCSdesc", SqliteType.Text);
            var posParam = billingCmd.Parameters.Add("@PlaceOfService", SqliteType.Text);
            var numParam = billingCmd.Parameters.Add("@NumberOfServices", SqliteType.Integer);
            var payParam = billingCmd.Parameters.Add("@TotalMedicarePayment", SqliteType.Real);

            int count = 0;
            int batchSize = 100000;

            using (var stream = new FileStream(_csvFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<BillingRecordMap>();
                var records = csv.GetRecordsAsync<BillingRecord>();

                var transaction = connection.BeginTransaction();
                providerCmd.Transaction = transaction;
                billingCmd.Transaction = transaction;

                await foreach (var record in records)
                {
                    // Insert provider if not already inserted
                    if (providerNpis.Add(record.NPI))
                    {
                        npiParam.Value = record.NPI;
                        nameParam.Value = record.Provider.ProviderName;
                        specParam.Value = record.Provider.Specialty ?? string.Empty;
                        stateParam.Value = record.Provider.State;
                        providerCmd.ExecuteNonQuery();
                    }

                    // Insert billing record
                    bNpiParam.Value = record.NPI;
                    hcpcsParam.Value = record.HCPCScode;
                    hcpcsDescParam.Value = record.HCPCSdesc;
                    posParam.Value = record.PlaceOfService;
                    numParam.Value = record.NumberOfServices;
                    payParam.Value = record.TotalMedicarePayment;
                    billingCmd.ExecuteNonQuery();

                    count++;
                    if (count % batchSize == 0)
                    {
                        transaction.Commit();
                        Console.WriteLine($"Inserted {count} records...");
                        transaction.Dispose();

                        // Start new transaction and assign to commands
                        transaction = connection.BeginTransaction();
                        providerCmd.Transaction = transaction;
                        billingCmd.Transaction = transaction;
                    }
                }
                transaction.Commit();
                transaction.Dispose();
            }

            // Add indexes after all inserts
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
            CREATE INDEX IF NOT EXISTS idx_provider_npi ON Provider(NPI);
            CREATE INDEX IF NOT EXISTS idx_provider_specialty ON Provider(Specialty);
            CREATE INDEX IF NOT EXISTS idx_provider_state ON Provider(State);
            CREATE INDEX IF NOT EXISTS idx_billing_hcpcs ON BillingRecord(HCPCScode);";
            //CREATE INDEX IF NOT EXISTS idx_billing_npi ON BillingRecord(NPI);";
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("Data ingestion complete.");
            return true;
        }
    }
}
