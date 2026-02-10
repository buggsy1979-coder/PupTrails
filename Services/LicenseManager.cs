using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PupTrailsV3.Data;

namespace PupTrailsV3.Services
{
    public class LicenseManager
    {
        private const int TrialDays = 14;
        private static readonly JsonSerializerOptions TrialJsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private class TrialInfo
        {
            public string MachineId { get; set; } = string.Empty;
            public DateTime StartDateUtc { get; set; }
        }

        private static string TrialInfoPath => Path.Combine(PathManager.DataDirectory, "trial.json");

        /// <summary>
        /// Checks if the application is properly licensed AND bound to this machine
        /// </summary>
        public static async Task<bool> IsApplicationLicensedAsync()
        {
            try
            {
                using (var context = new PupTrailDbContext())
                {
                    var activeLicense = await System.Threading.Tasks.Task.FromResult(
                        context.Licenses.FirstOrDefault(l => l.IsActive)
                    );

                    if (activeLicense == null)
                    {
                        var trialStatus = GetTrialStatus();
                        return trialStatus.isActive;
                    }

                    // CRITICAL: Verify the license is bound to THIS machine
                    string currentMachineId = MachineIdGenerator.GetMachineId();
                    if (activeLicense.MachineId != currentMachineId)
                    {
                        System.Diagnostics.Debug.WriteLine($"License machine ID mismatch. Expected: {activeLicense.MachineId}, Current: {currentMachineId}");
                        
                        // Deactivate the license since it's not for this machine
                        activeLicense.IsActive = false;
                        activeLicense.Notes += $"\n[Auto-deactivated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}] License machine ID does not match current machine.";
                        await context.SaveChangesAsync();
                        
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"License check error: {ex.Message}");
                return false;
            }
        }

        public static (bool isActive, int daysRemaining, DateTime? trialEndsUtc, string message) GetTrialStatus()
        {
            try
            {
                var trialInfo = EnsureTrialInfo();
                var currentMachineId = MachineIdGenerator.GetMachineId();

                if (!string.IsNullOrWhiteSpace(trialInfo.MachineId)
                    && !string.IsNullOrWhiteSpace(currentMachineId)
                    && !string.Equals(trialInfo.MachineId, currentMachineId, StringComparison.OrdinalIgnoreCase))
                {
                    return (false, 0, null, "Trial is not valid for this machine.");
                }

                if (trialInfo.StartDateUtc == default)
                {
                    trialInfo.StartDateUtc = DateTime.UtcNow;
                    SaveTrialInfo(trialInfo);
                }

                var trialEnd = trialInfo.StartDateUtc.Date.AddDays(TrialDays);
                var remaining = (int)Math.Ceiling((trialEnd - DateTime.UtcNow).TotalDays);
                if (remaining < 0)
                {
                    remaining = 0;
                }

                var isActive = DateTime.UtcNow < trialEnd;
                var message = isActive
                    ? $"Trial active. {remaining} day(s) remaining."
                    : "Trial expired. Activation is required.";

                return (isActive, remaining, trialEnd, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Trial status error: {ex.Message}");
                return (false, 0, null, "Trial status unavailable.");
            }
        }

        private static TrialInfo EnsureTrialInfo()
        {
            var trialInfo = LoadTrialInfo();
            if (trialInfo == null)
            {
                trialInfo = new TrialInfo
                {
                    MachineId = MachineIdGenerator.GetMachineId(),
                    StartDateUtc = DateTime.UtcNow
                };
                SaveTrialInfo(trialInfo);
            }

            if (string.IsNullOrWhiteSpace(trialInfo.MachineId))
            {
                trialInfo.MachineId = MachineIdGenerator.GetMachineId();
                SaveTrialInfo(trialInfo);
            }

            return trialInfo;
        }

        private static TrialInfo? LoadTrialInfo()
        {
            try
            {
                if (!File.Exists(TrialInfoPath))
                {
                    return null;
                }

                var json = File.ReadAllText(TrialInfoPath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<TrialInfo>(json, TrialJsonOptions);
            }
            catch
            {
                return null;
            }
        }

        private static void SaveTrialInfo(TrialInfo trialInfo)
        {
            try
            {
                Directory.CreateDirectory(PathManager.DataDirectory);
                var json = JsonSerializer.Serialize(trialInfo, TrialJsonOptions);
                File.WriteAllText(TrialInfoPath, json);
            }
            catch
            {
                // Non-fatal: trial data is best-effort
            }
        }

        /// <summary>
        /// Activates a license by validating the key and storing it
        /// </summary>
        public static async Task<(bool success, string message)> ActivateLicenseAsync(string licenseKey, string licenseeName, string requestedMachineId, string createdDateStr)
        {
            try
            {
                // Parse the license key format: XXXXX-XXXXX-XXXXX-XXXXX|SIGNATURE|DATE (legacy keys may include an extra machine ID segment)
                if (!licenseKey.Contains("|"))
                {
                    return (false, "Invalid license key format.");
                }

                var parts = licenseKey.Split('|');
                if (parts.Length < 2)
                {
                    return (false, "Invalid license key format. Expected format: KEY|SIGNATURE|DATE (or legacy KEY|SIGNATURE|MACHINEID|DATE).");
                }

                string keyPart = parts[0];
                string signature = parts[1];

                // Always read the current hardware ID on activation
                string currentMachineId = MachineIdGenerator.GetMachineId();
                if (string.IsNullOrWhiteSpace(currentMachineId))
                {
                    currentMachineId = requestedMachineId; // fallback to UI-provided value
                }

                if (string.IsNullOrWhiteSpace(currentMachineId))
                {
                    return (false, "Unable to determine this machine's hardware ID. Please retry after restarting PupTrail.");
                }

                // Validate the license signature
                if (!LicenseService.ValidateLicense(keyPart, signature, licenseeName, createdDateStr))
                {
                    return (false, "License key is invalid or has been tampered with.");
                }

                // Store the license in the database
                using (var context = new PupTrailDbContext())
                {
                    // Check if license already exists
                    var existingLicense = context.Licenses.FirstOrDefault(l => l.LicenseKey == keyPart);
                    if (existingLicense != null)
                    {
                        return (false, "This license key has already been activated.");
                    }

                    // Deactivate any other licenses
                    var otherLicenses = context.Licenses.Where(l => l.IsActive).ToList();
                    foreach (var license in otherLicenses)
                    {
                        license.IsActive = false;
                    }

                    // Add new license
                    var newLicense = new Models.License
                    {
                        LicenseeName = licenseeName,
                        LicenseKey = keyPart,
                        Signature = signature,
                        MachineId = currentMachineId,
                        ActivationDate = DateTime.Now,
                        IsActive = true,
                        Notes = $"Activated on {DateTime.Now:MMMM dd, yyyy HH:mm:ss} for Machine ID: {currentMachineId}"
                    };

                    context.Licenses.Add(newLicense);
                    await context.SaveChangesAsync();
                }

                return (true, $"License successfully activated for {licenseeName}!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"License activation error: {ex.Message}");
                return (false, $"Error activating license: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the active license information (only if it's valid for this machine)
        /// </summary>
        public static async Task<Models.License?> GetActiveLicenseAsync()
        {
            try
            {
                using (var context = new PupTrailDbContext())
                {
                    var activeLicense = await System.Threading.Tasks.Task.FromResult(
                        context.Licenses.FirstOrDefault(l => l.IsActive)
                    );

                    if (activeLicense == null)
                    {
                        return null;
                    }

                    // Verify the license is bound to THIS machine
                    string currentMachineId = MachineIdGenerator.GetMachineId();
                    if (activeLicense.MachineId != currentMachineId)
                    {
                        System.Diagnostics.Debug.WriteLine($"License machine ID mismatch in GetActiveLicenseAsync");
                        return null; // Don't return license if it's not for this machine
                    }

                    return activeLicense;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Validates that the current active license is still valid for this machine
        /// </summary>
        public static async Task<(bool isValid, string message)> ValidateCurrentLicenseAsync()
        {
            try
            {
                using (var context = new PupTrailDbContext())
                {
                    var activeLicense = await System.Threading.Tasks.Task.FromResult(
                        context.Licenses.FirstOrDefault(l => l.IsActive)
                    );

                    if (activeLicense == null)
                    {
                        return (false, "No active license found.");
                    }

                    // Verify machine ID
                    string currentMachineId = MachineIdGenerator.GetMachineId();
                    if (activeLicense.MachineId != currentMachineId)
                    {
                        // Deactivate the license
                        activeLicense.IsActive = false;
                        activeLicense.Notes += $"\n[Auto-deactivated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}] Machine ID mismatch detected.";
                        await context.SaveChangesAsync();

                        return (false, $"License is registered for a different machine.\nCurrent Machine ID: {currentMachineId}\nLicensed Machine ID: {activeLicense.MachineId}");
                    }

                    return (true, $"License is valid for {activeLicense.LicenseeName}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error validating license: {ex.Message}");
            }
        }
    }
}
