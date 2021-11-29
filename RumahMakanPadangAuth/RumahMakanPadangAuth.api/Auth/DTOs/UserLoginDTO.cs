using System.ComponentModel.DataAnnotations;

namespace RumahMakanPadangAuth.api.Auth.DTOs
{
    public class UserLoginDTO
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
