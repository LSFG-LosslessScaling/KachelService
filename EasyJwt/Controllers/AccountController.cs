using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EasyJwt.Models;
using EasyJwt.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace EasyJwt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IUserService _userService;
        private readonly IJwtAuthManager _jwtAuthManager;

        public AccountController(ILogger<AccountController> logger, IUserService userService, IJwtAuthManager jwtAuthManager)
        {
            _logger = logger;
            _userService = userService;
            _jwtAuthManager = jwtAuthManager;
        }

        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResult>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (! await _userService.IsValidUserCredentials(request.UserName, request.Password))
            {
                return Unauthorized();
            }

            var user = await _userService.GetUser(request.UserName);

            var jwtResult = _jwtAuthManager.GenerateTokens(request.UserName, user.Claims.ToArray(), DateTime.Now);

            _logger.LogInformation($"User [{request.UserName}] logged in the system.");

            return Ok(new LoginResult
            {
                UserName = request.UserName,
                Role = user.Roles,
                AccessToken = jwtResult.AccessToken,
                RefreshToken = jwtResult.RefreshToken.TokenString
            });
        }

        [HttpGet("user")]
        [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
        [Authorize]
        public async Task<ActionResult<UserResult>> GetCurrentUser()
        {
            if (User.Identity != null)
            {
                var userName = User.Identity.Name;

                try
                {
                    if (!string.IsNullOrEmpty(userName))
                    {
                        var myUser = await _userService.GetUser(userName);

                        var result = new UserResult()
                        {
                            Name = User.Identity.Name,
                            Roles = myUser.Roles,
                            ExposedClaims = new List<ClaimDto>()
                        };

                        foreach (var myUserClaim in myUser.Claims)
                        {
                            result.ExposedClaims.Add(new ClaimDto(){ Type = myUserClaim.Type, Value = myUserClaim.Value});
                        }

                        return Ok(result);
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return BadRequest();

        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        public ActionResult Logout()
        {
            if (User.Identity != null)
            {
                var userName = User.Identity.Name;
                _jwtAuthManager.RemoveRefreshTokenByUserName(userName);
                _logger.LogInformation($"User [{userName}] logged out the system.");
            }

            return Ok();
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (User.Identity != null)
                {
                    var userName = User.Identity.Name;
                    _logger.LogInformation($"User [{userName}] is trying to refresh JWT token.");

                    if (string.IsNullOrWhiteSpace(request.RefreshToken))
                    {
                        return Unauthorized();
                    }

                    var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
                    var jwtResult = _jwtAuthManager.Refresh(request.RefreshToken, accessToken, DateTime.Now);
                    _logger.LogInformation($"User [{userName}] has refreshed JWT token.");
                    return Ok(new LoginResult
                    {
                        UserName = userName,
                        Role = User.FindAll(ClaimTypes.Role).Select(s=>s.Value).ToList(),
                        AccessToken = jwtResult.AccessToken,
                        RefreshToken = jwtResult.RefreshToken.TokenString
                    });
                }

                return BadRequest();
            }
            catch (SecurityTokenException e)
            {
                return Unauthorized();
            }
        }

        [HttpPost("impersonation")]
        [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult> Impersonate([FromBody] ImpersonationRequest request)
        {
            var userName = User.Identity.Name;
            _logger.LogInformation($"User [{userName}] is trying to impersonate [{request.UserName}].");

            var impersonatedRole = await _userService.GetUserRole(request.UserName);
            if (!impersonatedRole.Any())
            {
                _logger.LogInformation($"User [{userName}] failed to impersonate [{request.UserName}] due to the target user not found.");
                return BadRequest($"The target user [{request.UserName}] is not found.");
            }
            if (impersonatedRole.Contains(UserRoles.Admin))
            {
                _logger.LogInformation($"User [{userName}] is not allowed to impersonate another Admin.");
                return BadRequest("This action is not supported.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name,request.UserName),
                new Claim(ClaimTypes.Role, impersonatedRole.FirstOrDefault() ?? ""),
                new Claim("OriginalUserName", userName)
            };

            var jwtResult = _jwtAuthManager.GenerateTokens(request.UserName, claims, DateTime.Now);
            _logger.LogInformation($"User [{request.UserName}] is impersonating [{request.UserName}] in the system.");
            return Ok(new LoginResult
            {
                UserName = request.UserName,
                Role = impersonatedRole,
                OriginalUserName = userName,
                AccessToken = jwtResult.AccessToken,
                RefreshToken = jwtResult.RefreshToken.TokenString
            });
        }

        [HttpPost("stop-impersonation")]
        [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginResult>> StopImpersonation()
        {
            if (User.Identity != null)
            {
                var userName = User.Identity.Name;
                var originalUserName = User.FindFirst("OriginalUserName")?.Value;
                if (string.IsNullOrWhiteSpace(originalUserName))
                {
                    return BadRequest("You are not impersonating anyone.");
                }
                _logger.LogInformation($"User [{originalUserName}] is trying to stop impersonate [{userName}].");

                var role = await _userService.GetUserRole(originalUserName);
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name,originalUserName),
                    new Claim(ClaimTypes.Role, role.FirstOrDefault() ?? "")
                };

                var jwtResult = _jwtAuthManager.GenerateTokens(originalUserName, claims, DateTime.Now);
                _logger.LogInformation($"User [{originalUserName}] has stopped impersonation.");
                return Ok(new LoginResult
                {
                    UserName = originalUserName,
                    Role = role,
                    OriginalUserName = null,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken.TokenString
                });
            }

            return BadRequest();
        }
    }
}
