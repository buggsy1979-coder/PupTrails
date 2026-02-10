using System;
using System.IO;
using System.IO.Compression;

namespace PupTrailsV3.Services
{
    public static class BackupService
    {
        public static void BackupDatabase(string backupPath)
        {
            PathManager.EnsureDirectoriesExist();

            var pupTrailsDir = PathManager.PupTrailsDocsRoot;
            var dataDir = PathManager.DataDirectory;
            var dbPath = Path.Combine(dataDir, "PupTrail.db");

            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException("Database file not found.");
            }

            // Create backup directory if it doesn't exist
            var backupDir = Path.GetDirectoryName(backupPath);
            if (!string.IsNullOrEmpty(backupDir) && !Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            // Force garbage collection to close any lingering database connections
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Create full backup zip with database, attachments (photos), and receipts
            CreateFullBackupZip(pupTrailsDir, backupPath);
        }

        public static void RestoreDatabase(string backupPath)
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException("Backup file not found.");
            }

            if (!backupPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid backup file. Please select a ZIP backup file.");
            }

            // Validate that this is a valid PupTrail backup
            if (!IsValidBackup(backupPath))
            {
                throw new InvalidOperationException("Invalid backup file. The ZIP file does not contain a valid PupTrail database.");
            }

            PathManager.EnsureDirectoriesExist();

            var pupTrailsDir = PathManager.PupTrailsDocsRoot;
            var dataDir = PathManager.DataDirectory;
            var dbPath = Path.Combine(dataDir, "PupTrail.db");

            // Create a backup of current database before restoring
            if (File.Exists(dbPath))
            {
                var backupsDir = PathManager.BackupsDirectory;
                Directory.CreateDirectory(backupsDir);
                var tempBackup = Path.Combine(backupsDir, $"PupTrail_pre_restore_{DateTime.Now:yyyyMMddHHmmss}.zip");
                
                try
                {
                    CreateFullBackupZip(pupTrailsDir, tempBackup);
                }
                catch
                {
                    // If backup fails, continue anyway - user has their own backup to restore
                }
            }

            // Create restore script and marker file
            CreateRestoreScript(backupPath, pupTrailsDir);
        }

        private static void CreateRestoreScript(string backupPath, string pupTrailsDir)
        {
            var baseDir = AppContext.BaseDirectory;
            var scriptPath = Path.Combine(baseDir, "restore_backup.bat");
            var exePath = Path.Combine(baseDir, Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "PupTrailsV3.exe"));
            var markerPath = Path.Combine(baseDir, "restore_pending.txt");

            // Write the backup path to marker file
            File.WriteAllText(markerPath, backupPath);

            // Create batch script
            var script = $@"@echo off
echo Waiting for PupTrails to close...
timeout /t 2 /nobreak >nul

:WAIT_LOOP
tasklist /FI ""IMAGENAME eq {Path.GetFileName(exePath)}"" 2>NUL | find /I ""{Path.GetFileName(exePath)}"" >NUL
if ""%ERRORLEVEL%""==""0"" (
    timeout /t 1 /nobreak >nul
    goto WAIT_LOOP
)

echo Restoring backup...
powershell -Command ""Add-Type -AssemblyName System.IO.Compression.FileSystem; $tempDir = Join-Path $env:TEMP ('PupTrail_Restore_' + [guid]::NewGuid()); New-Item -ItemType Directory -Path $tempDir -Force | Out-Null; [System.IO.Compression.ZipFile]::ExtractToDirectory('{backupPath.Replace("'", "''")}', $tempDir); $pupTrailsDir = '{pupTrailsDir.Replace("'", "''")}'; $dirs = @('data', 'receipts', 'attachments', 'photos'); foreach ($dir in $dirs) {{ $sourceDir = Join-Path $tempDir $dir; $targetDir = Join-Path $pupTrailsDir $dir; if (Test-Path $sourceDir) {{ if (Test-Path $targetDir) {{ Remove-Item -Path $targetDir -Recurse -Force -ErrorAction SilentlyContinue }}; Copy-Item -Path $sourceDir -Destination $targetDir -Recurse -Force }}}}; if (Test-Path $tempDir) {{ Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue }}""

if exist ""{markerPath}"" del ""{markerPath}""

echo Restarting PupTrails...
start """" ""{exePath}""

timeout /t 2 /nobreak >nul
del ""%~f0""
";

            File.WriteAllText(scriptPath, script);
        }

        private static void CreateFullBackupZip(string sourceDir, string zipPath)
        {
            // If file exists, try to delete it with retry logic
            if (File.Exists(zipPath))
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        File.Delete(zipPath);
                        break;
                    }
                    catch (IOException)
                    {
                        if (i == 2) throw;
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }

            // Ensure the file is completely gone before creating new one
            System.Threading.Thread.Sleep(100);

            // Create ZIP manually, excluding the backups directory to avoid circular reference
            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                // Add data directory
                var dataDir = Path.Combine(sourceDir, "data");
                if (Directory.Exists(dataDir))
                {
                    AddDirectoryToZip(archive, dataDir, "data");
                }

                // Add attachments directory
                var attachmentsDir = Path.Combine(sourceDir, "attachments");
                if (Directory.Exists(attachmentsDir))
                {
                    AddDirectoryToZip(archive, attachmentsDir, "attachments");
                }

                // Add receipts directory
                var receiptsDir = Path.Combine(sourceDir, "receipts");
                if (Directory.Exists(receiptsDir))
                {
                    AddDirectoryToZip(archive, receiptsDir, "receipts");
                }
            }
        }

        private static void AddDirectoryToZip(ZipArchive archive, string sourceDir, string entryPrefix)
        {
            var dirInfo = new DirectoryInfo(sourceDir);
            
            // Add all files in this directory
            foreach (var file in dirInfo.GetFiles())
            {
                try
                {
                    var entryName = Path.Combine(entryPrefix, file.Name).Replace("\\", "/");
                    
                    // Special handling for database files - copy first to avoid locks
                    if (file.Extension.Equals(".db", StringComparison.OrdinalIgnoreCase) || 
                        file.Extension.Equals(".db-shm", StringComparison.OrdinalIgnoreCase) ||
                        file.Extension.Equals(".db-wal", StringComparison.OrdinalIgnoreCase))
                    {
                        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + file.Extension);
                        try
                        {
                            File.Copy(file.FullName, tempFile, true);
                            archive.CreateEntryFromFile(tempFile, entryName, CompressionLevel.Optimal);
                        }
                        finally
                        {
                            if (File.Exists(tempFile))
                            {
                                File.Delete(tempFile);
                            }
                        }
                    }
                    else
                    {
                        archive.CreateEntryFromFile(file.FullName, entryName, CompressionLevel.Optimal);
                    }
                }
                catch (IOException ex)
                {
                    // Log and continue if a file can't be added
                    System.Diagnostics.Debug.WriteLine($"Warning: Could not add file {file.FullName} to backup: {ex.Message}");
                }
            }

            // Recursively add subdirectories
            foreach (var subDir in dirInfo.GetDirectories())
            {
                var subEntryPrefix = Path.Combine(entryPrefix, subDir.Name);
                AddDirectoryToZip(archive, subDir.FullName, subEntryPrefix);
            }
        }

        private static bool IsValidBackup(string zipPath)
        {
            try
            {
                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    // Check for current version format: data/PupTrail.db
                    var dbEntry = archive.GetEntry("data/PupTrail.db") ?? archive.GetEntry("data\\PupTrail.db");
                    if (dbEntry != null)
                        return true;

                    // Check for legacy formats:
                    // - Root level database file
                    var rootDb = archive.GetEntry("PupTrail.db");
                    if (rootDb != null)
                        return true;

                    // - Any .db file in data folder
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.Contains("data") && entry.Name.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                            return true;
                    }

                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private static void ExtractFullBackupZip(string zipPath, string targetDir)
        {
            // Force close all database connections
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            // Wait a moment for connections to fully close
            System.Threading.Thread.Sleep(500);

            // Create temp directory for extraction
            var tempDir = Path.Combine(Path.GetTempPath(), $"PupTrail_Restore_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                ZipFile.ExtractToDirectory(zipPath, tempDir);

                // Copy data directory
                var sourceDataDir = Path.Combine(tempDir, "data");
                var targetDataDir = Path.Combine(targetDir, "data");
                if (Directory.Exists(sourceDataDir))
                {
                    CopyDirectoryWithRetry(sourceDataDir, targetDataDir, true);
                }
                else
                {
                    // Handle legacy backup format - database might be in root
                    var rootDbFile = Path.Combine(tempDir, "PupTrail.db");
                    if (File.Exists(rootDbFile))
                    {
                        Directory.CreateDirectory(targetDataDir);
                        var targetDbFile = Path.Combine(targetDataDir, "PupTrail.db");
                        CopyFileWithRetry(rootDbFile, targetDbFile);
                    }
                }

                // Copy receipts directory if exists
                var sourceReceiptsDir = Path.Combine(tempDir, "receipts");
                var targetReceiptsDir = Path.Combine(targetDir, "receipts");
                if (Directory.Exists(sourceReceiptsDir))
                {
                    CopyDirectoryWithRetry(sourceReceiptsDir, targetReceiptsDir, true);
                }

                // Copy attachments directory if exists (contains animal_photos and group_images)
                var sourceAttachmentsDir = Path.Combine(tempDir, "attachments");
                var targetAttachmentsDir = Path.Combine(targetDir, "attachments");
                if (Directory.Exists(sourceAttachmentsDir))
                {
                    CopyDirectoryWithRetry(sourceAttachmentsDir, targetAttachmentsDir, true);
                }
                
                // Handle legacy photo directories
                var sourcePhotosDir = Path.Combine(tempDir, "photos");
                if (Directory.Exists(sourcePhotosDir))
                {
                    var targetPhotosDir = Path.Combine(targetAttachmentsDir, "animal_photos");
                    CopyDirectoryWithRetry(sourcePhotosDir, targetPhotosDir, true);
                }
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        private static void CopyDirectory(string sourceDir, string targetDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                return;

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(targetDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    string newTargetDir = Path.Combine(targetDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newTargetDir, true);
                }
            }
        }

        private static void CopyDirectoryWithRetry(string sourceDir, string targetDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                return;

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(targetDir, file.Name);
                CopyFileWithRetry(file.FullName, targetFilePath);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    string newTargetDir = Path.Combine(targetDir, subDir.Name);
                    CopyDirectoryWithRetry(subDir.FullName, newTargetDir, true);
                }
            }
        }

        private static void CopyFileWithRetry(string sourceFile, string targetFile)
        {
            const int maxRetries = 10;
            const int delayMs = 300;

            // Delete target file if it exists (with retry for locked files)
            if (File.Exists(targetFile))
            {
                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        // For database lock files, try to delete them
                        if (targetFile.EndsWith(".db-shm", StringComparison.OrdinalIgnoreCase) ||
                            targetFile.EndsWith(".db-wal", StringComparison.OrdinalIgnoreCase))
                        {
                            File.SetAttributes(targetFile, FileAttributes.Normal);
                        }
                        File.Delete(targetFile);
                        break;
                    }
                    catch (IOException) when (i < maxRetries - 1)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        System.Threading.Thread.Sleep(delayMs);
                    }
                    catch (UnauthorizedAccessException) when (i < maxRetries - 1)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        System.Threading.Thread.Sleep(delayMs);
                    }
                }
            }

            // Copy the file with retry logic
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    File.Copy(sourceFile, targetFile, true);
                    return;
                }
                catch (IOException) when (i < maxRetries - 1)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    System.Threading.Thread.Sleep(delayMs);
                }
            }
        }
    }
}
