using Microsoft.AspNetCore.Mvc;
using SongAppApi.Entities;
using SongAppApi.Models.Playlist;
using SongAppApi.Services;

namespace SongAppApi.Controllers
{
    [Authorization.Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PlaylistsController : BaseController
    {
        private IPlaylistService _service;

        public PlaylistsController(IPlaylistService service)
        {
            _service = service;
        }

        [HttpGet("{id:int}")]
        public ActionResult<PlaylistResponse> Get(int id)
        {
            var response = _service.Get(id);
            return Ok(response);
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlaylistResponse>> GetAll()
        {
            var response = _service.GetAll();
            return Ok(response);
        }

        [HttpGet("{accountid:int}")]
        public ActionResult<IEnumerable<PlaylistResponse>> GetAllByAccount(int accountid)
        {
            var response = _service.GetByAccount(accountid);
            return Ok(response);
        }

        [HttpPost("create-playlist")]
        public ActionResult<PlaylistResponse> CreatePlaylist(CreatePlaylistRequest request)
        {
            //todo test if it works
            var response = _service.Create(request, Account);
            return Ok(response);
        }

        [HttpPut("{id:int}")]
        public ActionResult<PlaylistResponse> UpdatePlaylist(int id, UpdatePlaylistRequest request)
        {
            var playlist = _service.Get(id);
            if (playlist.CreatedBy.Id != Account.Id || Account.Role != Role.Admin)
                return Unauthorized(new { message = "Unauthorized" });

            var response = _service.Update(id, request);
            return Ok(response);
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            var playlist = _service.Get(id);
            if (playlist.CreatedBy.Id != Account.Id || Account.Role != Role.Admin)
                return Unauthorized(new { message = "Unauthorized" });
            
            _service.Delete(id);
            return Ok(new { message = "Playlist successfully deleted" });
        }
    }
}
