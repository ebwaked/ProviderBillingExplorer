// See https://aka.ms/new-console-template for more information
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Data.Sqlite;
using System.Globalization;
using Common.Models;
using DataIngestionConsole;
using System.Diagnostics;

Console.WriteLine("Hello CompIQ!");

string csvFilePath = @"C:\data.csv";
string dbPath = Path.Combine("..", "..", "..", "..", "Data", "database.db");

try
{
    SQLiteUtil sqlUtil = new SQLiteUtil(csvFilePath, dbPath);
    var dbCreated = sqlUtil.CreateDb();

    if (!dbCreated)
    {
        Console.WriteLine("Error creating db");
    }
    else
    {
        var sw = Stopwatch.StartNew();
        TimeSpan ts = sw.Elapsed;
        Console.WriteLine($"Stopwatch elaped time: {string.Format("{0}:{1}", Math.Floor(ts.TotalMinutes), ts.ToString("ss\\.ff"))}");
        var hydated = await sqlUtil.HydrateDbAsync();
        sw.Stop();
        ts = sw.Elapsed;
        Console.WriteLine($"Stopwatch elaped time: {string.Format("{0}:{1}", Math.Floor(ts.TotalMinutes), ts.ToString("ss\\.ff"))}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error processing data: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();