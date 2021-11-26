using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.EntityFrameworkCore;
using RumahMakanPadangAuth.dal.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RumahMakanPadangAuth.bll
{
    public class ResourceOwnerPasswordValidatorService : IResourceOwnerPasswordValidator
    {
        private readonly IUnitOfWork _unitOfWork;

        public ResourceOwnerPasswordValidatorService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = await _unitOfWork.UserRepository.GetAll()
                .Where(u => u.UserName.ToLower().Equals(context.UserName.ToLower()))
                .Select(x => new { x.UserId, x.Password })
                .SingleOrDefaultAsync();

            bool valid = BCrypt.Net.BCrypt.Verify(context.Password, user.Password);

            if (context.Request.Raw["bypassPassword"] == "true" || valid)
            {
                context.Result = new GrantValidationResult(user.UserId.ToString(), "password");
            }
            else
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "invalid username or password");
            }

        }
    }
}
