using ApiEcommerce.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Category> Categories { get; set; } //Representa una tabla en la base de datos
    public DbSet<Product> Products { get; set; } //Representa una tabla en la base de datos
    public DbSet<User> Users { get; set; } //Representa una tabla en la base de datos
    public DbSet<ApplicationUser> ApplicationUser { get; set; }



}
