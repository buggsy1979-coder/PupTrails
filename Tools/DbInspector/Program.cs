using System;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;

if (args.Length == 0)
{
    Console.WriteLine("Usage: dotnet run --project Tools/DbInspector [--seed] <path-to-PupTrail.db>");
    return;
}

var seedMode = args[0].Equals("--seed", StringComparison.OrdinalIgnoreCase);
var dbPath = seedMode ? args.ElementAtOrDefault(1) ?? string.Empty : args[0];

if (!File.Exists(dbPath))
{
    Console.WriteLine($"Database not found: {dbPath}");
    return;
}

try
{
    using var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    int GetCount(string tableName, bool hasSoftDelete = true)
    {
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = hasSoftDelete
                ? $"SELECT COUNT(*) FROM {tableName} WHERE IsDeleted = 0"
                : $"SELECT COUNT(*) FROM {tableName}";
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result ?? 0);
        }
        catch (SqliteException) when (hasSoftDelete)
        {
            // Older databases may not have IsDeleted columns yet
            return GetCount(tableName, hasSoftDelete: false);
        }
    }

    if (seedMode)
    {
        SeedSampleData(connection);
    }

    var animals = GetCount("Animals");
    var people = GetCount("People");
    var vetVisits = GetCount("VetVisits");

    Console.WriteLine($"Database: {dbPath}");
    Console.WriteLine($"  Animals:   {animals}");
    Console.WriteLine($"  People:    {people}");
    Console.WriteLine($"  VetVisits: {vetVisits}");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to inspect database: {ex.Message}");
}

static void SeedSampleData(SqliteConnection connection)
{
    using var transaction = connection.BeginTransaction();

    // Check if any animals already exist
    using (var checkCommand = connection.CreateCommand())
    {
        checkCommand.CommandText = "SELECT COUNT(*) FROM Animals";
        var existing = Convert.ToInt32(checkCommand.ExecuteScalar() ?? 0);
        if (existing > 0)
        {
            Console.WriteLine("Database already contains animals; skipping sample seed.");
            transaction.Rollback();
            return;
        }
    }

    using var insertAnimal = connection.CreateCommand();
    insertAnimal.CommandText = @"
        INSERT INTO Animals (Name, Breed, Sex, CollarColor, Weight, DOB, IntakeDate, Status, Notes, PhotoPath, CreatedAt, UpdatedAt, IsDeleted, OriginCountry)
        VALUES ('Demo Pup', 'Mixed Breed', 'F', 'Blue', 42, date('now','-1 year'), date('now','-30 days'), 'In Care', 'Friendly demo dog used for preview testing.', NULL, datetime('now'), datetime('now'), 0, 'Canada');
        SELECT last_insert_rowid();";
    var animalId = (long)(insertAnimal.ExecuteScalar() ?? 0);

    using var insertVet = connection.CreateCommand();
    insertVet.CommandText = @"
        INSERT INTO VetVisits (AnimalId, Date, TotalCost, Notes, ReadyForAdoption, WormingDate, DeFleeingDate, SpayedNeuteringDate, VaccinationsGiven, IsDeleted)
        VALUES ($animalId, date('now','-10 days'), 275.50, 'Initial checkup, vaccinations, and spay.', 1, date('now','-25 days'), date('now','-20 days'), date('now','-15 days'), 'Rabies, DAPP', 0);";
    insertVet.Parameters.AddWithValue("$animalId", animalId);
    insertVet.ExecuteNonQuery();

    transaction.Commit();
    Console.WriteLine("Seeded demo animal and vet visit.");
}
