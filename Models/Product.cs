using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEcommerce.Models;

public class Product
{

    [Key]
    public int ProductId { get; set; }

    [Required]
    public string Name { get; set; } = "";
    
    public string Description { get; set; } = "";

    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
    [Column(TypeName = "decimal(18,2)")] //18 entires and 2 decimal places.
    public decimal Price { get; set; }

    public string? ImgUrl { get; set; }
    public string? ImgUrlLocal { get; set; }

    [Required]
    public string SKU { get; set; } = string.Empty; //PROD-001-BLK-M

    [Range(0, int.MaxValue, ErrorMessage = "Stock must be a positive value.")]
    public int Stock { get; set; }

    public DateTime CreationDate { get; set; } = DateTime.Now;

    public DateTime? UpdateDate { get; set; } = null;

    //Relation with Category model many to one
    public int CategoryId { get; set; }

    [ForeignKey("CategoryId")]
    public required Category Category { get; set; } = null!;    

}
