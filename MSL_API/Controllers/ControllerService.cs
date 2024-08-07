using Microsoft.AspNetCore.Mvc;
using DataLayer;
using MsLServiceLayer;
using Newtonsoft.Json.Linq;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControllerService(ListService listService) : ControllerBase
    {
        private readonly ListService _listService = listService;

        [HttpGet("search/{listId}")]
        [ProducesResponseType(typeof(List<Row>), StatusCodes.Status200OK)]
        public IActionResult Search(Guid listId, [FromQuery] string query)
        {
            using var db = new Database();
            var list = db.GetList(listId);

            ArgumentNullException.ThrowIfNull(list);

            var result = _listService.SearchList(listId, query);

            return Ok(result);
        }

        [HttpGet("allLists")]
        [ProducesResponseType(typeof(IEnumerable<List>), StatusCodes.Status200OK)]
        public IActionResult GetAllLists()
        {
            var lists = _listService.LoadLists();

            var result = _listService.ModifyLists(JArray.FromObject(lists));

            return Content(result, "application/json");
        }

        [HttpGet("ListByID")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public IActionResult GetListById(Guid listId)
        {
            var list = _listService.GetList(listId);

            ArgumentNullException.ThrowIfNull(list);
            var result = _listService.ModifyLists(JArray.FromObject(list));

            return Content(result, "application/json");
        }
        

        [HttpGet("currentPage")]
        [ProducesResponseType(typeof(List<Row>), StatusCodes.Status200OK)]
        public IActionResult GetCurrentPage(Guid listId)
        {
            var list = _listService.GetList(listId);
            ArgumentNullException.ThrowIfNull(list);

            var currentPageRows = list.GetCurrentPage();
            return Ok(currentPageRows);
        }

        [HttpGet("totalPages")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public IActionResult GetTotalPages(Guid listId)
        {
            var list = _listService.GetList(listId);
            ArgumentNullException.ThrowIfNull(list);

            var totalPages = list.GetTotalPages();
            return Ok(totalPages);
        }

        [HttpGet("export/json/{listId}")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public IActionResult ExportToJson(Guid listId)
        {
            try
            {
                var (fileContents, fileName) = _listService.ExportToJson(listId);
                return File(fileContents, "application/json", fileName);
            }
            catch (ArgumentNullException)
            {
                return NotFound($"List with ID {listId} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while exporting to JSON: {ex.Message}");
            }
        }

        [HttpGet("export/csv/{listId}")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public IActionResult ExportToCsv(Guid listId)
        {
            try
            {
                var (fileContents, fileName) = _listService.ExportToCsv(listId);
                return File(fileContents, "text/csv", fileName);
            }
            catch (ArgumentNullException)
            {
                return NotFound($"List with ID {listId} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while exporting to CSV: {ex.Message}");
            }
        }

        [HttpGet("nextPage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult NextPage(Guid listId)
        {
            var list = _listService.GetList(listId);
            ArgumentNullException.ThrowIfNull(list);

            list.NextPage();

            return Ok();
        }

        [HttpGet("previousPage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult PreviousPage(Guid listId)
        {
            var list = _listService.GetList(listId);
            ArgumentNullException.ThrowIfNull(list);

            list.PreviousPage();

            return Ok();
        }

        [HttpPost("createList")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public IActionResult CreateList(string listName, string description)
        {
            var list = _listService.CreateBlankList(listName, description);
            var lists = _listService.LoadLists();

            lists.Add(list);

            _listService.SaveLists(lists);
            return Ok(list);
        }

        [HttpPost("import/csv")]
        [ProducesResponseType(typeof(List), StatusCodes.Status200OK)]
        public Task<IActionResult> ImportFromCsv(IFormFile file)
        {
            ArgumentNullException.ThrowIfNull(file);
            ArgumentOutOfRangeException.ThrowIfZero(file.Length);

            try
            {
                var importedList = _listService.ImportFromCsv(file.OpenReadStream());
                return Task.FromResult<IActionResult>(Ok(importedList));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IActionResult>(StatusCode(500, $"An error occurred while importing from CSV: {ex.Message}"));
            }
        }

        [HttpPost("{colId}/hide")]
        public IActionResult HideColumn(Guid listId, Guid colId)
        {
            _listService.HideColumn(listId, colId);

            return GetListById(listId);
        }

        [HttpPost("{colId}/widen")]
        public IActionResult WidenColumn(Guid listId, Guid colId)
        {
            _listService.WidenColumn(listId, colId);
            
            return GetListById(listId);
        }

        [HttpPost("{colId}/narrow")]
        public IActionResult NarrowColumn(Guid listId, Guid colId)
        {
            _listService.NarrowColumn(listId, colId);
            
            return GetListById(listId);
        }

        [HttpPost("{colId}/rename")]
        public IActionResult RenameColumn(Guid listId, Guid colId, string newName)
        {
            _listService.RenameColumn(listId, colId, newName);
            
            return GetListById(listId);
        }

        [HttpPut("favor/{listID}")]
        [ProducesResponseType<bool>(StatusCodes.Status200OK)]
        public IActionResult FavorList(Guid listID)
        {
            _listService.FavorList(listID);

            return GetAllLists();
        }

        [HttpPut("sortColAsc/{listId}/{colId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public IActionResult SortColumnAsc(Guid listId, Guid colId)
        {
            _listService.SortColumnAsc(listId, colId);

            return GetListById(listId); // Return the sorted column
        }

        [HttpPut("sortColDes/{listId}/{colId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public IActionResult SortColumnDes(Guid listId, Guid colId)
        {
            _listService.SortColumnDes(listId, colId);

            return GetListById(listId); 
        }

        [HttpPut("update/{listId}")]
        [ProducesResponseType(typeof(List), StatusCodes.Status200OK)]
        public IActionResult UpdateListProperties(Guid listId, [FromBody] UpdateListRequest request)
        {
            try
            {
                var updatedList = _listService.UpdateListProperties(listId, request.Name, request.Description);
                return Ok(updatedList);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("updateCell")]
        [ProducesResponseType(typeof(List), StatusCodes.Status200OK)]
        public IActionResult UpdateCellValue(Guid listId, Guid rowId, Guid columnId, [FromBody] string newValue)
        {
            try
            {
                var updatedList = _listService.UpdateCellValue(listId, rowId, columnId, newValue);
                return Ok(updatedList);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("DeleteRows/{listId}")]
        [ProducesResponseType(typeof(List), StatusCodes.Status200OK)]
        public IActionResult BulkDeleteRows(Guid listId, [FromBody] List<Guid> rowIds)
        {
            try
            {
                var updatedList = _listService.DeleteRows(listId, rowIds);
                return Ok(updatedList);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("row")]
        [ProducesResponseType(typeof(List<List>), StatusCodes.Status200OK)]
        public IActionResult DeleteRow(Guid listId, Guid rowId)
        {
            _listService.DeleteRow(listId, rowId);

            return GetAllLists();
        }

        [HttpDelete("col")]
        [ProducesResponseType(typeof(List<List>), StatusCodes.Status200OK)]
        public IActionResult DeleteCol(Guid listId, Guid colId)
        {
            try
            {
                _listService.DeleteColumn(listId, colId);
                var updatedLists = _listService.LoadLists();
                return Ok(updatedLists);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{listID}")]
        [ProducesResponseType<List<List>>(StatusCodes.Status200OK)]
        public IActionResult DeleteList(Guid listID)
        {
            _listService.DeleteList(listID);

            return GetAllLists();
        }

        [HttpDelete("lists")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public IActionResult DeleteAllLists()
        {
            var savedLists = _listService.LoadLists();

            _listService.DeleteAll(savedLists);

            return GetAllLists();
        }

        [HttpPatch("columns")]
        [ProducesResponseType(typeof(List<List>), StatusCodes.Status200OK)]
        public IActionResult AddColumn(Guid listId, [FromBody] ColumnRequest request)
        {
            try
            {
                _listService.AddColumn(listId, request);

                return GetAllLists();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPatch("rows")]
        [ProducesResponseType(typeof(List), StatusCodes.Status200OK)]
        public IActionResult AddRow(Guid listId, [FromBody] List<string> request)
        {
            try
            {
                _listService.AddRow(listId, [.. request]);

                return GetAllLists();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

    }

}
