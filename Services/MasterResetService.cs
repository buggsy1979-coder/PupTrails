using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PupTrailsV3.Data;

namespace PupTrailsV3.Services
{
    public static class MasterResetService
    {
        /// <summary>
        /// Resets PupTrail to its original state by deleting all data
        /// </summary>
        public static async Task ResetAllDataAsync()
        {
            using var context = new PupTrailDbContext();
            
            // Delete all records from all tables
            await DeleteAllRecords(context);
            
            // Delete all physical files (photos, receipts, attachments)
            DeleteAllFiles();
            
            // Save changes
            await context.SaveChangesAsync();
        }

        private static async Task DeleteAllRecords(PupTrailDbContext context)
        {
            // Delete all records - order matters due to foreign key constraints
            // Delete child records first, then parent records
            
            // Delete VetServices (depends on VetVisits)
            await context.Database.ExecuteSqlRawAsync("DELETE FROM VetServices");
            
            // Delete TripAnimals (junction table)
            await context.Database.ExecuteSqlRawAsync("DELETE FROM TripAnimals");
            
            // Delete FileAttachments
            await context.Database.ExecuteSqlRawAsync("DELETE FROM FileAttachments");
            
            // Delete Adoptions (depends on Animals and People)
            await context.Database.ExecuteSqlRawAsync("DELETE FROM Adoptions");
            
            // Delete VetVisits (depends on Animals and People)
            await context.Database.ExecuteSqlRawAsync("DELETE FROM VetVisits");
            
            // Delete Expenses (depends on Animals and Trips)
            await context.Database.ExecuteSqlRawAsync("DELETE FROM Expenses");
            
            // Delete Incomes (depends on Animals and People)
            await context.Database.ExecuteSqlRawAsync("DELETE FROM Incomes");
            
            // Delete Trips
            await context.Database.ExecuteSqlRawAsync("DELETE FROM Trips");
            
            // Delete Animals
            await context.Database.ExecuteSqlRawAsync("DELETE FROM Animals");
            
            // Delete People
            await context.Database.ExecuteSqlRawAsync("DELETE FROM People");
            
            // Delete Intakes
            await context.Database.ExecuteSqlRawAsync("DELETE FROM Intakes");
            
            // Delete MoneyOwed
            await context.Database.ExecuteSqlRawAsync("DELETE FROM MoneyOwed");
            
            // Delete PuppyGroups
            await context.Database.ExecuteSqlRawAsync("DELETE FROM PuppyGroups");
            
            // Reset auto-increment counters
            await ResetAutoIncrement(context);
        }

        private static async Task ResetAutoIncrement(PupTrailDbContext context)
        {
            // Reset SQLite auto-increment sequences
            var tables = new[]
            {
                "Animals", "People", "Trips", "VetVisits", "VetServices",
                "Adoptions", "Expenses", "Incomes", "FileAttachments",
                "Intakes", "MoneyOwed", "PuppyGroups"
            };
            
            foreach (var table in tables)
            {
                await context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM sqlite_sequence WHERE name={0}", table);
            }
        }

        private static void DeleteAllFiles()
        {
            var attachmentsDir = PathManager.AttachmentsDirectory;
            
            if (Directory.Exists(attachmentsDir))
            {
                try
                {
                    // Delete all files and subdirectories in attachments folder
                    Directory.Delete(attachmentsDir, true);
                    
                    // Recreate the empty directory structure
                    Directory.CreateDirectory(attachmentsDir);
                    Directory.CreateDirectory(PathManager.GetAnimalPhotosDirectory());
                    Directory.CreateDirectory(PathManager.GetGroupImagesDirectory());
                }
                catch (Exception ex)
                {
                    LoggingService.LogError("Error deleting files during master reset", ex);
                    throw;
                }
            }
        }
    }
}

