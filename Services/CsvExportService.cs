using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PupTrailsV3.Services
{
    public static class CsvExportService
    {
        public static void ExportToCsv<T>(IEnumerable<T> data, string filePath, Func<T, string[]> propertySelector, string[] headers)
        {
            var csv = new StringBuilder();
            
            // Add headers
            csv.AppendLine(string.Join(",", headers.Select(h => EscapeCsvValue(h))));
            
            // Add rows
            foreach (var item in data)
            {
                var values = propertySelector(item);
                csv.AppendLine(string.Join(",", values.Select(v => EscapeCsvValue(v))));
            }
            
            File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
        }
        
        private static string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";
                
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            
            return value;
        }
    }
}
