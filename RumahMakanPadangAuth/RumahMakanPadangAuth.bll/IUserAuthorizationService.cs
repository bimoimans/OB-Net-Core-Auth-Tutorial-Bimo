using IdentityModel.Client;
using RumahMakanPadangAuth.dal.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RumahMakanPadangAuth.bll
{
    public interface IUserAuthorizationService
    {
        Task<TokenResponse> LoginAsync(string userName, string password, bool autopassword = true);
        Task<User> GetUserAsync(string userName);
        Guid GetUserId();
        string GetUserName();
        string GetEmail();
        List<string> GetRole();
    }
}
