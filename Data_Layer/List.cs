using System.Drawing;

namespace DataLayer
{
    public class List
    {
        private static User GetAdmin()
        {
            var admin = new User()
            {
                Name = "Admin",
                IsOwner = true
            };
            return admin;
        }

        private readonly List<User> accessList = [];
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Column> Columns { get; set; } = [];
        public Color Color { get; set; } = new Color();
        public List<Row> Rows { get; set; } = [];
        public string Icon { get; set; } = string.Empty;
        public bool IsFavorited { get; set; } = false;
        public int PageSize { get; set; } = 2;
        public ViewType CurrentView { get; set; }
        public List<View> Views { get; set; } = [];
        public int CurrentPage { get; set; } = 1;
        public User Owner { get; private set; } = GetAdmin();

        public void AddCol<T>(T col) where T : Column
        {
            Columns.Add(col);
            col.ParentID = this.Id;

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
                var cell = new Cell
                {
                    ColumnType = column.Type,
                    Value = value
                };
                newRow.Cells.Add(cell);
                column.AddCellValue(value);
                index++;
            }

            Rows.Add(newRow);
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

        public void EditRow(Guid rowId, List<object> newValues)
        {
            var row = Rows.Find(r => r.Id == rowId) ?? throw new ArgumentException("Row not found.");
            row.UpdateCells(newValues);
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
