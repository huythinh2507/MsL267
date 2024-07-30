using DataLayer;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MsLServiceLayer;
using PresentationLayer.Controllers;
using System.Text;

namespace TestMSL
{
    public class TestMsl
    {
        [Fact]
        public void TestCreateList()
        {
            var mockListService = new Mock<ListService>();
            var controller = new ListController(mockListService.Object);

            // Create a sample list
            var list = (controller.CreateList("My List", "A sample description") as OkObjectResult)?.Value as List;

            // Assert
            Assert.NotNull(list);
            Assert.Equal("My List", list.Name);
            Assert.Equal("A sample description", list.Description);
        }

        [Fact]
        public void TestGetAllLists()
        {
            var mockListService = new Mock<ListService>();
            var controller = new ListController(mockListService.Object);

            // Create a sample list
            var lists = (controller.GetAllLists() as OkObjectResult)?.Value as List<List>;

            // Assert
            Assert.NotNull(lists);
        }

        

        [Fact]
        public void TestDeleteList()
        {
            // Arrange
            var mockListService = new Mock<ListService>();
            var controller = new ListController(mockListService.Object);

            var lists = (controller.GetAllLists() as OkObjectResult)?.Value as List<List>;

            ArgumentNullException.ThrowIfNull(lists);
            var list = lists.FirstOrDefault();

            ArgumentNullException.ThrowIfNull(list);

            controller.DeleteList(list.Id);

            Assert.NotNull(lists);
            Assert.DoesNotContain(list, lists);
        }

        [Fact]
        public void TestGetListById()
        {
            // Arrange
            var mockListService = new Mock<ListService>();
            var controller = new ListController(mockListService.Object);

            var lists = (controller.GetAllLists() as OkObjectResult)?.Value as List<List>;

            ArgumentNullException.ThrowIfNull(lists);
            var list = lists.FirstOrDefault();

            // Act
            Assert.NotNull(list);
            var result = (controller.GetListById(list.Id) as OkObjectResult)?.Value as List;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(list);
            Assert.Equal(list.Id, result.Id);
            Assert.Equal(list.Name, result.Name);
            Assert.Equal(list.Description, result.Description);
        }

        [Fact]
        public void TestAddCol()
        {
            // Arrange
            var mockListService = new Mock<ListService>();
            var controller = new ListController(mockListService.Object);

            var lists = (controller.GetAllLists() as OkObjectResult)?.Value as List<List>;

            ArgumentNullException.ThrowIfNull(lists);
            var list = lists.FirstOrDefault();

            var request = new ColumnRequest()
            {
                Name = "3h1",
                Type = 0
            };

            Assert.NotNull(list);
            controller.AddColumn(list.Id, request);
        }

        [Fact]
        public void TestDelCol()
        {
            // Arrange
            var mockListService = new Mock<ListService>();
            var controller = new ListController(mockListService.Object);

            var lists = (controller.GetAllLists() as OkObjectResult)?.Value as List<List>;

            ArgumentNullException.ThrowIfNull(lists);
            var list = lists.FirstOrDefault();

            ArgumentNullException.ThrowIfNull(list);
            var col = list.Columns[0];
            var colId = col.Id;
            controller.DeleteCol(list.Id, col.Id);
            var colID2 = list.Columns[0].Id;
            Assert.NotEqual(colId, colID2);
        }

        [Fact]
        public void TestAddRow()
        {
            var mockListService = new Mock<ListService>();
            var controller = new ListController(mockListService.Object);
            var listId = Guid.NewGuid();
            var rowData = new List<string> { "Value1", "Value2", "Value3" };

            var result = controller.AddRow(listId, rowData) as OkObjectResult;

            Assert.NotNull(result);
            Assert.IsType<List>(result.Value);
        }

        [Fact]
        public void TestDeleteRow()
        {
            var mockListService = new Mock<ListService>();
            var controller = new ListController(mockListService.Object);
            var listId = Guid.NewGuid();
            var rowId = Guid.NewGuid();

            var result = controller.DeleteRow(listId, rowId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.IsType<List<List>>(result.Value);
        }

        [Fact]
        public void TestDeleteAllLists()
        {
            var mockListService = new Mock<ListService>();
            var controller = new ListController(mockListService.Object);

            var result = controller.DeleteAllLists() as OkObjectResult;

            Assert.NotNull(result);
            Assert.IsType<List<List>>(result.Value);
        }

        [Fact]
        public void TestFavorList()
        {
            var mockListService = new Mock<ListService>();
            mockListService.Setup(s => s.GetList(It.IsAny<Guid>())).Returns(new List { IsFavorited = true });
            var controller = new ListController(mockListService.Object);
            var listId = Guid.NewGuid();

            var result = controller.FavorList(listId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.IsType<bool>(result.Value);
            Assert.True((bool)result.Value);
        }

        [Fact]
        public void TestSortColumnAsc()
        {
            var mockListService = new Mock<ListService>();
            var controller = new ListController(mockListService.Object);
            var listId = Guid.NewGuid();
            var colId = Guid.NewGuid();

            var result = controller.SortColumnAsc(listId, colId) as OkObjectResult;

            Assert.NotNull(result);
        }

        [Fact]
        public void TestSortColumnDes()
        {
            var mockListService = new Mock<ListService>();
            var controller = new ListController(mockListService.Object);
            var listId = Guid.NewGuid();
            var colId = Guid.NewGuid();

            var result = controller.SortColumnDes(listId, colId) as OkObjectResult;

            Assert.NotNull(result);
        }

        [Fact]
        public void TestSearch()
        {
            var mockListService = new Mock<ListService>();
            var controller = new ListController(mockListService.Object);
            var listId = Guid.NewGuid();
            var query = "test";

            var result = controller.Search(listId, query) as OkObjectResult;

            Assert.NotNull(result);
            Assert.IsType<List<Row>>(result.Value);
        }

        [Fact]
        public void TestGetCurrentPage()
        {
            var mockListService = new Mock<ListService>();
            mockListService.Setup(s => s.GetList(It.IsAny<Guid>())).Returns(new List());
            var controller = new ListController(mockListService.Object);
            var listId = Guid.NewGuid();

            var result = controller.GetCurrentPage(listId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.IsType<List<Row>>(result.Value);
        }

        [Fact]
        public void TestNextPage()
        {
            var mockListService = new Mock<ListService>();
            mockListService.Setup(s => s.GetList(It.IsAny<Guid>())).Returns(new List());
            var controller = new ListController(mockListService.Object);
            var listId = Guid.NewGuid();

            var result = controller.NextPage(listId) as OkResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public void TestPreviousPage()
        {
            var mockListService = new Mock<ListService>();
            mockListService.Setup(s => s.GetList(It.IsAny<Guid>())).Returns(new List());
            var controller = new ListController(mockListService.Object);
            var listId = Guid.NewGuid();

            var result = controller.PreviousPage(listId) as OkResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public void TestGetTotalPages()
        {
            var mockListService = new Mock<ListService>();
            mockListService.Setup(s => s.GetList(It.IsAny<Guid>())).Returns(new List());
            var controller = new ListController(mockListService.Object);
            var listId = Guid.NewGuid();

            var result = controller.GetTotalPages(listId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.IsType<int>(result.Value);
        }

        [Fact]
        public void TestExportToJson()
        {
            // Arrange
            var mockListService = new Mock<ListService>();
            var testListId = Guid.NewGuid();
            var testFileName = "TestList_20240730120000.json";
            var testFileContents = Encoding.UTF8.GetBytes("{\"name\":\"TestList\"}");

            mockListService.Setup(s => s.ExportToJson(testListId))
                .Returns((testFileContents, testFileName));

            var controller = new ListController(mockListService.Object);

            // Act
            var result = controller.ExportToJson(testListId) as FileContentResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("application/json", result.ContentType);
            Assert.Equal(testFileName, result.FileDownloadName);
            Assert.Equal(testFileContents, result.FileContents);
        }

        [Fact]
        public void TestExportToCsv()
        {
            // Arrange
            var mockListService = new Mock<ListService>();
            var testListId = Guid.NewGuid();
            var testFileName = "TestList_20240730120000.csv";
            var testFileContents = Encoding.UTF8.GetBytes("Column1,Column2\nValue1,Value2");

            mockListService.Setup(s => s.ExportToCsv(testListId))
                .Returns((testFileContents, testFileName));

            var controller = new ListController(mockListService.Object);

            // Act
            var result = controller.ExportToCsv(testListId) as FileContentResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("text/csv", result.ContentType);
            Assert.Equal(testFileName, result.FileDownloadName);
            Assert.Equal(testFileContents, result.FileContents);
        }
    }
}