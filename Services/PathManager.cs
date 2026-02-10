using System;
using System.IO;

namespace PupTrailsV3.Services
{
    /// <summary>
    /// Manages portable file paths within the PupTrails root directory
    /// All application data is stored within PupTrails folder for full portability
    /// </summary>
    public static class PathManager
    {
        // Application install/run directory (for bundled resources)
        private static readonly string _appBaseDirectory = AppContext.BaseDirectory;
        private static readonly string _pupTrailsRoot;

        static PathManager()
        {
            var baseDir = _appBaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var baseName = Path.GetFileName(baseDir);
            if (string.Equals(baseName, "PupTrails", StringComparison.OrdinalIgnoreCase))
            {
                _pupTrailsRoot = baseDir;
            }
            else
            {
                _pupTrailsRoot = Path.Combine(_appBaseDirectory, "PupTrails");
            }
        }

        /// <summary>
        /// Gets the PupTrails root directory path
        /// </summary>
        public static string PupTrailsRoot => _pupTrailsRoot;

        /// <summary>
        /// Gets the PupTrails docs root directory path (data, logs, backups, attachments)
        /// </summary>
        public static string PupTrailsDocsRoot => Path.Combine(_pupTrailsRoot, "PupTrailsDocs");

        /// <summary>
        /// Gets the data directory path (for database)
        /// </summary>
        public static string DataDirectory => Path.Combine(PupTrailsDocsRoot, "data");

        /// <summary>
        /// Gets the attachments directory path (for receipts, invoices, etc.)
        /// </summary>
        public static string AttachmentsDirectory => Path.Combine(PupTrailsDocsRoot, "attachments");

        /// <summary>
        /// Gets the backups directory path
        /// </summary>
        public static string BackupsDirectory => Path.Combine(PupTrailsDocsRoot, "backups");

        /// <summary>
        /// Gets the logs directory path
        /// </summary>
        public static string LogsDirectory => Path.Combine(PupTrailsDocsRoot, "logs");

        /// <summary>
        /// Gets the group images directory path (for puppy group photos)
        /// </summary>
        public static string GetGroupImagesDirectory()
        {
            var groupImagesDir = Path.Combine(AttachmentsDirectory, "group_images");
            Directory.CreateDirectory(groupImagesDir);
            return groupImagesDir;
        }

        /// <summary>
        /// Gets the animal photos directory path (for individual animal photos)
        /// </summary>
        public static string GetAnimalPhotosDirectory()
        {
            var animalPhotosDir = Path.Combine(AttachmentsDirectory, "animal_photos");
            Directory.CreateDirectory(animalPhotosDir);
            return animalPhotosDir;
        }

        /// <summary>
        /// Ensures all required directories exist
        /// </summary>
        public static void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(PupTrailsDocsRoot);
            MigrateLegacyDocsLayout();
            Directory.CreateDirectory(DataDirectory);
            Directory.CreateDirectory(AttachmentsDirectory);
            Directory.CreateDirectory(BackupsDirectory);
            Directory.CreateDirectory(LogsDirectory);
        }

        /// <summary>
        /// Copies a file to the attachments directory and returns the relative path
        /// </summary>
        public static string SaveAttachment(string sourceFilePath, string subfolder = "")
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
            {
                return string.Empty;
            }

            try
            {
                var targetDir = string.IsNullOrWhiteSpace(subfolder) 
                    ? AttachmentsDirectory 
                    : Path.Combine(AttachmentsDirectory, subfolder);
                
                Directory.CreateDirectory(targetDir);

                var fileName = Path.GetFileName(sourceFilePath);
                var targetPath = Path.Combine(targetDir, fileName);

                // If file already exists, add timestamp to filename
                if (File.Exists(targetPath))
                {
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    var extension = Path.GetExtension(fileName);
                    fileName = $"{fileNameWithoutExt}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                    targetPath = Path.Combine(targetDir, fileName);
                }

                File.Copy(sourceFilePath, targetPath, false);

                // Return relative path from PupTrails root
                return Path.Combine("attachments", subfolder, fileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save attachment: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the full path from a relative path stored in the database
        /// </summary>
        public static string GetFullPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return string.Empty;
            }

            return Path.Combine(PupTrailsDocsRoot, relativePath.Replace("/", "\\"));
        }

        /// <summary>
        /// Resolves the stored photo path for an animal to an absolute on-disk path.
        /// </summary>
        public static string ResolveAnimalPhotoPath(string? storedPhotoPath)
        {
            if (string.IsNullOrWhiteSpace(storedPhotoPath))
            {
                return string.Empty;
            }

            if (Path.IsPathRooted(storedPhotoPath))
            {
                return storedPhotoPath;
            }

            var normalized = storedPhotoPath.Replace("\\", "/").TrimStart('/');

            if (normalized.StartsWith("attachments/", StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(PupTrailsDocsRoot, normalized.Replace("/", "\\"));
            }

            if (normalized.StartsWith("animal_photos/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring("animal_photos/".Length);
            }

            return Path.Combine(GetAnimalPhotosDirectory(), normalized.Replace("/", "\\"));
        }

        /// <summary>
        /// Checks if a file exists given a relative path
        /// </summary>
        public static bool FileExists(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return false;
            }

            var fullPath = GetFullPath(relativePath);
            return File.Exists(fullPath);
        }

        /// <summary>
        /// Gets the path to the export header image (Annie's Little Paws Rescue logo)
        /// </summary>
        public static string GetExportHeaderImagePath()
        {
            // Resources are deployed alongside the application binaries
            var resourcesDir = Path.Combine(_appBaseDirectory, "Resources");
            return Path.Combine(resourcesDir, "ExportHeader.png");
        }

        private static void MigrateLegacyDocsLayout()
        {
            try
            {
                var legacyDataDir = Path.Combine(_pupTrailsRoot, "data");
                var legacyAttachmentsDir = Path.Combine(_pupTrailsRoot, "attachments");
                var legacyBackupsDir = Path.Combine(_pupTrailsRoot, "backups");
                var legacyLogsDir = Path.Combine(_pupTrailsRoot, "logs");

                CopyDirectoryIfMissing(legacyDataDir, DataDirectory);
                CopyDirectoryIfMissing(legacyAttachmentsDir, AttachmentsDirectory);
                CopyDirectoryIfMissing(legacyBackupsDir, BackupsDirectory);
                CopyDirectoryIfMissing(legacyLogsDir, LogsDirectory);
            }
            catch
            {
                // Non-fatal migration best-effort
            }
        }

        private static void CopyDirectoryIfMissing(string sourceDir, string destinationDir)
        {
            if (!Directory.Exists(sourceDir))
            {
                return;
            }

            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

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
                CopyDirectoryIfMissing(dir, Path.Combine(destinationDir, dirName));
            }
        }


    }
}
