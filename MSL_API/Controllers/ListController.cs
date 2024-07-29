using DataLayer;
using Microsoft.AspNetCore.Mvc;
using MsLServiceLayer;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListController(ListService listService) : ControllerBase
    {
        private readonly ListService _listService = listService;

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


        [HttpGet("allLists")]
        [ProducesResponseType(typeof(IEnumerable<List>), StatusCodes.Status200OK)]
        public IActionResult GetAllLists()
        {
            try
            {
                var lists = _listService.LoadLists();

                return Ok(lists);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error reading lists: {ex.Message}");
            }
        }

        [HttpGet("ListByID")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public IActionResult GetListById(Guid listId)
        {
            var list = _listService.GetList(listId);

            return Ok(list);
        }

        [HttpPatch("columns")]
        [ProducesResponseType(typeof(List), StatusCodes.Status200OK)]
        public IActionResult AddColumn(Guid listId, [FromBody] ColumnRequest request)
        {
            try
            {
                _listService.AddColumn(listId, request);
                var list = _listService.GetList(listId) ?? throw new ArgumentException("List not found");
                return Ok(list);
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
                var list = _listService.GetList(listId) ?? throw new ArgumentException("List not found");
                return Ok(list);
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
            var lists = _listService.LoadLists();
            var list = lists.Find(l => l.Id == listId);

            ArgumentNullException.ThrowIfNull(list);

            var row = list.Rows.Find(r => r.Id == rowId);

            ArgumentNullException.ThrowIfNull(row);

            list.Rows.Remove(row);

            // Save the modified lists
            _listService.SaveLists(lists);

            return Ok(lists); // Return the updated list
        }

        [HttpDelete("col")]
        [ProducesResponseType(typeof(List<List>), StatusCodes.Status200OK)]
        public IActionResult DeleteCol(Guid listId, Guid colId)
        {
            var lists = _listService.LoadLists();
            var list = lists.Find(l => l.Id == listId);

            ArgumentNullException.ThrowIfNull(list);

            var col = list.Columns.Find(r => r.Id == colId);

            ArgumentNullException.ThrowIfNull(col);

            list.Columns.Remove(col);

            _listService.SaveLists(lists);

            return Ok(lists); // Return the updated list
        }

        [HttpDelete("{listID}")]
        [ProducesResponseType<List<List>>(StatusCodes.Status200OK)]
        public IActionResult DeleteList(Guid listID)
        {
            var lists = _listService.DeleteList(listID);

            return Ok(lists);
        }

        [HttpDelete("lists")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public IActionResult DeleteAllLists()
        {
            var savedLists = _listService.LoadLists();

            _listService.DeleteAll(savedLists);

            return Ok(savedLists);
        }

        [HttpPut("favor/{listID}")]
        [ProducesResponseType<bool>(StatusCodes.Status200OK)]
        public IActionResult FavorList(Guid listID)
        {
            _listService.FavorList(listID);

            var list = _listService.GetList(listID);
            ArgumentNullException.ThrowIfNull(list);
            return Ok(list.IsFavorited);
        }

        [HttpPut("sortColAsc/{listId}/{colId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public IActionResult SortColumnAsc(Guid listId, Guid colId)
        {
            _listService.SortColumnAsc(listId, colId);
            var list = GetListById(listId);

            return Ok(list); // Return the sorted column
        }

        [HttpPut("sortColDes/{listId}/{colId}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public IActionResult SortColumnDes(Guid listId, Guid colId)
        {
            _listService.SortColumnDes(listId, colId);
            var list = GetListById(listId);

            return Ok(list); // Return the sorted column
        }

        [HttpGet("search/{listId}")]
        [ProducesResponseType(typeof(List<Row>), StatusCodes.Status200OK)]
        public IActionResult Search(Guid listId, [FromQuery] string query)
        {
            var result = _listService.SearchList(listId, query);
    
            return Ok(result);
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

        [HttpPost("nextPage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult NextPage(Guid listId)
        {
            var list = _listService.GetList(listId);
            ArgumentNullException.ThrowIfNull(list);

            list.NextPage();

            return Ok();
        }

        [HttpPost("previousPage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult PreviousPage(Guid listId)
        {
            var list = _listService.GetList(listId);
            ArgumentNullException.ThrowIfNull(list);

            list.PreviousPage();

            return Ok();
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
    }

}
