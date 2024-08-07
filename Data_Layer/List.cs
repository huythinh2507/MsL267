using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Globalization;

namespace DataLayer
{
    public class List
    {
        private readonly List<User> accessList = [];
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Column> Columns { get; set; } = new List<Column>();
        public String Color { get; set; } = string.Empty;
        public List<Row> Rows { get; set; } = new List<Row>();
        public string Icon { get; set; } = string.Empty;
        public bool IsFavorited { get; set; } = false;
        public int PageSize { get; set; } = 2;
        public int CurrentPage { get; set; } = 1;
        public void AddCol<T>(T col) where T : Column
        {
            Columns.Add(col);
            col.ListID = this.Id;

            // Update existing rows to match the new column count
            Rows.ForEach(row =>
            {
                var cellsToAdd = Columns.Count - row.Cells.Count;
                row.Cells.AddRange(Enumerable.Repeat(new Cell(), cellsToAdd));
            });
        }

        public void AddRow(params object[] values)
        {
            if (values.Length != Columns.Count)
            {
                throw new ArgumentException("Number of values must match the number of columns.");
            }
            var newRow = new Row();
            int index = 0;
            foreach (var value in values)
            {
                var column = Columns[index];
                if (!IsValidTypeForColumn(column, value))
                {
                    throw new ArgumentException($"Invalid type for column '{column.Name}'. Expected {GetExpectedType(column)}, but got {value?.GetType().Name ?? "null"}.");
                }
                string cellValue = ConvertToCellValue(column, value);

                var cell = new Cell
                {
                    ColumnType = column.TypeId,
                    Value = cellValue,
                };
                newRow.Cells.Add(cell);
                column.AddCellValue(cellValue);
                index++;
            }
            Rows.Add(newRow);
        }

        private string ConvertToCellValue(Column column, object value)
        {
            return column switch
            {
                HyperlinkColumn when value is ValueTuple<string, string> link =>
                    JsonConvert.SerializeObject(new { Url = link.Item1, Text = link.Item2 }),
                DateColumn when value is DateTime date =>
                    date.ToString("o"),  
                NumberColumn when value is double number =>
                    number.ToString(CultureInfo.InvariantCulture),
                _ => value?.ToString() ?? string.Empty
            };
        }

        private bool IsValidTypeForColumn(Column column, object value)
        {
            return column switch
            {
                TextColumn or PersonColumn or ImageColumn or MultipleLinesOfTextColumn => value is string or null,
                NumberColumn => value is double or int or float or decimal or null,
                DateColumn => value is DateTime or null,
                YesNoColumn => value is bool or null,
                HyperlinkColumn => value is ValueTuple<string, string> or null,
                ChoiceColumn => value is string or null, // Assuming choices are stored as strings
                AverageRatingColumn => value is double or null,
                _ => true // For any other column types, assume it's valid
            };
        }

        private string GetExpectedType(Column column)
        {
            return column switch
            {
                TextColumn or PersonColumn or ImageColumn or MultipleLinesOfTextColumn => "string",
                NumberColumn => "number",
                DateColumn => "DateTime",
                YesNoColumn => "bool",
                HyperlinkColumn => "ValueTuple<string, string>",
                ChoiceColumn => "string",
                AverageRatingColumn => "double",
                _ => "unknown"
            };
        }

        public void MoveColumnLeft(int index)
        {
            // Ensure index is within valid bounds
            if (index <= 0 || index >= Columns.Count) return;

            // Swap columns
            (Columns[index - 1], Columns[index]) = (Columns[index], Columns[index - 1]);

            // Update rows
            Rows.ForEach(row =>
            {
                (row.Cells[index - 1], row.Cells[index]) = (row.Cells[index], row.Cells[index - 1]);
            });
        }

        public void MoveColumnRight(int index)
        {
            // Ensure index is within valid bounds
            index = Math.Max(index, 0);
            index = Math.Min(index, Columns.Count - 2);

            // Swap columns
            (Columns[index + 1], Columns[index]) = (Columns[index], Columns[index + 1]);

            // Update rows
            Rows.ForEach(row =>
            {
                (row.Cells[index + 1], row.Cells[index]) = (row.Cells[index], row.Cells[index + 1]);
            });
        }

        public List<Row> Search(string query)
        {
            return Rows.Where(row => row.Cells.Exists(cell =>
            {
                var cellValue = cell.Value?.ToString();
                return cellValue != null && cellValue.Contains(query, StringComparison.OrdinalIgnoreCase);
            })).ToList();
        }

        public List<Row> GetCurrentPage()
        {
            return Rows.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
        }

        // Method to move to the next page
        public void NextPage()
        {
            if (CurrentPage * PageSize < Rows.Count)
            {
                CurrentPage++;
            }
        }

        // Method to move to the previous page
        public void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        }

        // Method to get total pages
        public int GetTotalPages()
        {
            return (int)Math.Ceiling((double)Rows.Count / PageSize);
        }

        public List<Row> FilterByColumn(string columnName, Func<object, bool> predicate)
        {
            var column = Columns.Find(col => col.Name.Equals(columnName)) ?? throw new ArgumentException($"Column {columnName} does not exist.");
            int columnIndex = Columns.IndexOf(column);
            return Rows.Where(row => predicate(row.Cells[columnIndex].Value)).ToList();
        }

        public void Delete(Row row)
        {
            Rows.Remove(row);
        }

        public void EditRow(Guid rowId, List<string> newValues)
        {
            var row = Rows.Find(r => r.Id == rowId) ?? throw new ArgumentException("Row not found.");

            if (newValues.Count != Columns.Count)
            {
                throw new ArgumentException($"Number of new values ({newValues.Count}) does not match the number of columns ({Columns.Count}).");
            }

            for (int i = 0; i < newValues.Count; i++)
            {
                var column = Columns[i];
                var newValue = newValues[i];

                if (!IsValidValueForColumn(column, newValue))
                {
                    throw new ArgumentException($"Invalid value '{newValue}' for column '{column.Name}'. Expected {GetExpectedType(column)}.");
                }
            }

            row.UpdateCells(newValues);
        }

        private bool IsValidValueForColumn(Column column, string value)
        {
            return column switch
            {
                TextColumn or PersonColumn or ImageColumn or MultipleLinesOfTextColumn => true, 
                NumberColumn => double.TryParse(value, out _),
                DateColumn => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
                YesNoColumn => bool.TryParse(value, out _),
                HyperlinkColumn => Uri.TryCreate(value, UriKind.Absolute, out _), 
                ChoiceColumn => column.Choices?.Exists(c => c.Name == value) ?? false, 
                AverageRatingColumn => double.TryParse(value, out var rating) && rating >= 0 && rating <= 5, 
                _ => true 
            };
        }

        public void AddAccess(User user)
        {
            if (accessList.Contains(user))
            {
                return;
            }
            accessList.Add(user);
        }

        public bool HasAccess(User user)
        {
            return accessList.Contains(user);
        }

        public void RemoveAccess(User user)
        {
            accessList.Remove(user);
        }

        public List<User> GetUsers()
        {
            return accessList;
        }

        
    }
}
