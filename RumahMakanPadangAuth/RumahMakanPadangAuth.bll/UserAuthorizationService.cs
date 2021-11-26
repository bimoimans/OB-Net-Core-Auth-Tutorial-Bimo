using IdentityModel.Client;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RumahMakanPadangAuth.dal.Models;
using RumahMakanPadangAuth.dal.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RumahMakanPadangAuth.bll
{
    public class UserAuthorizationService : IUserAuthorizationService
    {
        private readonly IConfiguration _config;
        private readonly IClientStore _clientStore;
        private readonly ClaimsPrincipal _userPrincipal;
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _httpClient;


        public UserAuthorizationService(IConfiguration config, IHttpContextAccessor httpContextAccessor,
            IClientStore clientStore, IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _clientStore = clientStore;
            _unitOfWork = unitOfWork;
            _userPrincipal = httpContextAccessor?.HttpContext?.User;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<User> GetUserAsync(string userName)
        {
            User user = await _unitOfWork.UserRepository.GetSingleAsync(u => u.UserName.Equals(userName));
            return user;
        }

        public async Task<TokenResponse> LoginAsync(string userName, string password, bool bypassPassword = true)
        {
            try
            {
                DiscoveryDocumentRequest discoReq = new DiscoveryDocumentRequest()
                {
                    Address = _config.GetValue<string>("AuthorizationServer:Address"),
                    Policy = new DiscoveryPolicy()
                    {
                        RequireHttps = false,
                        ValidateEndpoints = false,
                        ValidateIssuerName = false
                    }
                };

                DiscoveryDocumentResponse discoveryDocument = await _httpClient.GetDiscoveryDocumentAsync(discoReq);


                Client client = await _clientStore.FindEnabledClientByIdAsync(_config.GetValue<string>("Service:ClientId"));

                PasswordTokenRequest passwordTokenRequest = new PasswordTokenRequest()
                {

                    Address = discoveryDocument.TokenEndpoint,
                    ClientId = _config.GetValue<string>("Service:ClientId"),
                    ClientSecret = _config.GetValue<string>("Service:ClientSecret"),
                    GrantType = GrantTypes.ResourceOwnerPassword.First(),
                    Scope = client.AllowedScopes.Aggregate((p, n) => p + " " + n),
                    UserName = userName,
                    Password = password
                };
                if (bypassPassword)
                {
                    passwordTokenRequest.Parameters.Add("bypassPassword", "true");
                }


                TokenResponse tokenResponse = await _httpClient.RequestPasswordTokenAsync(passwordTokenRequest);

                if (tokenResponse.IsError)
                {
                    throw new Exception(tokenResponse.ErrorDescription);
                }

                return tokenResponse;
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }

        }

        public Guid GetUserId()
        {
            string userId = _userPrincipal.Claims.FirstOrDefault(i => i.Type == ClaimTypes.NameIdentifier)?.Value;
            return new Guid(userId);
        }

        public string GetUserName()
        {
            string userName = _userPrincipal.Claims.FirstOrDefault(i => i.Type == ClaimTypes.Name)?.Value;
            return userName;
        }

        public string GetEmail()
        {
            string userName = _userPrincipal.Claims.FirstOrDefault(i => i.Type == ClaimTypes.Email)?.Value;
            return userName;
        }

        public List<string> GetRole()
        {
            List<string> userName = _userPrincipal.Claims.Where(i => i.Type == ClaimTypes.Role).Select(r => r.Value).ToList();
            return userName;
        }

        Task<TokenResponse> IUserAuthorizationService.LoginAsync(string userName, string password, bool autopassword)
        {
            throw new NotImplementedException();
        }

        Task<User> IUserAuthorizationService.GetUserAsync(string userName)
        {
            throw new NotImplementedException();
        }
    }
}
