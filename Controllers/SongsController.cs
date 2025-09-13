using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SongAppApi.Entities;
using SongAppApi.Models.Songs;
using SongAppApi.Services;

namespace SongAppApi.Controllers
{
    [Authorization.Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SongsController : BaseController
    {
        ISongService _service;
        IAccountService _accountService;

        public SongsController(ISongService service, IAccountService accountService)
        {
            _service = service;
            _accountService = accountService;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult<IEnumerable<SongResponse>> GetAll()
        {
            var response = _service.GetAll();
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public ActionResult<SongResponse> Get(int id)
        {
            var response = _service.Get(id);
            return Ok(response);
        }

        [HttpPost("create-song")]
        public ActionResult<SongResponse> Create(CreateSongRequest request)
        {
            //todo test if it works
            var response = _service.Create(request, Account);
            return Ok(response);
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            var song = _service.Get(id);

            //todo check if createdby is properly fetched ie not always null
            if (song.CreatedBy.Id != Account.Id || Account.Role != Role.Admin)
                return Unauthorized(new { message = "Unauthorized" });

            _service.Delete(id);
            return Ok(new { message = "Song deleted successfully" });
        }

        [HttpPost("fliplike/{id:int}")]
        public ActionResult FlipLike(int id)
        {
            _service.FlipLike(id, Account);
            return Ok(new { message = "Song successfully liked/unliked" });
        }
    }
}
