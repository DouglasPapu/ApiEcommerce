using ApiEcommerce.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; } //Representa una tabla en la base de datos
    public DbSet<Product> Products { get; set; } //Representa una tabla en la base de datos
    public DbSet<User> Users { get; set; } //Representa una tabla en la base de datos

}
