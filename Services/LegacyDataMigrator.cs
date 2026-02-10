using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace PupTrailsV3.Services
{
    /// <summary>
    /// Copies user data from the legacy portable storage (next to the executable)
    /// into the shared AppData location whenever the legacy copy appears to be newer.
    /// </summary>
    public static class LegacyDataMigrator
    {
        public static void EnsureLatestCopy()
        {
            try
            {
                var legacyRoot = Path.Combine(AppContext.BaseDirectory, "PupTrails");
                if (!Directory.Exists(legacyRoot))
                {
                    return;
                }

                var legacyDbPath = Path.Combine(legacyRoot, "data", "PupTrail.db");
                if (!File.Exists(legacyDbPath))
                {
                    return;
                }

                Directory.CreateDirectory(PathManager.DataDirectory);
                Directory.CreateDirectory(PathManager.AttachmentsDirectory);

                var targetDbPath = Path.Combine(PathManager.DataDirectory, "PupTrail.db");
                var legacyCount = GetAnimalCountSafe(legacyDbPath);
                var targetCount = GetAnimalCountSafe(targetDbPath);

                var copiedDatabase = false;

                if (!File.Exists(targetDbPath) || legacyCount > targetCount)
                {
                    File.Copy(legacyDbPath, targetDbPath, overwrite: true);
                    copiedDatabase = true;
                    LoggingService.LogInfo($"LegacyDataMigrator: copied PupTrail.db from legacy location (legacyCount={legacyCount}, targetCount={targetCount}).");
                }

                // Copy attachments if the legacy location has more files than the target
                var legacyAttachments = Path.Combine(legacyRoot, "attachments");
                if (Directory.Exists(legacyAttachments))
                {
                    CopyDirectory(legacyAttachments, PathManager.AttachmentsDirectory);
                }

                if (!copiedDatabase)
                {
                    LoggingService.LogInfo($"LegacyDataMigrator: no copy performed (legacyCount={legacyCount}, targetCount={targetCount}).");
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogError("LegacyDataMigrator: failed to copy legacy data", ex);
            }
        }

        private static int GetAnimalCountSafe(string dbPath)
        {
            try
            {
                if (!File.Exists(dbPath))
                {
                    return 0;
                }

                using var connection = new SqliteConnection($"Data Source={dbPath}");
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Animals WHERE IsDeleted = 0";
                var result = command.ExecuteScalar();
                return Convert.ToInt32(result ?? 0);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"LegacyDataMigrator: failed to read animal count from {dbPath}", ex);
                return 0;
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.TopDirectoryOnly))
            {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(destinationDir, fileName);
                if (!File.Exists(destFile))
                {
                    File.Copy(file, destFile, overwrite: false);
                }
            }

            foreach (var dir in Directory.EnumerateDirectories(sourceDir, "*", SearchOption.TopDirectoryOnly))
            {
                var dirName = Path.GetFileName(dir);
                CopyDirectory(dir, Path.Combine(destinationDir, dirName));
            }
        }
    }
}
