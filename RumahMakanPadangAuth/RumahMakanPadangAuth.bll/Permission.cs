using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace RumahMakanPadangAuth.bll
{
    public class Permission : IAuthorizationRequirement
    {
        public string PermissionName { get; set; }

        public Permission(string permissionName)
        {
            PermissionName = permissionName;
        }
    }
}
