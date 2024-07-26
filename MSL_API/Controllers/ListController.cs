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
            // Assuming _listService is your list service instance
            var list = _listService.CreateBlankList(listName, description);

            // Load the existing lists (or create a new one if none exist)
            var savedLists = ListService.LoadLists() ?? [];

            // Add your new list to the existing (or new) list
            savedLists.Add(list);

            // Save the updated list of lists
            _listService.SaveLists(savedLists);

            return Ok(savedLists);
        }


        [HttpGet("allLists")]
        [ProducesResponseType(typeof(IEnumerable<List>), StatusCodes.Status200OK)]
        public IActionResult GetAllLists()
        {
            try
            {
                var savedLists = ListService.LoadLists();

                return Ok(savedLists);
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., file not found, invalid JSON, etc.)
                return BadRequest($"Error reading lists: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public IActionResult GetListById(Guid id)
        {
            var list = ListService.GetList(id);
            if (list == null)
                return NotFound();

            return Ok(list);
        }

        [HttpPatch("columns")]
        [ProducesResponseType(typeof(List), StatusCodes.Status200OK)]
        public IActionResult AddColumnToList(Guid id, [FromBody] ColumnRequest request)
        {
            var lists = ListService.LoadLists();
            var list = ListService.GetList(id);
            var col = ListService.CreateColumnFromRequest(request);

            ArgumentNullException.ThrowIfNull(list);
            list.AddCol(col);

            // Update lists with the modified list
            var indexToUpdate = lists.FindIndex(l => l.Id == id);
            lists[indexToUpdate] = list;

            _listService.SaveLists(lists);

            return Ok(lists);
        }

        [HttpPatch("rows")]
        [ProducesResponseType(typeof(List), StatusCodes.Status200OK)]
        public IActionResult AddRow(Guid listId, [FromBody] List<RowRequest> rowValues)
        {
            var lists = ListService.LoadLists();
            var list = ListService.GetList(listId);

            ArgumentNullException.ThrowIfNull(list);

            list.AddRow([.. rowValues]);

            var indexToUpdate = lists.FindIndex(l => l.Id == listId);
            lists[indexToUpdate] = list;

            _listService.SaveLists(lists);

            return Ok(lists); // Return the updated list

        }

        [HttpDelete("row")]
        [ProducesResponseType(typeof(List<List>), StatusCodes.Status200OK)]
        public IActionResult DeleteRow(Guid listId, Guid rowId)
        {
            var lists = ListService.LoadLists();
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
            var lists = ListService.LoadLists();
            var list = lists.Find(l => l.Id == listId);

            ArgumentNullException.ThrowIfNull(list);

            var col = list.Columns.Find(r => r.Id == colId);

            ArgumentNullException.ThrowIfNull(col);

            list.Columns.Remove(col);
            
            _listService.SaveLists(lists);

            return Ok(lists); // Return the updated list
        }

        [HttpDelete("{id}")]
        [ProducesResponseType<List<List>>(StatusCodes.Status200OK)]
        public IActionResult DeleteList(Guid id)
        {
            var lists = _listService.DeleteList(id);

            return Ok(lists);
        }

        [HttpDelete("all lists")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public IActionResult DeleteAllLists()
        {
            var savedLists = ListService.LoadLists();

            _listService.DeleteAll(savedLists);

            return Ok(savedLists);
        }

        [HttpPut("favor/{id}")]
        [ProducesResponseType<bool>(StatusCodes.Status200OK)]
        public IActionResult FavorList(Guid id)
        {
            _listService.FavorList(id);

            var list = ListService.GetList(id);
            ArgumentNullException.ThrowIfNull(list);
            return Ok(list.IsFavorited);
        }

    }

}
