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

        private readonly List<List> _templates;
        private readonly List<List> _lists;

        public ListService()
        {
            var path = MsLConstant.FilePath;
            ArgumentNullException.ThrowIfNull(path);

            _templates = LoadTemplatesFromJson(path) ?? [];
            ArgumentNullException.ThrowIfNull(_templates);

            _lists = [];
        }

        public static List<List> LoadTemplatesFromJson(string filePath)
        {
            var json = System.IO.File.ReadAllText(MsLConstant.FilePath);

            // Deserialize the JSON data into a list of ListModel
            var savedLists = JsonConvert.DeserializeObject<List<List>>(json);
            ArgumentNullException.ThrowIfNull(savedLists);
            return savedLists;
        }

        public List CreateBlankList(string listName, string description, Color color, string icon)
        {
            var newList = new List
            {
                Id = Guid.NewGuid(),
                Name = listName,
                Description = description,
                Columns = [],
                Color = color,
                Icon = icon,
                Rows = []
            };
            _lists.Add(newList);

            return newList;
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

            _lists.Add(newList);

            return newList;
        }

        public List<List> GetTemplate()
        {
            return _templates;
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

        public static List? GetList(Guid id)
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

        public static Form ToForm(List list)
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

        public static List<List> LoadLists()
        {
            var existingData = System.IO.File.Exists(MsLConstant.FilePath) ? System.IO.File.ReadAllText(MsLConstant.FilePath) : "[]";
            var savedLists = JsonConvert.DeserializeObject<List<List>>(existingData);
            ArgumentNullException.ThrowIfNull(savedLists);
            return savedLists;
        }

        public void SaveLists(List<List> lists)
        {
            var updatedData = JsonConvert.SerializeObject(lists);
            System.IO.File.WriteAllText(_filePath, updatedData);
        }

        public static void DeleteAllLists()
        {
            var lists = LoadLists();
            lists.Clear();
        }

        public static Column CreateColumnFromRequest(ColumnRequest columnRequest)
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
        
    }
}
