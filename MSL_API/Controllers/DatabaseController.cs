using DataLayer;
using Microsoft.AspNetCore.Mvc;
using MsLServiceLayer;

namespace PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController() : ControllerBase
    {
        private readonly ListService _listService = new();

        [HttpDelete("{listID}")]
        [ProducesResponseType<List<List>>(StatusCodes.Status200OK)]
        public IActionResult DeleteList(Guid listID)
        {
            _listService.DeleteList(listID);

            return GetAllLists().Result;
        }

        [HttpDelete("lists")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public IActionResult DeleteAllLists()
        {
            _listService.DeleteAll();

            return GetAllLists().Result;
        }

        [HttpGet("allLists")]
        [ProducesResponseType(typeof(IEnumerable<List>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllLists()
        {
            using var db = new Database();

            var lists = await db.GetLists();
            return Ok(lists);
        }

        [HttpGet("List")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public IActionResult GetListById(Guid listId)
        {
            using var db = new Database();
            var list = db.GetList(listId);

            ArgumentNullException.ThrowIfNull(list);
            return Ok(list);
        }

        [HttpGet("search/{templateId}")]
        [ProducesResponseType(typeof(List<Row>), StatusCodes.Status200OK)]
        public IActionResult Search(Guid listId, [FromQuery] string query)
        {
            using var db = new Database();
            var list = db.GetList(listId);

            ArgumentNullException.ThrowIfNull(list);

            var result = _listService.SearchList(listId, query);

            return Ok(result);
        }

        [HttpGet("template/{Id}")]
        [ProducesResponseType(typeof(List<Row>), StatusCodes.Status200OK)]
        public IActionResult GetTemplate(Guid templateId, [FromQuery] string query)
        {
            using var db = new Database();
            var template = db.GetListTemplate(templateId);

            ArgumentNullException.ThrowIfNull(template);

            var result = _listService.SearchList(templateId, query);

            return Ok(result);
        }

    }
}
