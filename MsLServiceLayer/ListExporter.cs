using DataLayer;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.Json;
using CsvHelper.Configuration;
using CsvHelper;

namespace MsLServiceLayer
{
    public class ListExporter : List
    {
        public ListExporter()
        {
        }

        public static void ExportToCsv(List list, string filePath)
        {
            // Configure CSV writer options
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Adjust settings as needed, e.g., delimiter, quoting, etc.
                Delimiter = ",",
                HasHeaderRecord = true,
                IgnoreBlankLines = true
            };

            // Write CSV file
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, config);
            // Write header based on column names
            foreach (var col in list.Columns)
            {
                csv.WriteField(col.Name);
            }
            csv.NextRecord();

            // Write rows
            foreach (var row in list.Rows)
            {
                foreach (var cell in row.Cells)
                {
                    csv.WriteField(cell.Value?.ToString() ?? string.Empty);
                }
                csv.NextRecord();
            }
        }

        public static class JsonOptions
        {
            public static readonly JsonSerializerOptions Default = new()
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.Preserve
            };
        }

        public static string ExportToJson(List list)
        {
            return JsonSerializer.Serialize(list, JsonOptions.Default);
        }


        public static void SaveToJson(string json, string filePath)
        {
            File.WriteAllText(filePath, json);
        }
    }
}
