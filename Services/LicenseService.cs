using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PupTrailsV3.Models;

namespace PupTrailsV3.Services
{
    public class LicenseService
    {
        /// <summary>
        /// Represents the license data that gets signed
        /// </summary>
        public class LicenseData
        {
            public string LicenseeName { get; set; } = string.Empty;
            public string LicenseType { get; set; } = "Lifetime"; // Lifetime license
            public DateTime CreatedDate { get; set; }
        }

        /// <summary>
        /// Validates a license key using the embedded public key
        /// </summary>
        public static bool ValidateLicense(string licenseKey, string signature, string licenseeName, string createdDateStr)
        {
            try
            {
                // Parse the creation date
                if (!DateTime.TryParseExact(createdDateStr, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime createdDate))
                {
                    createdDate = DateTime.Now.Date; // Fallback for older license keys
                }
                
                string createdDateString = createdDate.ToString("yyyy-MM-dd");

                // Use the embedded public key
                string publicKeyXml = GetPublicKeyXml();
                
                using (RSA rsa = RSA.Create())
                {
                    rsa.FromXmlString(publicKeyXml);

                    // NOTE: The license generator uses an anonymous type, not LicenseData class
                    // We need to match the exact serialization format
                    // Anonymous types serialize with property order as declared
                    // USE STRING for date to match generator
                    string normalizedName = NormalizeLicenseeName(licenseeName);

                    var licenseDataForValidation = new
                    {
                        LicenseeName = normalizedName,
                        LicenseType = "Lifetime",
                        CreatedDate = createdDateString  // String format matches generator
                    };

                    var jsonOptions = new JsonSerializerOptions
                    {
                        WriteIndented = false // Compact JSON
                    };
                    string dataToVerify = JsonSerializer.Serialize(licenseDataForValidation, jsonOptions);
                    
                    // Debug output
                    System.Diagnostics.Debug.WriteLine($"Validator JSON: {dataToVerify}");
                    System.Diagnostics.Debug.WriteLine($"License Name: {normalizedName}");
                    System.Diagnostics.Debug.WriteLine($"Created Date: {createdDate:yyyy-MM-dd}");
                    
                    byte[] dataBytes = Encoding.UTF8.GetBytes(dataToVerify);
                    byte[] signatureBytes = Convert.FromBase64String(signature);

                    // Verify the signature
                    return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"License validation error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the embedded public key (used in the application)
        /// </summary>
        public static string GetPublicKeyXml()
        {
            // This is the public key that is embedded in the application
            // It can verify signatures but cannot create new ones
            // CRITICAL: This MUST match the public key in LicenseGenerator/LicenseKeys/public.key
            return @"<RSAKeyValue><Modulus>yGuTIuPhzCUBYfM0o3Bw/s6jR9AUACwJ77dbAAqbVE8S3wV7RU8XG8I51F/DT0pidqT9utU1SlHBYno12j76ETS5cHaGHRzKIFKSbGBCqDO/mbm88ma+iesnlMGCVozLoZMiBxm3SaK5sLBdccqTdzHsqyTC1mtCIImsWtHBFEAqJdJtAatRtRoMutotwsRJsjN4Dw9VuqjwhjCquss5Hwmcpbt3LQB6dczzE0TtCPPcb19uPQzwO+Chlf6gPBgoxNkmVpZR1I4cuzPBoPKPmCMIhXkmcM5xZ9LvamylWnRU3jk+nWKh5sZCglb9hRk11tB+zLA72wxEZoWmOnisBQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        }

        private static string NormalizeLicenseeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(' ', parts).ToUpperInvariant();
        }
    }
}
