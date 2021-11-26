using AutoMapper;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RumahMakanPadangAuth.bll;
using RumahMakanPadangAuth.dal.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RumahMakanPadangAuth.api.Auth
{
    public class AuthController : ControllerBase
    {
        private readonly IUserAuthorizationService _authService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public AuthController(ILogger<AuthController> logger, IUserAuthorizationService authService)
        {
            _authService = authService;
            _logger = logger;

            MapperConfiguration configMapper = new MapperConfiguration(m =>
            {
                m.CreateMap<TokenResponse, UserTokenDTO>();
                m.CreateMap<User, UserInfoTokenDTO>();
            });

            _mapper = configMapper.CreateMapper();
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="user">Model of user login object.</param>
        /// <response code="200">Request successful.</response>
        /// <response code="400">Request failed because of an exception.</response>
        [ProducesResponseType(typeof(UserInfoTokenDTO), 200)]
        [HttpPost]
        [Route("Auth/login")]
        public async Task<IActionResult> Get([FromBody] UserLoginDTO userLoginDto)
        {
            try
            {
                TokenResponse userToken = await _authService.LoginAsync(userLoginDto.UserName, userLoginDto.Password);

                User user = await _authService.GetUserAsync(userLoginDto.UserName);

                UserInfoTokenDTO userInfoDto = _mapper.Map<User, UserInfoTokenDTO>(user);

                userInfoDto.TokenResponse = _mapper.Map<TokenResponse, UserTokenDTO>(userToken);

                return new OkObjectResult(userInfoDto);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="user">Model of user login object.</param>
        /// <response code="200">Request successful.</response>
        /// <response code="400">Request failed because of an exception.</response>
        [ProducesResponseType(typeof(UserInfoTokenDTO), 200)]
        [HttpGet]
        [Route("Auth/google")]
        public IActionResult LoginWithGoogle()
        {
            try
            {
                var props = new AuthenticationProperties
                {
                    RedirectUri = "/handle-google-login"
                };
                return Challenge(props, "Google");
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <response code="200">Request successful.</response>
        /// <response code="400">Request failed because of an exception.</response>
        [HttpGet]
        [Route("handle-google-login")]
        public async Task<IActionResult> RedirectAsync(string returnUrl = null, string remoteError = null)
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
                if (result?.Succeeded != true)
                {
                    throw new Exception("External authentication error");
                }

                // retrieve claims of the external user
                var externalUser = result.Principal;
                if (externalUser == null)
                {
                    throw new Exception("External authentication error");
                }

                // retrieve claims of the external user
                var claims = externalUser.Claims.ToList();

                var email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;

                TokenResponse userToken = await _authService.LoginAsync(email, "password");

                User user = await _authService.GetUserAsync(email);

                UserInfoTokenDTO userInfoDto = _mapper.Map<User, UserInfoTokenDTO>(user);

                userInfoDto.TokenResponse = _mapper.Map<TokenResponse, UserTokenDTO>(userToken);

                return new OkObjectResult(userInfoDto);

            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }



        /// <summary>
        /// Check authorization only
        /// </summary>
        /// <response code="200">Request successful.</response>
        /// <response code="400">Request failed because of an exception.</response>
        [ProducesResponseType(200)]
        [HttpGet]
        [Route("check")]
        [Authorize]
        public ActionResult CheckAuthRole()
        {
            return Ok();
        }

        /// <summary>
        /// Check authorization with role
        /// </summary>
        /// <response code="200">Request successful.</response>
        /// <response code="400">Request failed because of an exception.</response>
        [ProducesResponseType(200)]
        [HttpGet]
        [Route("check-with-role")]
        [AuthorizedByRole("Admin")]
        public ActionResult CheckAuthOnly()
        {
            return Ok();
        }

    }

}

