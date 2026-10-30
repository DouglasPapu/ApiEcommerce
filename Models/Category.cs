using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Models;

public class Category
{
    [Key]  
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreationDate { get; set; }
}
