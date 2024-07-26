using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer
{
    public class Cell
    {
        public object Value { get; set; } = new object();
        public ColumnType ColumnType { get; set; }

        private readonly static Dictionary<ColumnType, Func<object, object>> ValueConverters = new()
        {
            [ColumnType.Text] = ConvertToString,
            [ColumnType.Number] = ConvertToNumber,
            [ColumnType.Choice] = ConvertToString,
            [ColumnType.DateAndTime] = ConvertToDateTime,
            [ColumnType.MultipleLinesOfText] = ConvertToString,
            [ColumnType.Person] = ConvertToString,
            [ColumnType.YesNo] = ConvertToYesNo,
            [ColumnType.Hyperlink] = ConvertToString,
            [ColumnType.Image] = ConvertToString,
            [ColumnType.Lookup] = value => value,
            [ColumnType.AverageRating] = ConvertToAverageRating
        };

        private static string ConvertToString(object value) => value?.ToString() ?? string.Empty;

        private static object ConvertToNumber(object value) => value switch
        {
            double v => v,
            string s when double.TryParse(s, out double d) => d,
            _ => throw new ArgumentException("Invalid value for Number column")
        };

        private static object ConvertToDateTime(object value) => value switch
        {
            DateTime time => time,
            string s when DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt) => dt,
            _ => throw new ArgumentException("Invalid value for Date and Time column")
        };

        private static object ConvertToYesNo(object value) => value switch
        {
            bool v => v,
            string s when bool.TryParse(s, out bool b) => b,
            _ => throw new ArgumentException("Invalid value for Yes/No column")
        };

        private static object ConvertToAverageRating(object value) => value switch
        {
            double v => v,
            string s when double.TryParse(s, out double ar) => ar,
            _ => throw new ArgumentException("Invalid value for Average Rating column")
        };

        public void SetValue(object value)
        {
            Value = ValueConverters.TryGetValue(ColumnType, out var converter)
                ? converter(value)
                : throw new ArgumentException("Unsupported column type");
        }

    }
}
