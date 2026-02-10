using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using PupTrailsV3.Services;

namespace PupTrailsV3;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public App()
    {
        // Global exception handlers for debugging startup issues
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"An unhandled exception occurred:\n\n{e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}", 
                        "Application Error", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
        LoggingService.LogError("Unhandled dispatcher exception", e.Exception);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        var message = ex != null 
            ? $"A fatal exception occurred:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}" 
            : "A fatal exception occurred but details are unavailable.";
            
        MessageBox.Show(message, "Fatal Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
        
        if (ex != null)
        {
            LoggingService.LogError("Unhandled domain exception", ex);
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        bool licenseCheckPassed = false;
        
        try
        {
            // Ensure directories exist before any operations
            PathManager.EnsureDirectoriesExist();
            LoggingService.LogInfo("Directories initialized");
            
            // Initialize database (create tables if needed)
            InitializeDatabase();
            LoggingService.LogInfo("Database initialized");
            
            // Check for valid license before doing anything else
            if (!CheckLicenseOnStartup())
            {
                // License check failed, app should shut down
                LoggingService.LogInfo("License check failed - shutting down");
                Current.Shutdown();
                return;
            }
            
            licenseCheckPassed = true;
            LoggingService.LogInfo("License check passed");
            
            // Perform automatic backup on startup (non-critical)
            try
            {
                PerformAutomaticBackup();
                LoggingService.LogInfo("Automatic backup completed");
            }
            catch (Exception backupEx)
            {
                // Log backup error but don't prevent app from starting
                LoggingService.LogError("Automatic backup failed on startup", backupEx);
                MessageBox.Show($"Warning: Automatic backup failed.\n\n{backupEx.Message}\n\nThe application will continue to start.", 
                                "Backup Warning", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError("Critical error during startup", ex);
            MessageBox.Show($"Error during startup:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                            "Startup Error", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
            
            if (!licenseCheckPassed)
            {
                Current.Shutdown();
                return;
            }
        }
        
        // Always show the main window if license check passed
        if (licenseCheckPassed)
        {
            // CRITICAL: Use BeginInvoke to defer window creation until AFTER OnStartup completes
            // This allows the WPF message loop to start before creating the window
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    LoggingService.LogInfo("Creating and showing MainWindow (deferred via Dispatcher)");
                    var mainWindow = new MainWindow();
                    
                    LoggingService.LogInfo("MainWindow created, setting as application MainWindow");
                    // CRITICAL: Set as application's MainWindow so app doesn't exit immediately
                    Current.MainWindow = mainWindow;
                    
                    LoggingService.LogInfo("Calling mainWindow.Show()");
                    mainWindow.Show();
                    
                    LoggingService.LogInfo("Calling mainWindow.Activate()");
                    mainWindow.Activate();
                    
                    LoggingService.LogInfo("Setting focus and window state");
                    mainWindow.Focus();
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.Topmost = true;  // Force on top
                    mainWindow.Topmost = false; // Then release
                    
                    LoggingService.LogInfo($"MainWindow shown. IsVisible={mainWindow.IsVisible}, IsLoaded={mainWindow.IsLoaded}, WindowState={mainWindow.WindowState}, ActualWidth={mainWindow.ActualWidth}, ActualHeight={mainWindow.ActualHeight}");
                }
                catch (Exception windowEx)
                {
                    LoggingService.LogError("Failed to create or show MainWindow", windowEx);
                    MessageBox.Show($"Failed to open main window:\n\n{windowEx.Message}\n\nStack Trace:\n{windowEx.StackTrace}", 
                                    "Window Error", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Error);
                    Current.Shutdown();
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            LoggingService.LogInfo("OnExit invoked: scheduling forced process termination");
            ProcessTerminationService.EnsureProcessExit(1500);
        }
        catch (Exception ex)
        {
            LoggingService.LogError("Error scheduling forced process termination", ex);
        }
        finally
        {
            base.OnExit(e);
        }
    }

    private bool CheckLicenseOnStartup()
    {
        try
        {
            LoggingService.LogInfo("Checking license status...");
            
            // Check if already licensed
            var isLicensed = LicenseManager.IsApplicationLicensedAsync().Result;
            
            LoggingService.LogInfo($"License check result: {isLicensed}");
            
            if (!isLicensed)
            {
                LoggingService.LogInfo("License not found or invalid - showing activation window");
                
                // Show activation window
                var activationWindow = new ActivationWindow();
                var dialogResult = activationWindow.ShowDialog();
                
                LoggingService.LogInfo($"Activation window closed with result: {dialogResult}");
                
                if (dialogResult != true)
                {
                    LoggingService.LogInfo("Activation was cancelled or failed");
                    return false;
                }
                
                LoggingService.LogInfo("Activation succeeded");
            }
            
            return true;
        }
        catch (Exception ex)
        {
            LoggingService.LogError("Error checking license", ex);
            MessageBox.Show($"Error checking license: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                           "License Error", 
                           MessageBoxButton.OK, 
                           MessageBoxImage.Error);
            return false;
        }
    }

    private void PerformAutomaticBackup()
    {
        try
        {
            // Ensure the backup directory exists
            var backupsDir = PathManager.BackupsDirectory;
            Directory.CreateDirectory(backupsDir);

            var dbPath = Path.Combine(PathManager.DataDirectory, "PupTrail.db");
            
            // Only backup if database exists
            if (File.Exists(dbPath))
            {
                // Create timestamped backup filename
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"PupTrail_AutoBackup_{timestamp}.db";
                var backupPath = Path.Combine(backupsDir, backupFileName);

                // Use BackupService to backup database and all photos
                BackupService.BackupDatabase(backupPath);

                // Clean up old auto-backups (keep only last 10)
                CleanupOldBackups(backupsDir);
                
                LoggingService.LogInfo($"Automatic backup created: {backupFileName}");
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError("Failed to create automatic backup", ex);
        }
    }

    private void CleanupOldBackups(string backupsDir)
    {
        try
        {
            var autoBackupFiles = Directory.GetFiles(backupsDir, "PupTrail_AutoBackup_*.db");
            
            if (autoBackupFiles.Length > 10)
            {
                // Sort by creation time (oldest first)
                Array.Sort(autoBackupFiles, (a, b) => File.GetCreationTime(a).CompareTo(File.GetCreationTime(b)));
                
                // Delete oldest backups, keeping only the 10 most recent
                for (int i = 0; i < autoBackupFiles.Length - 10; i++)
                {
                    File.Delete(autoBackupFiles[i]);
                    
                    // Also delete corresponding _full.zip file if it exists
                    var zipFile = autoBackupFiles[i].Replace(".db", "_full.zip");
                    if (File.Exists(zipFile))
                    {
                        File.Delete(zipFile);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError("Failed to cleanup old backups", ex);
        }
    }

    private void InitializeDatabase()
    {
        try
        {
            using (var context = new PupTrailsV3.Data.PupTrailDbContext())
            {
                // Ensure database is created and all migrations are applied
                context.Database.Migrate();
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError("Database initialization failed", ex);
            MessageBox.Show($"Failed to initialize database:\n\n{ex.Message}\n\nThe application may not work correctly.", 
                           "Database Error", 
                           MessageBoxButton.OK, 
                           MessageBoxImage.Error);
        }
    }
}
