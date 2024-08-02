using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;

namespace DataLayer
{
    public class Column
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public ColumnType TypeId { get; set; }
        public List<object> Value { get; set; } = new List<object>();
        public string Description { get; set; } = string.Empty;
        public bool IsHidden { get; set; } = false;
        public int Width { get; set; } = MsLConstant.DefaultColWidth;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public List<Choice>? Choices { get; set; }
        public Guid ListID { get; set; } = Guid.Empty;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public object? DefaultValue { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public bool AtoZFilter { get; set; } = false;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public bool ZtoAFilter { get; set; } = false;

        public void Hide()
        {
            IsHidden = true;
        }

        public void Widen()
        {
            Width += MsLConstant.WidthIncrement;
        }

        public void Narrow()
        {
            Width = Math.Max(Width - MsLConstant.WidthIncrement, MsLConstant.DefaultColWidth);
        }

        public void Rename(string newName)
        {
            Name = newName;
        }

        public void AddCellValue(object value)
        {
            Value.Add(value);
        }

        public void AtoZ()
        {

            var sortedValues = Value.OfType<string>()
                                         .OrderBy(val => val, StringComparer.Ordinal)
                                         .Cast<object>()
                                         .ToList();
            AtoZFilter = true;
            ZtoAFilter = false;
            UpdateCellValues(sortedValues);
        }

        public void ZtoA()
        {
            var sortedValues = Value.OfType<string>()
                                         .OrderByDescending(val => val, StringComparer.Ordinal)
                                         .Cast<object>()
                                         .ToList();
            AtoZFilter = false;
            ZtoAFilter = true;
            UpdateCellValues(sortedValues);
        }

        private void UpdateCellValues(List<object> sortedValues)
        {
            int sortedIndex = 0;
            Value = Value.Select(val => val is string ? sortedValues[sortedIndex++] : val).ToList();
        }

        public List<object> FilterBy(Func<object, bool> predicate)
        {
            return Value.Where(predicate).ToList();
        }

        public void AddValueToCell(Cell cell, object value)
        {
            if (value is Tuple<string, string> hyperlinkValue)
            {
                cell.Value = new HyperlinkColumn
                {
                    HyperlinkUrl = hyperlinkValue.Item1,
                    DisplayText = hyperlinkValue.Item2
                };
            }
            else
            {
                throw new ArgumentException("HyperlinkColumn values must be of type Tuple<string, string>.");
            }
        }
    }

    public class PersonColumn : Column
    {
        public new string DefaultValue { get; set; } = string.Empty;
        public bool ShowProfilePic { get; set; } = false;

        public PersonColumn()
        {
            TypeId = ColumnType.Person;
        }
    }

    public class YesNoColumn : Column
    {
        public new bool DefaultValue { get; set; } = false;

        public YesNoColumn()
        {
            TypeId = ColumnType.YesNo;
        }
    }

    public class HyperlinkColumn : Column
    {
        public new string DefaultValue { get; set; } = string.Empty;

        public  string HyperlinkUrl { get; set; }

        public  string DisplayText { get; set; }
        public HyperlinkColumn()
        {
            TypeId = ColumnType.Hyperlink;
            HyperlinkUrl = string.Empty;
            DisplayText = string.Empty;
        }
    }

    public class ImageColumn : Column
    {
        public new string DefaultValue { get; set; } = string.Empty;

        public ImageColumn()
        {
            TypeId = ColumnType.Image;
        }


    }

    public class LookupColumn : Column
    {
        public Guid ColumnID { get; set; }

        public LookupColumn()
        {
            TypeId = ColumnType.Lookup;
        }
    }

    public class AverageRatingColumn : Column
    {
        public List<double> Ratings { get; set; } = [];

        public AverageRatingColumn()
        {
            TypeId = ColumnType.AverageRating;
        }

        public double GetAverageRating()
        {
            return Ratings.Average();
        }
    }

    public class MultipleLinesOfTextColumn : Column
    {
        public new string DefaultValue { get; set; } = string.Empty;

        public MultipleLinesOfTextColumn()
        {
            TypeId = ColumnType.MultipleLinesOfText;
        }
    }

    public class TextColumn : Column
    {
        public new string DefaultValue { get; set; } = string.Empty;
        public bool CalculatedValue { get; set; } = false;
        public new bool AtoZFilter { get; set; } = false;
        public new bool ZtoAFilter { get; set; } = false;

        public TextColumn()
        {
            TypeId = ColumnType.Text;
        }
    }

    public class NumberColumn : Column
    {
        public new double DefaultValue { get; set; } = 0.0;

        public NumberColumn()
        {
            TypeId = ColumnType.Number;
        }
    }

    public class ChoiceColumn : Column
    {
        public new List<Choice> Choices { get; set; } =
        [
            new Choice { Name = "Choice 1", Color = Color.Blue },
            new Choice { Name = "Choice 2", Color = Color.Green },
            new Choice { Name = "Choice 3", Color = Color.Yellow }
        ];
        public new string DefaultValue { get; set; } = string.Empty;
        public bool AddValuesManually { get; set; } = false;

        public ChoiceColumn()
        {
            TypeId = ColumnType.Choice;
        }
    }

    public class DateColumn : Column
    {
        public new DateTime DefaultValue { get; set; } = DateTime.Now;

        public DateColumn()
        {
            TypeId = ColumnType.DateAndTime;
        }
    }


    public class Choice
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public Color Color { get; set; }
    }
}