using System;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace PupTrailsV3.Services
{
    /// <summary>
    /// Generates a unique hardware ID for the current machine using WMI (Windows Management Instrumentation)
    /// This ID is based on the processor and disk drive information to uniquely identify a machine
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class MachineIdGenerator
    {
        /// <summary>
        /// Gets or generates a unique machine ID based on hardware
        /// </summary>
        public static string GetMachineId()
        {
            try
            {
                string processorId = GetProcessorId();
                string diskId = GetDiskId();
                
                if (string.IsNullOrEmpty(processorId) || string.IsNullOrEmpty(diskId))
                {
                    return GenerateFallbackMachineId();
                }

                // Combine processor and disk IDs for uniqueness
                string combined = $"{processorId}|{diskId}";
                return HashMachineData(combined);
            }
            catch
            {
                return GenerateFallbackMachineId();
            }
        }

        /// <summary>
        /// Gets the CPU processor ID
        /// </summary>
        private static string GetProcessorId()
        {
            try
            {
                var searcher = new System.Management.ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                var results = searcher.Get();
                
                foreach (var obj in results)
                {
                    return obj["ProcessorId"]?.ToString() ?? string.Empty;
                }
            }
            catch { }
            
            return string.Empty;
        }

        /// <summary>
        /// Gets the first hard disk's serial number
        /// </summary>
        private static string GetDiskId()
        {
            try
            {
                var searcher = new System.Management.ManagementObjectSearcher("SELECT SerialNumber FROM Win32_LogicalDisk WHERE Name = 'C:'");
                var results = searcher.Get();
                
                foreach (var obj in results)
                {
                    var serialNumber = obj["SerialNumber"]?.ToString();
                    if (!string.IsNullOrEmpty(serialNumber))
                    {
                        return serialNumber;
                    }
                }

                // Fallback to physical disk if logical disk fails
                searcher = new System.Management.ManagementObjectSearcher("SELECT SerialNumber FROM Win32_PhysicalMedia");
                results = searcher.Get();
                
                foreach (var obj in results)
                {
                    var serialNumber = obj["SerialNumber"]?.ToString();
                    if (!string.IsNullOrEmpty(serialNumber))
                    {
                        return serialNumber.Trim();
                    }
                }
            }
            catch { }
            
            return string.Empty;
        }

        /// <summary>
        /// Creates a SHA256 hash of the machine data
        /// </summary>
        private static string HashMachineData(string machineData)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(machineData));
                return Convert.ToHexString(hashedBytes).Substring(0, 32); // First 32 chars of hex
            }
        }

        /// <summary>
        /// Fallback machine ID generator if WMI fails (uses computer name + username)
        /// </summary>
        private static string GenerateFallbackMachineId()
        {
            string computerName = Environment.MachineName;
            string userName = Environment.UserName;
            string combined = $"{computerName}_{userName}";
            
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return Convert.ToHexString(hashedBytes).Substring(0, 32);
            }
        }
    }
}
