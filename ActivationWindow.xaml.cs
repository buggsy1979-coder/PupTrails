using System;
using System.Windows;

namespace PupTrailsV3
{
    public partial class ActivationWindow : Window
    {
        private bool _activationSuccessful = false;
        private string _currentMachineId = string.Empty;

        public ActivationWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentMachineId = Services.MachineIdGenerator.GetMachineId();
                MachineIdTextBox.Text = _currentMachineId;
            }
            catch
            {
                _currentMachineId = string.Empty;
                MachineIdTextBox.Text = "Unable to detect hardware ID";
            }

            LicenseeNameTextBox.Focus();
        }

        private async void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            string licenseeName = LicenseeNameTextBox.Text.Trim();
            string licenseKey = LicenseKeyTextBox.Text.Trim();

            if (string.IsNullOrEmpty(licenseeName))
            {
                ShowError("Please enter your organization/licensee name.");
                return;
            }

            if (string.IsNullOrEmpty(licenseKey))
            {
                ShowError("Please enter your license key.");
                return;
            }

            // Disable button during activation
            ActivateButton.IsEnabled = false;
            ActivateButton.Content = "⏳ Validating...";

            try
            {
                if (string.IsNullOrWhiteSpace(_currentMachineId))
                {
                    ShowError("Unable to read this machine's hardware ID. Please restart the app and try again.");
                    return;
                }

                // Extract metadata from license key (format: key|signature[|machineId][|date])
                var parts = licenseKey.Split('|');
                if (parts.Length < 2)
                {
                    ShowError("Invalid license key format. Expected KEY|SIGNATURE with optional metadata.");
                    return;
                }

                string createdDateStr = parts.Length >= 4
                    ? parts[3]
                    : parts.Length >= 3
                        ? parts[2]
                        : DateTime.Now.Date.ToString("yyyy-MM-dd");

                var (success, message) = await Services.LicenseManager.ActivateLicenseAsync(
                    licenseKey,
                    licenseeName,
                    _currentMachineId,
                    createdDateStr);

                if (success)
                {
                    MessageBox.Show(message, "License Activated", MessageBoxButton.OK, MessageBoxImage.Information);
                    _activationSuccessful = true;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowError(message);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error during activation: {ex.Message}");
            }
            finally
            {
                ActivateButton.IsEnabled = true;
                ActivateButton.Content = "✓ Activate License";
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                "Exit without activating?\n\nPupTrail requires a valid license to run.",
                "Confirm Exit",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _activationSuccessful = false;
                DialogResult = false;
                Application.Current.Shutdown();
            }
        }

        private void ShowError(string message)
        {
            StatusMessage.Text = message;
            StatusMessage.Visibility = Visibility.Visible;
        }

        public bool ActivationSuccessful => _activationSuccessful;
    }
}
