using DataLayer;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MsLServiceLayer;
using PresentationLayer.Controllers;

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
      
    }
}