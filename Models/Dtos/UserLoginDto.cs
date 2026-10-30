
using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Models.Dtos
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "The field Username is required")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "The field Password is required")]
        public string? Password { get; set; }
    }
}