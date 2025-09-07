namespace SongAppApi.Controllers
{
    using Microsoft.AspNetCore.Identity.Data;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.StaticFiles;
    using SongAppApi.Authorization;
    using SongAppApi.Entities;
    using SongAppApi.Models.Accounts;
    using SongAppApi.Services;

    [Authorization.Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AccountsController : BaseController
    {
        private readonly IAccountService _accountService;
        private readonly IFileService _fileService;

        public AccountsController(IAccountService accountService, IFileService fileService)
        {
            _accountService = accountService;
            _fileService = fileService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public ActionResult<AuthenticateResponse> Authenticate(AuthenticateRequest model)
        {
            var response = _accountService.Authenticate(model, ipAddress());
            setTokenCookie(response.RefreshToken);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public ActionResult<AuthenticateResponse> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = _accountService.RefreshToken(refreshToken, ipAddress());
            setTokenCookie(response.RefreshToken);
            return Ok(response);
        }

        [HttpPost("revoke-token")]
        public IActionResult RevokeToken()
        {
            // accept token from request body or cookie
            var token = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            // users can revoke their own tokens and admins can revoke any tokens
            if (!Account.OwnsToken(token) && Account.Role != Role.Admin)
                return Unauthorized(new { message = "Unauthorized" });

            _accountService.RevokeToken(token, ipAddress());
            return Ok(new { message = "Token revoked" });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register(Models.Accounts.RegisterRequest model)
        {
            _accountService.Register(model, Request.Headers["origin"]);
            return Ok(new { message = "Registration successful, please check your email for verification instructions" });
        }

        [AllowAnonymous]
        [HttpPost("verify-email")]
        public IActionResult VerifyEmail(VerifyEmailRequest model)
        {
            _accountService.VerifyEmail(model.Token);
            return Ok(new { message = "Verification successful, you can now login" });
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword(Models.Accounts.ForgotPasswordRequest model)
        {
            _accountService.ForgotPassword(model, Request.Headers["origin"]);
            return Ok(new { message = "Please check your email for password reset instructions" });
        }

        [AllowAnonymous]
        [HttpPost("validate-reset-token")]
        public IActionResult ValidateResetToken(ValidateResetTokenRequest model)
        {
            _accountService.ValidateResetToken(model);
            return Ok(new { message = "Token is valid" });
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public IActionResult ResetPassword(Models.Accounts.ResetPasswordRequest model)
        {
            _accountService.ResetPassword(model);
            return Ok(new { message = "Password reset successful, you can now login" });
        }

        [Authorize(Role.Admin)]
        [HttpGet]
        public ActionResult<IEnumerable<AccountResponse>> GetAll()
        {
            var accounts = _accountService.GetAll();
            return Ok(accounts);
        }

        [HttpGet("{id:int}")]
        public ActionResult<AccountResponse> GetById(int id)
        {
            // users can get their own account and admins can get any account
            if (id != Account.Id && Account.Role != Role.Admin)
                return Unauthorized(new { message = "Unauthorized" });

            var account = _accountService.GetById(id);
            return Ok(account);
        }

        [AllowAnonymous]
        [HttpGet("profile/{id:int}")]
        public ActionResult<AccountProfileResponse> GetProfileById(int id)
        {
            var profile = _accountService.GetProfileById(id);
            return Ok(profile);
        }

        [AllowAnonymous]
        [HttpGet("profile/{id:int}/picture")]
        public ActionResult<AccountProfilePictureResponse> GetProfilePictureById(int id)
        {
            Console.WriteLine(id);
            //todo fix - currently getting null fileid from the
            // getprofilepictureid func
            // tried using automapper to map from profilepicture inside acc
            // to accountprofilepictureresp, aslo returned null
            // fml

            var fileid = _accountService.GetProfilePictureId(id);
            if (fileid == null)
            {
                Console.WriteLine("File ID IS NULL");
                return NotFound();
            }
            var profilepicture = _fileService.GetFileById((int)fileid);
            //Console.WriteLine(System.IO.File.Exists(profilepicture.FilePath));
            if (profilepicture == null || !System.IO.File.Exists(profilepicture.FilePath))
                return NotFound();

            var stream = new FileStream(profilepicture.FilePath, FileMode.Open, FileAccess.Read);
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(profilepicture.FileName, out var contentType))
                contentType = "application/octet-stream";
            return File(stream, contentType, profilepicture.FileName);
        }

        [Authorize(Role.Admin)]
        [HttpPost]
        public ActionResult<AccountResponse> Create(CreateRequest model)
        {
            var account = _accountService.Create(model);
            return Ok(account);
        }

        [HttpPut("{id:int}")]
        public ActionResult<AccountResponse> Update(int id, [FromForm] UpdateRequest model)
        {
            // users can update their own account and admins can update any account
            if (id != Account.Id && Account.Role != Role.Admin)
                return Unauthorized(new { message = "Unauthorized" });

            // only admins can update role
            if (Account.Role != Role.Admin)
                model.Role = null;

            if (model.ProfilePicture != null)
            {
                var fileid = _fileService.Create(model.ProfilePicture);
                var file = _fileService.GetFileById(fileid);
                var account = _accountService.Update(id, model, file);
                return Ok(account);
            }
            else
            {
                var account = _accountService.Update(id, model);
                return Ok(account);
            }
            //todo probably refactor

            //return Ok(account);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            // users can delete their own account and admins can delete any account
            if (id != Account.Id && Account.Role != Role.Admin)
                return Unauthorized(new { message = "Unauthorized" });

            _accountService.Delete(id);
            return Ok(new { message = "Account deleted successfully" });
        }

        // helper methods

        private void setTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        private string ipAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
