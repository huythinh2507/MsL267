using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;
using DataLayer;
using Newtonsoft.Json;
using static MsLServiceLayer.ListExporter;

namespace MsLServiceLayer
{
    public class ListService
    {
        private readonly string _filePath = MsLConstant.FilePath;

        public class ListTemplate : List
        {
            public ListTemplate()
            {
            }
        }

        private readonly List<List> _lists;

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
            var updatedData = JsonConvert.SerializeObject(lists);
            File.WriteAllText(_filePath, updatedData);
        }

        public void DeleteAllLists()
        {
            var lists = LoadLists();
            lists.Clear();
        }
        
        public Column CreateColumnFromRequest(ColumnRequest columnRequest)
        {
            var column = new Column()
            {
                Name = columnRequest.Name,
                Type = columnRequest.Type,
            };
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

        public void AddColumn(Guid listId, ColumnRequest request)
        {
            var lists = LoadLists();
            var list = GetList(listId) ?? throw new ArgumentException("List not found");
            var col = CreateColumnFromRequest(request);
            list.AddCol(col);

            var indexToUpdate = lists.FindIndex(l => l.Id == listId);
            lists[indexToUpdate] = list;

            SaveLists(lists);
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
    }
}
