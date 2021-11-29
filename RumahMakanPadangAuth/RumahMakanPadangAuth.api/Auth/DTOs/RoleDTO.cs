using System;

namespace RumahMakanPadangAuth.api.Auth.DTOs
{
    public class RoleDTO
    {
        public Guid? RoleId { get; set; }

        public string RoleCode { get; set; }

        public bool Status { get; set; }
    }
}
