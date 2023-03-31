using KachelService.Model;
using KachelService.Model.Customer;
using KachelService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KachelService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TilesController : ControllerBase
    {

        private readonly TileService _tileService;

        public TilesController(TileService tileService)
        {
            _tileService = tileService;
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(TileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("Tiles")]
        public async Task<ActionResult> GetTileResult([FromBody] TileRequest? request)
        {
            if (request == null)
            {
                return BadRequest("Request may not be empty");
            }

            var result = await _tileService.GetAction(request.Token, request.Actions);

            return Ok(result);
        }
    }
}
