using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PupTrailsV3.ViewModels;
using PupTrailsV3.Views;
using PupTrailsV3.Models;
using PupTrailsV3.Services;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;

namespace PupTrailsV3
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                LoggingService.LogInfo("MainWindow constructor: Initializing component");
                InitializeComponent();
                LoggingService.LogInfo("MainWindow constructor: Component initialized");
                
                LoggingService.LogInfo("MainWindow constructor: Creating MainViewModel");
                this.DataContext = new MainViewModel();
                LoggingService.LogInfo("MainWindow constructor: MainViewModel created");

                // Ensure forced termination is scheduled on any close path
                this.Closing += MainWindow_Closing;
                
                Loaded += async (s, e) =>
                {
                    try
                    {
                        LoggingService.LogInfo("MainWindow Loaded event: Starting");
                        var viewModel = DataContext as MainViewModel;
                        if (viewModel != null)
                        {
                            LoggingService.LogInfo("MainWindow Loaded event: Calling LoadDataAsync");
                            await viewModel.LoadDataAsync();
                            LoggingService.LogInfo("MainWindow Loaded event: LoadDataAsync completed");
                        }
                        else
                        {
                            LoggingService.LogWarning("MainWindow Loaded event: ViewModel is null");
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingService.LogError("MainWindow Loaded event failed", ex);
                        MessageBox.Show($"Error loading data:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                                      "Data Load Error",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                    }
                };
                
                LoggingService.LogInfo("MainWindow constructor: Completed successfully");
            }
            catch (Exception ex)
            {
                LoggingService.LogError("MainWindow constructor failed", ex);
                MessageBox.Show($"Failed to initialize MainWindow:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                              "Initialization Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                throw;
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            try
            {
                LoggingService.LogInfo("MainWindow Closing: scheduling forced process termination");
                ProcessTerminationService.EnsureProcessExit(1500);
            }
            catch (Exception ex)
            {
                LoggingService.LogError("MainWindow Closing: failed to schedule forced termination", ex);
            }
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tag = button?.Tag as string;
            
            // Hide all views
            DashboardView.Visibility = Visibility.Collapsed;
            AnimalsView.Visibility = Visibility.Collapsed;
            PuppiesView.Visibility = Visibility.Collapsed;
            PeopleView.Visibility = Visibility.Collapsed;
            VetVisitsView.Visibility = Visibility.Collapsed;
            AdoptionsView.Visibility = Visibility.Collapsed;
            IntakeView.Visibility = Visibility.Collapsed;
            ExpensesView.Visibility = Visibility.Collapsed;
            IncomeView.Visibility = Visibility.Collapsed;
            MoneyOwedView.Visibility = Visibility.Collapsed;
            ReportsView.Visibility = Visibility.Collapsed;

            // Show selected view
            switch (tag)
            {
                case "Dashboard":
                    DashboardView.Visibility = Visibility.Visible;
                    break;
                case "Animals":
                    AnimalsView.Visibility = Visibility.Visible;
                    break;
                case "Puppies":
                    PuppiesView.Visibility = Visibility.Visible;
                    break;
                case "People":
                    PeopleView.Visibility = Visibility.Visible;
                    break;
                case "VetVisits":
                    VetVisitsView.Visibility = Visibility.Visible;
                    break;
                case "Adoptions":
                    AdoptionsView.Visibility = Visibility.Visible;
                    break;
                case "Intake":
                    IntakeView.Visibility = Visibility.Visible;
                    break;
                case "Expenses":
                    ExpensesView.Visibility = Visibility.Visible;
                    break;
                case "Income":
                    IncomeView.Visibility = Visibility.Visible;
                    break;
                case "MoneyOwed":
                    MoneyOwedView.Visibility = Visibility.Visible;
                    break;
                case "Reports":
                    ReportsView.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void AddAnimal_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddAnimalWindow { Owner = this };
            if (dialog.ShowDialog() == true && dialog.ResultAnimal != null)
            {
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    viewModel.AddAnimal(dialog.ResultAnimal);
                }
            }
        }

        private void ManageGroups_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectWindow = new SelectPuppiesWindow { Owner = this };
                selectWindow.ShowDialog();
                
                // Refresh the data after managing groups
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    _ = viewModel.LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening group management window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddPerson_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddPersonWindow { Owner = this };
            if (dialog.ShowDialog() == true && dialog.ResultPerson != null)
            {
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    viewModel.AddPerson(dialog.ResultPerson);
                }
            }
        }

        private void AddVetVisit_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddVetVisitWindow { Owner = this };
            if (dialog.ShowDialog() == true && dialog.ResultVetVisit != null)
            {
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    viewModel.AddVetVisit(dialog.ResultVetVisit);
                }
            }
        }

        private void AddAdoption_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddAdoptionWindow { Owner = this };
            if (dialog.ShowDialog() == true && dialog.ResultAdoption != null)
            {
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    viewModel.AddAdoption(dialog.ResultAdoption);
                }
            }
        }

        private void AddIntake_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddIntakeWindow { Owner = this };
            if (dialog.ShowDialog() == true && dialog.ResultIntake != null)
            {
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    viewModel.AddIntake(dialog.ResultIntake);
                    _ = viewModel.LoadDataAsync();
                }
            }
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddExpenseWindow { Owner = this };
            if (dialog.ShowDialog() == true && dialog.ResultExpense != null)
            {
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    viewModel.AddExpense(dialog.ResultExpense);
                    _ = viewModel.LoadDataAsync();
                }
            }
        }

        private void AddIncome_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddIncomeWindow { Owner = this };
            if (dialog.ShowDialog() == true && dialog.ResultIncome != null)
            {
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    viewModel.AddIncome(dialog.ResultIncome);
                    _ = viewModel.LoadDataAsync();
                }
            }
        }

        // Double-click handlers for editing records
        private void Animal_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem is Animal animal)
            {
                var dialog = new AddAnimalWindow { Owner = this };
                dialog.LoadAnimal(animal); // Load existing data
                var result = dialog.ShowDialog();
                
                // Refresh data regardless of result (to handle deletions)
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    _ = viewModel.LoadDataAsync();
                    
                    if (result == true && dialog.ResultAnimal != null)
                    {
                        viewModel.UpdateAnimal(dialog.ResultAnimal);
                    }
                }
            }
        }

        private void Person_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem is Person person)
            {
                var dialog = new AddPersonWindow { Owner = this };
                dialog.LoadPerson(person);
                var result = dialog.ShowDialog();
                
                // Refresh data regardless of result (to handle deletions)
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    _ = viewModel.LoadDataAsync();
                    
                    if (result == true && dialog.ResultPerson != null)
                    {
                        viewModel.UpdatePerson(dialog.ResultPerson);
                    }
                }
            }
        }

        private void VetVisit_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem is VetVisit vetVisit)
            {
                var dialog = new AddVetVisitWindow { Owner = this };
                dialog.LoadVetVisit(vetVisit);
                var result = dialog.ShowDialog();
                
                // Refresh data regardless of result (to handle deletions)
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    _ = viewModel.LoadDataAsync();
                    
                    if (result == true && dialog.ResultVetVisit != null)
                    {
                        viewModel.UpdateVetVisit(dialog.ResultVetVisit);
                    }
                }
            }
        }

        private void Adoption_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem is Adoption adoption)
            {
                var dialog = new AddAdoptionWindow { Owner = this };
                dialog.LoadAdoption(adoption);
                var result = dialog.ShowDialog();
                
                // Refresh data regardless of result (to handle deletions)
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    _ = viewModel.LoadDataAsync();
                    
                    if (result == true && dialog.ResultAdoption != null)
                    {
                        viewModel.UpdateAdoption(dialog.ResultAdoption);
                    }
                }
            }
        }

        private void Intake_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem is Intake intake)
            {
                var dialog = new AddIntakeWindow { Owner = this };
                dialog.LoadIntake(intake);
                var result = dialog.ShowDialog();
                
                // Refresh data regardless of result (to handle deletions)
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    _ = viewModel.LoadDataAsync();
                    
                    if (result == true && dialog.ResultIntake != null)
                    {
                        viewModel.UpdateIntake(dialog.ResultIntake);
                    }
                }
            }
        }

        private void Expense_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem is Expense expense)
            {
                var dialog = new AddExpenseWindow { Owner = this };
                dialog.LoadExpense(expense);
                var result = dialog.ShowDialog();
                
                // Refresh data regardless of result (to handle deletions)
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    _ = viewModel.LoadDataAsync();
                    
                    if (result == true && dialog.ResultExpense != null)
                    {
                        viewModel.UpdateExpense(dialog.ResultExpense);
                    }
                }
            }
        }

        private void Income_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem is Income income)
            {
                var dialog = new AddIncomeWindow { Owner = this };
                dialog.LoadIncome(income);
                var result = dialog.ShowDialog();
                
                // Refresh data regardless of result (to handle deletions)
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    _ = viewModel.LoadDataAsync();
                    
                    if (result == true && dialog.ResultIncome != null)
                    {
                        viewModel.UpdateIncome(dialog.ResultIncome);
                    }
                }
            }
        }

        private void AddMoneyOwed_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddMoneyOwedWindow { Owner = this };
            if (dialog.ShowDialog() == true && dialog.ResultMoneyOwed != null)
            {
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    viewModel.AddMoneyOwed(dialog.ResultMoneyOwed);
                    _ = viewModel.LoadDataAsync();
                }
            }
        }

        private void MoneyOwed_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem is MoneyOwed moneyOwed)
            {
                var dialog = new AddMoneyOwedWindow { Owner = this };
                dialog.LoadMoneyOwed(moneyOwed);
                var result = dialog.ShowDialog();
                
                // Refresh data regardless of result (to handle deletions)
                var viewModel = DataContext as MainViewModel;
                if (viewModel != null)
                {
                    _ = viewModel.LoadDataAsync();
                    
                    if (result == true && dialog.ResultMoneyOwed != null)
                    {
                        viewModel.UpdateMoneyOwed(dialog.ResultMoneyOwed);
                    }
                }
            }
        }

        private void AdvancedSearch_Click(object sender, RoutedEventArgs e)
        {
            var searchWindow = new AdvancedSearchWindow { Owner = this };
            searchWindow.ShowDialog();
        }

        private void ExportFacebook_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null)
            {
                var exportWindow = new Views.ExportSocialMediaWindow(viewModel.DatabaseService, "Facebook") { Owner = this };
                exportWindow.ShowDialog();
            }
        }


        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to close PupTrail? This will close the application and all its processes.", 
                "Close Application", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    LoggingService.LogInfo("Close_Click: scheduling forced process termination");
                    ProcessTerminationService.EnsureProcessExit(1500);
                }
                catch (Exception ex)
                {
                    LoggingService.LogError("Close_Click: failed to schedule forced termination", ex);
                }
                Application.Current.Shutdown();
            }
        }

        private async void MasterExport_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            await RunExportWithBackupAsync(
                "Master Export",
                $"PupTrail_MasterExport_{DateTime.Now:yyyyMMdd_HHmmss}.docx",
                button,
                "📤 Master Export");
        }

        private async void GroupExport_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            await RunExportWithBackupAsync(
                "Group Export",
                $"PupTrail_GroupExport_{DateTime.Now:yyyyMMdd_HHmmss}.docx",
                button,
                "📦 Group Export");
        }

        private async Task RunExportWithBackupAsync(string dialogTitle, string defaultFileName, System.Windows.Controls.Button? button, string buttonLabel)
        {
            try
            {
                var result = MessageBox.Show(
                    "This will export ALL data from PupTrail including:\n\n" +
                    "• All Animals\n" +
                    "• All People\n" +
                    "• All Vet Visits\n" +
                    "• All Adoptions\n" +
                    "• All Expenses\n" +
                    "• All Income\n" +
                    "• All Intake Records\n" +
                    "• All Money Owed Records\n" +
                    "• All Puppy Groups\n" +
                    "• References to all Photos and Receipts\n\n" +
                    "The export will also generate a full backup ZIP in the same folder.\n\n" +
                    "Would you like to proceed?",
                    dialogTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Word Document (*.docx)|*.docx",
                    FileName = defaultFileName,
                    Title = $"Save {dialogTitle}"
                };

                if (saveDialog.ShowDialog() != true)
                {
                    return;
                }

                if (button != null)
                {
                    button.IsEnabled = false;
                    button.Content = "⏳ Exporting...";
                }

                var docxPath = saveDialog.FileName;
                var exportDir = Path.GetDirectoryName(docxPath) ?? AppContext.BaseDirectory;
                var baseName = Path.GetFileNameWithoutExtension(docxPath);
                var folderName = $"{baseName}_Export_{DateTime.Now:yyyyMMdd_HHmmss}";
                var exportFolder = Path.Combine(exportDir, folderName);
                Directory.CreateDirectory(exportFolder);

                var exportDocxPath = Path.Combine(exportFolder, baseName + ".docx");
                var backupPath = Path.Combine(exportFolder, $"{baseName}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.zip");

                await MasterExportService.ExportAllDataAsync(exportDocxPath);
                BackupService.BackupDatabase(backupPath);

                if (button != null)
                {
                    button.IsEnabled = true;
                    button.Content = buttonLabel;
                }

                MessageBox.Show(
                    $"Export completed successfully!\n\n" +
                    $"Folder:\n{exportFolder}\n\n" +
                    $"Word document:\n{exportDocxPath}\n\n" +
                    $"Backup ZIP:\n{backupPath}",
                    "Export Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                if (button != null)
                {
                    button.IsEnabled = true;
                    button.Content = buttonLabel;
                }

                MessageBox.Show(
                    $"Export failed:\n\n{ex.Message}",
                    "Export Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                LoggingService.LogError("Export failed", ex);
            }
        }

        private void BackupDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ensure backup directory exists
                var backupsDir = PathManager.BackupsDirectory;
                Directory.CreateDirectory(backupsDir);

                // Create timestamped backup filename
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"PupTrail_Backup_{timestamp}.zip";
                var backupPath = Path.Combine(backupsDir, backupFileName);

                // Perform full backup (database, photos, and receipts)
                BackupService.BackupDatabase(backupPath);
                
                MessageBox.Show($"Full backup completed successfully!\n\nBackup file: {backupPath}\n\nThis backup includes:\n- Database\n- Photos\n- Receipts\n- All attachments", "Backup Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Backup failed: {ex.Message}", "Backup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestoreDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var backupsDir = PathManager.BackupsDirectory;
                
                // Ensure backup directory exists
                if (!Directory.Exists(backupsDir))
                {
                    Directory.CreateDirectory(backupsDir);
                    MessageBox.Show("No backups found. The backups folder has been created.\n\nCreate a backup first using the Backup Database button.", "No Backups", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Open file dialog to select backup ZIP
                var openDialog = new OpenFileDialog
                {
                    Filter = "PupTrail Backup Files (*.zip)|*.zip",
                    Title = "Select Backup to Restore",
                    InitialDirectory = backupsDir
                };

                if (openDialog.ShowDialog() == true)
                {
                    var result = MessageBox.Show(
                        "Restoring a backup will replace all current data including:\n\n- Database\n- Photos\n- Receipts\n- All attachments\n\nA backup of your current data will be created automatically.\n\nThis action cannot be undone. Continue?",
                        "Confirm Restore",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Close database connections before restore
                        var viewModel = DataContext as MainViewModel;
                        if (viewModel != null)
                        {
                            viewModel.CloseConnections();
                        }

                        // Force garbage collection to close any remaining connections
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();

                        // Create the restore script
                        BackupService.RestoreDatabase(openDialog.FileName);
                        
                        MessageBox.Show("Restore process prepared!\n\nThe application will now close and the backup will be restored automatically.\n\nThe application will restart once the restore is complete.", "Restore Starting", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        // Execute the restore script
                        var scriptPath = Path.Combine(AppContext.BaseDirectory, "restore_backup.bat");
                        if (File.Exists(scriptPath))
                        {
                            var process = new System.Diagnostics.Process
                            {
                                StartInfo = new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = scriptPath,
                                    UseShellExecute = true,
                                    CreateNoWindow = false,
                                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                                }
                            };
                            process.Start();
                            
                            // Give the script a moment to start
                            System.Threading.Thread.Sleep(500);
                        }
                        
                        // Close the application
                        Application.Current.Shutdown();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Restore failed: {ex.Message}\n\nPlease ensure you selected a valid PupTrail backup file.", "Restore Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LicenseInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get current machine ID
                string currentMachineId = Services.MachineIdGenerator.GetMachineId();
                
                // Get active license
                var activeLicense = await Services.LicenseManager.GetActiveLicenseAsync();
                
                // Validate current license
                var (isValid, validationMessage) = await Services.LicenseManager.ValidateCurrentLicenseAsync();
                
                string licenseInfo;
                if (activeLicense != null && isValid)
                {
                    licenseInfo = $"✅ LICENSE ACTIVE\n\n" +
                                 $"Organization: {activeLicense.LicenseeName}\n" +
                                 $"License Type: Lifetime\n" +
                                 $"Activated: {activeLicense.ActivationDate:MMMM dd, yyyy}\n\n" +
                                 $"🔒 MACHINE BINDING\n" +
                                 $"Machine ID: {currentMachineId}\n" +
                                 $"Status: ✓ Valid & Bound to This Machine\n\n" +
                                 $"This license is hardware-locked to this specific machine.\n" +
                                 $"To transfer to a different machine, contact support for a new license key.";
                }
                else
                {
                    licenseInfo = $"⚠️ LICENSE ISSUE\n\n" +
                                 $"Current Machine ID: {currentMachineId}\n\n";

                    if (activeLicense != null)
                    {
                        licenseInfo += $"Licensed Machine ID: {activeLicense.MachineId}\n\n" +
                                      $"ERROR: Machine ID mismatch detected!\n" +
                                      $"The license in the database is registered for a different machine.\n\n" +
                                      $"This typically happens when:\n" +
                                      $"• The database was copied from another computer\n" +
                                      $"• Hardware changes were made to this computer\n" +
                                      $"• The database was restored from a different machine\n\n" +
                                      $"Please contact support for a new license key.";
                    }
                    else
                    {
                        var trialStatus = Services.LicenseManager.GetTrialStatus();
                        if (trialStatus.isActive)
                        {
                            var endLocal = trialStatus.trialEndsUtc?.ToLocalTime();
                            licenseInfo += $"✅ FREE TRIAL ACTIVE\n\n" +
                                          $"Days remaining: {trialStatus.daysRemaining}\n" +
                                          $"Trial ends: {endLocal:MMMM dd, yyyy}\n\n" +
                                          $"Activation is required after the trial ends.";
                        }
                        else
                        {
                            licenseInfo += $"No active license found.\n\n" +
                                          $"{trialStatus.message}\n\n" +
                                          $"Please activate a license to continue using this application.";
                        }
                    }
                }
                
                MessageBox.Show(licenseInfo, "License Information", MessageBoxButton.OK, 
                               isValid ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving license information: {ex.Message}", 
                               "License Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
