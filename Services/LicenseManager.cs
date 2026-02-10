using System;
using System.Linq;
using System.Threading.Tasks;
using PupTrailsV3.Data;

namespace PupTrailsV3.Services
{
    public class LicenseManager
    {
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
                        return false;
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
