using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Models.Dtos;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "The field {0} is required")]
    [MaxLength(50, ErrorMessage = "The field {0} must be a maximum of {1} characters long")]
    [MinLength(3, ErrorMessage = "The field {0} must be at least {1} characters long")]
    public string Name { get; set; } = string.Empty;
}
