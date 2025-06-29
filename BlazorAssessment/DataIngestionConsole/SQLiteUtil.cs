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
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim
            };

            // Read all records into memory (if memory allows)
            List<BillingRecord> records = new List<BillingRecord>();
            using (var stream = new FileStream(_csvFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<BillingRecordMap>();
                await foreach (var record in csv.GetRecordsAsync<BillingRecord>())
                {
                    records.Add(record);
                }
            }

            // Extract distinct providers
            var providers = records
                .GroupBy(r => r.NPI)
                .Select(g => g.First())
                .ToList();

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            await connection.OpenAsync();

            // Insert providers in a transaction
            using (var transaction = connection.BeginTransaction())
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
               INSERT OR IGNORE INTO Provider (NPI, ProviderName, Specialty, State)
               VALUES (@NPI, @ProviderName, @Specialty, @State)";
                var npiParam = cmd.Parameters.Add("@NPI", SqliteType.Text);
                var nameParam = cmd.Parameters.Add("@ProviderName", SqliteType.Text);
                var specParam = cmd.Parameters.Add("@Specialty", SqliteType.Text);
                var stateParam = cmd.Parameters.Add("@State", SqliteType.Text);

                foreach (var p in providers)
                {
                    npiParam.Value = p.NPI;
                    nameParam.Value = p.ProviderName;
                    specParam.Value = p.Specialty ?? string.Empty;
                    stateParam.Value = p.State;
                    cmd.ExecuteNonQuery();
                }
                transaction.Commit();
            }

            // Insert billing records in batches
            const int batchSize = 1000;
            int total = records.Count;
            for (int i = 0; i < records.Count; i += batchSize)
            {
                using var transaction = connection.BeginTransaction();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
               INSERT INTO BillingRecord (NPI, HCPCScode, PlaceOfService, NumberOfServices, TotalMedicarePayment)
               VALUES (@NPI, @HCPCScode, @PlaceOfService, @NumberOfServices, @TotalMedicarePayment)";
                var npiParam = cmd.Parameters.Add("@NPI", SqliteType.Text);
                var hcpcsParam = cmd.Parameters.Add("@HCPCScode", SqliteType.Text);
                var posParam = cmd.Parameters.Add("@PlaceOfService", SqliteType.Text);
                var numParam = cmd.Parameters.Add("@NumberOfServices", SqliteType.Integer);
                var payParam = cmd.Parameters.Add("@TotalMedicarePayment", SqliteType.Real);

                for (int j = i; j < Math.Min(i + batchSize, records.Count); j++)
                {
                    var r = records[j];
                    npiParam.Value = r.NPI;
                    hcpcsParam.Value = r.HCPCScode;
                    posParam.Value = r.PlaceOfService;
                    numParam.Value = r.NumberOfServices;
                    payParam.Value = r.TotalMedicarePayment;
                    cmd.ExecuteNonQuery();
                }
                transaction.Commit();
                Console.WriteLine($"Inserted {Math.Min(i + batchSize, total)} of {total} records ({(Math.Min(i + batchSize, total) * 100 / total)}%)");
            }

            // Add indexes (optional, but recommended for query performance)
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
               CREATE INDEX IF NOT EXISTS idx_provider_npi ON Provider(NPI);
               CREATE INDEX IF NOT EXISTS idx_provider_specialty ON Provider(Specialty);
               CREATE INDEX IF NOT EXISTS idx_provider_state ON Provider(State);
               CREATE INDEX IF NOT EXISTS idx_billing_hcpcs ON BillingRecord(HCPCScode);";
                cmd.ExecuteNonQuery();
            }

            return true;
        }
    }
}
