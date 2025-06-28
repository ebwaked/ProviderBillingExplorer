// See https://aka.ms/new-console-template for more information
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Data.Sqlite;
using System.Globalization;
using Common.Models;
using DataIngestionConsole;

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
        var hydated = await sqlUtil.HydrateDbAsync();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error processing data: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();