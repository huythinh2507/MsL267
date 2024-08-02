using System.Drawing;
using System.Globalization;
using CsvHelper;
using DataLayer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MsLServiceLayer
{
    public class ListService
    {
        public class ListTemplate : List
        {
            public ListTemplate()
            {
            }
        }

        private readonly List<List> _lists;
        private static readonly string[] value = ["Value"];
        private static readonly string[] valueArray = ["Value"];

        public ListService()
        {
            var path = MsLConstant.FilePath;
            ArgumentNullException.ThrowIfNull(path);


            _lists = [];
        }
        public List CreateBlankList(string listName, string description, Color color, string icon)
        {
            var list = new List
            {
                Id = Guid.NewGuid(),
                Name = listName,
                Description = description,
                Columns = [],
                Color = color,
                Icon = icon,
                Rows = []
            };

            return list;
        }

        public List CreateBlankList(string listName, string description)
        {

            return CreateBlankList(listName, description, Color.White, "🌟");
        }

        public List CreateFromExistingList(Guid existingID, string newName, string description = "", Color? color = null, string icon = "Smile")
        {
            var existingList = _lists.Find(l => l.Id == existingID);
            ArgumentNullException.ThrowIfNull(existingList);

            var newList = new List
            {
                Id = Guid.NewGuid(),
                Name = newName,
                Description = description,
                Columns = existingList.Columns,
                Color = color ?? Color.Transparent,
                Icon = icon,
                Rows = existingList.Rows
            };

            return newList;
        }
        public List<List> GetLists()
        {
            return _lists;
        }

        public List CreateListFromTemplate(List template)
        {
            var newList = new List
            {
                Id = Guid.NewGuid(),
                Name = template.Name,
                Description = template.Description,
                Columns = template.Columns,
                Color = template.Color,
                Icon = template.Icon,
                Rows = template.Rows,
            };

            _lists.Add(newList);

            return newList;
        }

        public List<List> DeleteList(Guid id)
        {
            var lists = LoadLists();
            var list = lists.Find(l => l.Id == id);

            ArgumentNullException.ThrowIfNull(list);

            lists.Remove(list);
            SaveLists(lists);
            return lists; // List successfully removed
        }

        public List? GetList(Guid id)
        {
            var lists = LoadLists();
            var list = lists.Find(l => l.Id == id);

            return list;
        }

        public bool FavorList(Guid id)
        {
            var lists = LoadLists();
            var list = lists.Find(l => l.Id == id);

            ArgumentNullException.ThrowIfNull(list);

            list.IsFavorited = true;
            SaveLists(lists);
            return true; // List successfully removed
        }

        public Form ToForm(List list)
        {
            var form = new Form()
            {
                Id = Guid.NewGuid(),
                Name = list.Name,
                Description = list.Description,
                Columns = list.Columns,
                Color = list.Color,
                Rows = list.Rows,
            };
            return form;
        }

        public List<List> LoadLists()
        {
            var existingData = File.Exists(MsLConstant.FilePath) ? File.ReadAllText(MsLConstant.FilePath) : "[]";
            var savedLists = JsonConvert.DeserializeObject<List<List>>(existingData) ?? new List<List>();


            return savedLists;
        }

        public void SaveLists(List<List> lists)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };
            var json = JsonConvert.SerializeObject(lists, Formatting.Indented, settings);
            File.WriteAllText(MsLConstant.FilePath, json);
        }

        public void DeleteAllLists()
        {
            var lists = LoadLists();
            lists.Clear();
        }

        private Column CreateColumnFromRequest(ColumnRequest request)
        {
            Column column = request.TypeId switch
            {
                ColumnType.Text => new TextColumn(),
                ColumnType.Number => new NumberColumn(),
                ColumnType.DateAndTime => new DateColumn(),
                ColumnType.Choice => new ChoiceColumn(),
                ColumnType.Person => new PersonColumn(),
                ColumnType.YesNo => new YesNoColumn(),
                ColumnType.Hyperlink => new HyperlinkColumn(),
                ColumnType.Image => new ImageColumn(),
                ColumnType.Lookup => new LookupColumn(),
                ColumnType.AverageRating => new AverageRatingColumn(),
                ColumnType.MultipleLinesOfText => new MultipleLinesOfTextColumn(),
                _ => throw new ArgumentException($"Unsupported column type: {request.TypeId}")
            };

            column.Name = request.Name;

            if (column is ChoiceColumn choiceColumn)
            {
                choiceColumn.Choices = new List<Choice>
                {
                    new() { Name = "Choice 1", Color = Color.Blue },
                    new() { Name = "Choice 2", Color = Color.Green },
                    new() { Name = "Choice 3", Color = Color.Yellow }
                };
            }

            if (column is YesNoColumn yesnoColumn)
            {
                yesnoColumn.DefaultValue = false;
            }

            return column;
        }


        public void DeleteAll(List<List> savedLists)
        {
            savedLists.Clear();
            SaveLists(savedLists);
        }

        public void SortColumnAsc(Guid listId, Guid colId)
        {
            var lists = LoadLists();
            var list = lists.Find(l => l.Id == listId);

            ArgumentNullException.ThrowIfNull(list);

            var column = list.Columns.Find(c => c.Id == colId);

            ArgumentNullException.ThrowIfNull(column);

            column.AtoZ();

            SaveLists(lists);
        }

        public List<Row> SearchList(Guid listId, string query)
        {
            var list = _lists.Find(l => l.Id == listId) ?? throw new ArgumentException("List not found.");
            return list.Search(query);
        }

        public List<List> AddColumn(Guid listId, ColumnRequest request)
        {
            var lists = LoadLists();
            var list = GetList(listId) ?? throw new ArgumentException("List not found");
            var col = CreateColumnFromRequest(request);
            list.AddCol(col);

            var indexToUpdate = lists.FindIndex(l => l.Id == listId);
            lists[indexToUpdate] = list;

            SaveLists(lists);

            return lists;
        }

        public void AddRow(Guid listId, params object[] values)
        {
            var lists = LoadLists();
            var list = GetList(listId);

            ArgumentNullException.ThrowIfNull(list);

            list.AddRow(values);

            var indexToUpdate = lists.FindIndex(l => l.Id == listId);
            lists[indexToUpdate] = list;

            SaveLists(lists);
        }

        public void SortColumnDes(Guid listId, Guid colId)
        {
            var lists = LoadLists();
            var list = lists.Find(l => l.Id == listId);

            ArgumentNullException.ThrowIfNull(list);

            var column = list.Columns.Find(c => c.Id == colId);

            ArgumentNullException.ThrowIfNull(column);

            column.ZtoA();

            SaveLists(lists);
        }

        public void DeleteColumn(Guid listId, Guid colId)
        {
            var lists = LoadLists();
            var list = lists.Find(l => l.Id == listId);

            ArgumentNullException.ThrowIfNull(list);

            var col = list.Columns.Find(r => r.Id == colId);

            ArgumentNullException.ThrowIfNull(col);

            list.Columns.Remove(col);

            SaveLists(lists);
        }

        public (byte[] FileContents, string FileName) ExportToJson(Guid listId)
        {
            var list = GetList(listId);
            ArgumentNullException.ThrowIfNull(list);

            var json = JsonConvert.SerializeObject(list);
            var fileName = $"{list.Name}_{DateTime.Now:yyyyMMddHHmmss}.json";
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

            File.WriteAllText(filePath, json);
            var fileBytes = File.ReadAllBytes(filePath);

            return (fileBytes, fileName);
        }

        public (byte[] FileContents, string FileName) ExportToCsv(Guid listId)
        {
            var list = GetList(listId);
            ArgumentNullException.ThrowIfNull(list);

            var fileName = $"{list.Name}_{DateTime.Now:yyyyMMddHHmmss}.csv";
            var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                // Write headers
                foreach (var column in list.Columns)
                {
                    csv.WriteField(column.Name);
                }
                csv.NextRecord();

                // Write data
                foreach (var row in list.Rows)
                {
                    foreach (var cell in row.Cells)
                    {
                        csv.WriteField(cell.Value);
                    }
                    csv.NextRecord();
                }
            }

            var fileBytes = File.ReadAllBytes(filePath);
            return (fileBytes, fileName);
        }

        public List DeleteRows(Guid listId, List<Guid> rowIds)
        {

            var lists = LoadLists();
            var list = lists.Find(l => l.Id == listId);

            ArgumentNullException.ThrowIfNull(list);

            list.Rows.RemoveAll(r => rowIds.Contains(r.Id));
            SaveLists(lists);

            return list;
        }

        public object UpdateListProperties(Guid listId, string newName, string newDescription)
        {
            var lists = LoadLists();
            var list = lists.Find(l => l.Id == listId);

            ArgumentNullException.ThrowIfNull(list);

            list.Name = newName;
            list.Description = newDescription;
            SaveLists(lists);

            return list;
        }

        public List UpdateCellValue(Guid listId, Guid rowId, Guid columnId, object newValue)
        {
            var lists = LoadLists();
            var list = GetList(listId) ?? throw new ArgumentException($"List with ID {listId} not found.");

            var column = list.Columns.Find(c => c.Id == columnId)
                ?? throw new ArgumentException($"Column with ID {columnId} not found in the list.");

            var row = list.Rows.Find(r => r.Id == rowId)
                ?? throw new ArgumentException($"Row with ID {rowId} not found in the list.");

            var cellIndex = list.Columns.IndexOf(column);
            if (cellIndex == -1 || cellIndex >= row.Cells.Count)
                throw new InvalidOperationException("Cell index out of range.");

            var cell = row.Cells[cellIndex];

            cell.Value = newValue;
            cell.ColumnType = column.TypeId;

            // Update the corresponding value in the column's Value list
            var rowIndex = list.Rows.IndexOf(row);
            if (rowIndex != -1 && rowIndex < column.Value.Count)
            {
                column.Value[rowIndex] = newValue;
            }
            else
            {
                column.Value.Add(newValue);
            }

            var indexToUpdate = lists.FindIndex(l => l.Id == listId);
            lists[indexToUpdate] = list;

            SaveLists(lists);
            return list;
        }

        public List ImportFromCsv(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<dynamic>().ToList();

            ArgumentNullException.ThrowIfNull(records);
            var list = new List
            {
                Name = "Imported List",
                Columns = records.FirstOrDefault() is IDictionary<string, object> firstRecord ? firstRecord.Keys.Select(k => new Column { Name = k }).ToList() : new List<Column>(),
                Rows = records.Select(r =>
                {
                    return new Row
                    {
                        Cells = r is IDictionary<string, object> recordAsDict ? recordAsDict.Values.Select(v => new Cell { Value = v.ToString() ?? new object() }).ToList() : new List<Cell>()
                    };
                }).ToList()
            };

            var lists = LoadLists();

            lists.Add(list);

            SaveLists(lists);
            return list;
        }

        public string ModifyLists(JArray jsonArray)
        {
            var propertiesToRemove = new Dictionary<ColumnType, string[]>
            {
                { ColumnType.Choice, valueArray },
                { ColumnType.YesNo, value }
            };

            var defaultPropertiesToRemove = new[] { "Choices", "AtoZFilter", "ZtoAFilter" };

            foreach (var item in jsonArray)
            {
                var columnsArray = item["Columns"] as JArray;
                columnsArray?.Select(column => column as JObject)
                             .Where(columnObject => columnObject != null)
                             .ToList()
                             .ForEach(columnObject =>
                             {
                                 ArgumentNullException.ThrowIfNull(columnObject);

                                 var typeId = columnObject["TypeId"]?.Value<int>() ?? 0;
                                 var columnType = Enum.IsDefined(typeof(ColumnType), typeId)
                                 ? (ColumnType)typeId
                                 : ColumnType.Text; 

                                 var toRemove = columnType switch
                                 {
                                     ColumnType.Text => ["Choices"],
                                     ColumnType.Choice => ["AtoZFilter", "ZtoAFilter"],
                                     _ => defaultPropertiesToRemove
                                 };

                                 if (propertiesToRemove.TryGetValue(columnType, out var additionalProperties))
                                 {
                                     toRemove = [.. toRemove, .. additionalProperties];
                                 }

                                 toRemove.ToList().ForEach(prop => columnObject.Remove(prop));
                             });
            }

            return jsonArray.ToString(Formatting.Indented);
        }

        public void DeleteRow(Guid listId, Guid rowId)
        {
            var lists = LoadLists();
            var list = lists.Find(l => l.Id == listId);

            ArgumentNullException.ThrowIfNull(list);

            var row = list.Rows.Find(r => r.Id == rowId);

            ArgumentNullException.ThrowIfNull(row);

            list.Rows.Remove(row);

            SaveLists(lists);
        }

        public void HideColumn(Guid listId, Guid colId)
        {
            var lists = LoadLists();
            var list = lists.Find(l => l.Id == listId);

            ArgumentNullException.ThrowIfNull(list);

            var col = list.Columns.Find(c => c.Id == colId);

            ArgumentNullException.ThrowIfNull(col);

            col.Hide();

            SaveLists(lists);
        }

        public void WidenColumn(Guid listId, Guid colId)
        {
            var lists = LoadLists();
            var list = lists.Find(l => l.Id == listId);

            ArgumentNullException.ThrowIfNull(list);

            var col = list.Columns.Find(c => c.Id == colId);

            ArgumentNullException.ThrowIfNull(col);

            col.Widen();

            SaveLists(lists);
        }

        public void NarrowColumn(Guid listId, Guid colId)
        {
            var lists = LoadLists();
            var list = lists.Find(l => l.Id == listId);

            ArgumentNullException.ThrowIfNull(list);

            var col = list.Columns.Find(c => c.Id == colId);

            ArgumentNullException.ThrowIfNull(col);

            col.Narrow();

            SaveLists(lists);
        }

        public void RenameColumn(Guid listId, Guid colId, string newName)
        {
            var lists = LoadLists();
            var list = lists.Find(l => l.Id == listId);

            ArgumentNullException.ThrowIfNull(list);

            var col = list.Columns.Find(c => c.Id == colId);

            ArgumentNullException.ThrowIfNull(col);

            col.Rename(newName);

            SaveLists(lists);
        }
    }
}
