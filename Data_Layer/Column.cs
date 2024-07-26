

using System.Drawing;
using System.Text.Json.Serialization;

namespace DataLayer
{
    public class Column
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public ColumnType Type { get; set; }
        public List<object> CellValues { get; set; } = [];
        public string Description { get; set; } = string.Empty;
        public bool IsHidden { get; set; } = false;
        public int Width { get; set; } = MsLConstant.DefaultColWidth;

        public Guid ParentID { get; set; } = Guid.Empty;

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
            CellValues.Add(value);
        }

        public void AtoZ()
        {
            var sortedValues = CellValues.OfType<string>()
                                         .OrderBy(val => val, StringComparer.Ordinal)
                                         .Cast<object>()
                                         .ToList();

            UpdateCellValues(sortedValues);
        }

        public void ZtoA()
        {
            var sortedValues = CellValues.OfType<string>()
                                         .OrderByDescending(val => val, StringComparer.Ordinal)
                                         .Cast<object>()
                                         .ToList();

            UpdateCellValues(sortedValues);
        }

        private void UpdateCellValues(List<object> sortedValues)
        {
            int sortedIndex = 0;
            CellValues = CellValues.Select(val => val is string ? sortedValues[sortedIndex++] : val).ToList();
        }

        public List<object> FilterBy(Func<object, bool> predicate)
        {
            return CellValues.Where(predicate).ToList();
        }
    }
    public class PersonColumn : Column
    {
        public string DefaultValue { get; set; } = string.Empty;
        public bool ShowProfilePic { get; set; } = false;

        public PersonColumn()
        {
            Type = ColumnType.Person;
        }
    }

    public class YesNoColumn : Column
    {
        public bool DefaultValue { get; set; } = false;

        public YesNoColumn()
        {
            Type = ColumnType.YesNo;
        }
    }

    public class HyperlinkColumn : Column
    {
        public string DefaultValue { get; set; } = string.Empty;

        public HyperlinkColumn()
        {
            Type = ColumnType.Hyperlink;
        }
    }

    public class ImageColumn : Column
    {
        public string DefaultValue { get; set; } = string.Empty;

        public ImageColumn()
        {
            Type = ColumnType.Image;
        }


    }

    public class LookupColumn : Column
    {
        public Guid ListID { get; set; }
        public Guid ColumnID { get; set; }

        public LookupColumn()
        {
            Type = ColumnType.Lookup;
        }
    }

    public class AverageRatingColumn : Column
    {
        public List<double> Ratings { get; set; } = [];

        public AverageRatingColumn()
        {
            Type = ColumnType.AverageRating;
        }

        public double GetAverageRating()
        {
            return Ratings.Average();
        }
    }

    public class MultipleLinesOfTextColumn : Column
    {
        public string DefaultValue { get; set; } = string.Empty;

        public MultipleLinesOfTextColumn()
        {
            Type = ColumnType.MultipleLinesOfText;
        }
    }

    public class TextColumn : Column
    {
        public string DefaultValue { get; set; } = string.Empty;
        public bool CalculatedValue { get; set; } = false;
        public bool AtoZFilter { get; set; } = false;
        public bool ZtoAFilter { get; set; } = false;

        public TextColumn()
        {
            Type = ColumnType.Text;
        }
    }

    public class NumberColumn : Column
    {
        public double DefaultValue { get; set; } = 0.0;

        public NumberColumn()
        {
            Type = ColumnType.Number;
        }
    }

    public class ChoiceColumn : Column
    {
        public List<Choice> Choices { get; set; } =
        [
            new Choice { Name = "Choice 1", Color = Color.Blue },
            new Choice { Name = "Choice 2", Color = Color.Green },
            new Choice { Name = "Choice 3", Color = Color.Yellow }
        ];
        public string DefaultValue { get; set; } = string.Empty;
        public bool AddValuesManually { get; set; } = false;

        public ChoiceColumn()
        {
            Type = ColumnType.Choice;
        }
    }

    public class DateColumn : Column
    {
        public DateTime DefaultValue { get; set; } = DateTime.Now;

        public DateColumn()
        {
            Type = ColumnType.DateAndTime;
        }
    }


    public class Choice
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public Color Color { get; set; }
    }
}