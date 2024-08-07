using DataLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PresentationLayer.Controllers;
using System.Drawing;
using System.Globalization;

namespace TestMSL
{
    public class TestMsl
    {
        private readonly ControllerService _controller;
        private readonly Database _context;
            
        protected TestMsl(Database context, ControllerService controller)
        {
            _context = context;
            _controller = controller;
        }

        [Fact]
        public async Task SaveListToDatabase()
        {
            using var db = new Database();
            var list = new List
            {
                Id = Guid.NewGuid(),
                Name = "Test List",
                Description = "This is a test list",
                Color = Color.Blue.ToString(),
                Icon = "🌟"
            };

            list.AddCol(new TextColumn { Name = "Text" });
            list.AddCol(new NumberColumn { Name = "Number" });
            list.AddCol(new DateColumn { Name = "Date" });
            list.AddCol(new ChoiceColumn { Name = "Choice" });
            list.AddCol(new PersonColumn { Name = "Person" });
            list.AddCol(new YesNoColumn { Name = "Yes/No" });
            list.AddCol(new HyperlinkColumn { Name = "Hyperlink" });
            list.AddCol(new ImageColumn { Name = "Image" });
            list.AddCol(new LookupColumn { Name = "Lookup", ColumnID = Guid.NewGuid() });
            list.AddCol(new AverageRatingColumn { Name = "Average Rating" });
            list.AddCol(new MultipleLinesOfTextColumn { Name = "Multiple Lines" });

            // Add a row with data for each column type
            list.AddRow(
                "Sample Text",
                42.5,
                DateTime.Now,
                "Choice 1",
                "John Doe",
                true,
                ("https://example.com", "Example Link"),
                "image_url.jpg",
                "Lookup Value",
                4.5,
                "This is a\nmulti-line\ntext entry"
            );

            await db.SaveList(list);
            var savedList = await db.GetList(list.Id);

            Assert.NotNull(savedList);
            Assert.NotNull(savedList.Columns);
            Assert.NotNull(savedList.Rows);
            Assert.Equal(list.Name, savedList.Name);
            Assert.Equal(list.Description, savedList.Description);
            Assert.Equal(11, savedList.Columns.Count);
            Assert.Single(savedList.Rows);

            // Verify 
            Assert.IsType<TextColumn>(savedList.Columns[0]);
            Assert.IsType<NumberColumn>(savedList.Columns[1]);
            Assert.IsType<DateColumn>(savedList.Columns[2]);
            Assert.IsType<ChoiceColumn>(savedList.Columns[3]);
            Assert.IsType<PersonColumn>(savedList.Columns[4]);
            Assert.IsType<YesNoColumn>(savedList.Columns[5]);
            Assert.IsType<HyperlinkColumn>(savedList.Columns[6]);
            Assert.IsType<ImageColumn>(savedList.Columns[7]);
            Assert.IsType<LookupColumn>(savedList.Columns[8]);
            Assert.IsType<AverageRatingColumn>(savedList.Columns[9]);
            Assert.IsType<MultipleLinesOfTextColumn>(savedList.Columns[10]);

            // Verify data 
            var firstRow = savedList.Rows[0];
            Assert.Equal("Sample Text", firstRow.Cells[0].Value);
            Assert.Equal("42.5", firstRow.Cells[1].Value);
            Assert.True(DateTime.TryParse(firstRow.Cells[2].Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _));
            Assert.Equal("Choice 1", firstRow.Cells[3].Value);
            Assert.Equal("John Doe", firstRow.Cells[4].Value);
            Assert.Equal("True", firstRow.Cells[5].Value);
            Assert.Contains("https://example.com", firstRow.Cells[6].Value);
            Assert.Contains("Example Link", firstRow.Cells[6].Value);
            Assert.Equal("image_url.jpg", firstRow.Cells[7].Value);
            Assert.Equal("Lookup Value", firstRow.Cells[8].Value);
            Assert.Equal("4.5", firstRow.Cells[9].Value);
            Assert.Contains("This is a", firstRow.Cells[10].Value);
            Assert.Contains("multi-line", firstRow.Cells[10].Value);
            Assert.Contains("text entry", firstRow.Cells[10].Value);
        }

        [Fact]
        public async Task GetLists()
        {
            using var db = new Database();
            
            var lists = await db.GetLists();
            
            Assert.NotNull(lists);
        }

        [Fact]
        public async Task SaveListTemplate()
        {
            using var db = new Database();

            var template = new ListTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Test Template",
                Description = "A test template",
                Color = "#FF0000",
                Icon = "icon.png",
                PageSize = 20,
                Columns = new List<Column>
                {
                    new() {
                        Id = Guid.NewGuid(),
                        Name = "Test Column",
                        TypeId = 0,
                        Description = "A test column",
                        IsHidden = false,
                        Width = 100
                    }
                }
            };

            await db.SaveListTemplate(template);
            var savedTemplate = await db.GetList(template.Id);

            Assert.NotNull(savedTemplate);
        }

        [Fact]
        public async Task GetListTemplates()
        {
            using var db = new Database();

            var templates = await db.GetTemplates();

            Assert.NotNull(templates);
        }

        [Fact]
        public async Task Search()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var query = "test";
            var list = new List { Id = listId, Name = "Search Test List" };
            list.AddCol(new TextColumn { Name = "Text Column" });
            list.AddRow("This is a test row");
            _context.Lists.Add(list);
            await _context.SaveChangesAsync();

            // Act
            var result = _controller.Search(listId, query) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var searchResults = Assert.IsType<List<Row>>(result.Value);
            Assert.Single(searchResults);
            Assert.Contains("test", searchResults[0].Cells[0].Value);
        }

        [Fact]
        public async Task ExportToJson()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var list = new List { Id = listId, Name = "Export Test List" };
            list.AddCol(new TextColumn { Name = "Column1" });
            list.AddRow("Test Value");
            _context.Lists.Add(list);
            await _context.SaveChangesAsync();

            // Act
            var result = _controller.ExportToJson(listId) as FileContentResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("application/json", result.ContentType);
            Assert.Equal("list.json", result.FileDownloadName);

            var jsonContent = System.Text.Encoding.UTF8.GetString(result.FileContents);
            var exportedList = JsonConvert.DeserializeObject<List>(jsonContent);

            Assert.NotNull(exportedList);
            Assert.Equal(listId, exportedList.Id);
            Assert.Equal("Export Test List", exportedList.Name);
            Assert.Single(exportedList.Columns);
            Assert.Single(exportedList.Rows);
            Assert.Equal("Test Value", exportedList.Rows[0].Cells[0].Value);
        }

        [Fact]
        public async Task ImportFromCsv()
        {
            // Arrange
            var csvContent = "Column1,Column2,Column3\nValue1,Value2,Value3\nValue4,Value5,Value6";
            var file = new FormFile(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent)), 0, csvContent.Length, "Data", "test.csv");

            // Act
            var result = await _controller.ImportFromCsv(file) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var importedList = Assert.IsType<List>(result.Value);
            Assert.Equal(3, importedList.Columns.Count);
            Assert.Equal(2, importedList.Rows.Count);
            Assert.Equal("Value1", importedList.Rows[0].Cells[0].Value);
            Assert.Equal("Value6", importedList.Rows[1].Cells[2].Value);
        }

        [Fact]
        public async Task AddRow()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var list = new List { Id = listId };
            list.AddCol(new TextColumn { Name = "Column1" });
            list.AddCol(new TextColumn { Name = "Column2" });
            list.AddCol(new TextColumn { Name = "Column3" });
            _context.Lists.Add(list);
            await _context.SaveChangesAsync();

            var request = new List<string> { "Value1", "Value2", "Value3" };

            // Act
            var result = _controller.AddRow(listId, request) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var updatedList = Assert.IsType<List>(result.Value);
            Assert.NotNull(updatedList);
            Assert.Single(updatedList.Rows);
            Assert.Equal(3, updatedList.Rows[0].Cells.Count);
            Assert.Equal("Value1", updatedList.Rows[0].Cells[0].Value);
            Assert.Equal("Value2", updatedList.Rows[0].Cells[1].Value);
            Assert.Equal("Value3", updatedList.Rows[0].Cells[2].Value);
        }

        [Fact]
        public async Task NextPage()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var list = new List { Id = listId, PageSize = 2 };
            list.AddCol(new TextColumn { Name = "Column1" });
            for (int i = 0; i < 5; i++)
            {
                list.AddRow($"Value{i}");
            }
            _context.Lists.Add(list);
            await _context.SaveChangesAsync();

            // Act
            var result = _controller.NextPage(listId) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var updatedList = Assert.IsType<List>(result.Value);
            Assert.Equal(2, updatedList.CurrentPage);
            var currentPageRows = updatedList.GetCurrentPage();
            Assert.Equal(2, currentPageRows.Count);
            Assert.Equal("Value2", currentPageRows[0].Cells[0].Value);
        }

        [Fact]
        public async Task PreviousPage()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var list = new List { Id = listId, PageSize = 2, CurrentPage = 2 };
            list.AddCol(new TextColumn { Name = "Column1" });
            for (int i = 0; i < 5; i++)
            {
                list.AddRow($"Value{i}");
            }
            _context.Lists.Add(list);
            await _context.SaveChangesAsync();

            // Act
            var result = _controller.PreviousPage(listId) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var updatedList = Assert.IsType<List>(result.Value);
            Assert.Equal(1, updatedList.CurrentPage);
            var currentPageRows = updatedList.GetCurrentPage();
            Assert.Equal(2, currentPageRows.Count);
            Assert.Equal("Value0", currentPageRows[0].Cells[0].Value);
        }

        [Fact]
        public async Task UpdateListProperties()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var list = new List { Id = listId, Name = "Old Name", Description = "Old Description" };
            _context.Lists.Add(list);
            await _context.SaveChangesAsync();

            var request = new UpdateListRequest { Name = "New Name", Description = "New Description" };

            // Act
            var result = _controller.UpdateListProperties(listId, request) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var updatedList = Assert.IsType<List>(result.Value);
            Assert.Equal("New Name", updatedList.Name);
        }

        [Fact]
        public async Task UpdateCellValue()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var list = new List { Id = listId };
            var column = new TextColumn { Id = Guid.NewGuid(), Name = "Column1" };
            list.AddCol(column);
            list.AddRow("Old Value");
            var rowId = list.Rows[0].Id;
            _context.Lists.Add(list);
            await _context.SaveChangesAsync();

            var newValue = "New Value";

            // Act
            var result = _controller.UpdateCellValue(listId, rowId, column.Id, newValue) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var updatedList = Assert.IsType<List>(result.Value);
            Assert.NotNull(updatedList);
            Assert.Equal(newValue, updatedList.Rows[0].Cells[0].Value);
        }

        [Fact]
        public async Task BulkDeleteRows()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var list = new List { Id = listId };
            list.AddCol(new TextColumn { Name = "Column1" });
            list.AddRow("Row1");
            list.AddRow("Row2");
            list.AddRow("Row3");
            var rowIds = new List<Guid> { list.Rows[0].Id, list.Rows[1].Id };
            _context.Lists.Add(list);
            await _context.SaveChangesAsync();

            // Act
            var result = _controller.BulkDeleteRows(listId, rowIds) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var updatedList = Assert.IsType<List>(result.Value);
            Assert.NotNull(updatedList);
            Assert.Single(updatedList.Rows);
            Assert.Equal("Row3", updatedList.Rows[0].Cells[0].Value);
        }

        [Fact]
        public async Task DeleteRow()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var list = new List { Id = listId };
            list.AddCol(new TextColumn { Name = "Column1" });
            list.AddRow("Row1");
            list.AddRow("Row2");
            var rowIdToDelete = list.Rows[0].Id;
            _context.Lists.Add(list);
            await _context.SaveChangesAsync();

            // Act
            var result = _controller.DeleteRow(listId, rowIdToDelete) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var updatedLists = Assert.IsType<List<List>>(result.Value);
            Assert.NotNull(updatedLists);
            var updatedList = updatedLists.Find(l => l.Id == listId);
            Assert.NotNull(updatedList);
            Assert.Single(updatedList.Rows);
            Assert.Equal("Row2", updatedList.Rows[0].Cells[0].Value);
        }

        [Fact]
        public async Task DeleteColumn()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var list = new List { Id = listId };
            var column1 = new TextColumn { Id = Guid.NewGuid(), Name = "Column1" };
            var column2 = new TextColumn { Id = Guid.NewGuid(), Name = "Column2" };
            list.AddCol(column1);
            list.AddCol(column2);
            list.AddRow("Value1", "Value2");
            _context.Lists.Add(list);
            await _context.SaveChangesAsync();

            // Act
            var result = _controller.DeleteCol(listId, column1.Id) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var updatedLists = Assert.IsType<List<List>>(result.Value);
            Assert.NotNull(updatedLists);
            var updatedList = updatedLists.Find(l => l.Id == listId);
            Assert.NotNull(updatedList);
            Assert.Single(updatedList.Columns);
            Assert.Equal("Column2", updatedList.Columns[0].Name);
            Assert.Single(updatedList.Rows[0].Cells);
            Assert.Equal("Value2", updatedList.Rows[0].Cells[0].Value);
        }

        [Fact]
        public async Task DeleteList()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var list = new List { Id = listId };
            _context.Lists.Add(list);
            await _context.SaveChangesAsync();

            // Act
            var result = _controller.DeleteList(listId) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var updatedLists = Assert.IsType<List<List>>(result.Value);
            Assert.NotNull(updatedLists);
        }

        [Fact]
        public async Task DeleteAllLists()
        {
            // Arrange
            var list1 = new List { Id = Guid.NewGuid() };
            var list2 = new List { Id = Guid.NewGuid() };
            _context.Lists.AddRange(list1, list2);
            await _context.SaveChangesAsync();

            // Act
            var result = _controller.DeleteAllLists() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var updatedLists = Assert.IsType<List<List>>(result.Value);
            Assert.NotNull(updatedLists);
        }

        [Fact]
        public Task AddColumn()
        {
            // Arrange
            var listId = Guid.NewGuid();
            var request = new ColumnRequest { /* populate with valid data */ };

            // Act
            var result = _controller.AddColumn(listId, request) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var updatedList = Assert.IsType<List>(result.Value);
            Assert.NotNull(updatedList);
            return Task.CompletedTask;
        }
    }
}