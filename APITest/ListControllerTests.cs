using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using MsLServiceLayer;
using System.Text.Json;
using System.Drawing;
using System.Net.Http.Json;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using DataLayer;

namespace APITest
{
    public class ListControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task CreateBlankList_ReturnsOkResult()
        {
            var listName = "Test List";
            var description = "Test Description";
            var color = Color.White;
            var icon = "🌟";

            var response = await _client.PostAsJsonAsync("/api/list/create", new
            {
                listName,
                description,
                color,
                icon
            });

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var list = JsonSerializer.Deserialize<List>(responseString);

            Assert.NotNull(list);
            Assert.Equal(listName, list.Name);
        }

        [Fact]
        public async Task GetList_ReturnsNotFound_ForInvalidId()
        {
            var response = await _client.GetAsync($"/api/list/{Guid.NewGuid()}");
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteList_ReturnsNoContent()
        {
            // First, create a list
            var listName = "Test List";
            var description = "Test Description";
            var color = Color.White;
            var icon = "🌟";

            var createResponse = await _client.PostAsJsonAsync("/api/list/create", new
            {
                listName,
                description,
                color,
                icon
            });

            createResponse.EnsureSuccessStatusCode();

            var responseString = await createResponse.Content.ReadAsStringAsync();
            var list = JsonSerializer.Deserialize<List>(responseString);

            // Now, delete the list
            var deleteResponse = await _client.DeleteAsync($"/api/list/{list.Id}");

            Assert.Equal(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }
    }
}