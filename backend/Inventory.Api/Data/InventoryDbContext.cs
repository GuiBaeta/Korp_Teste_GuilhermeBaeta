using Inventory.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");

            entity.HasKey(product => product.Id);

            entity.HasIndex(product => product.Code)
                .IsUnique();

            entity.Property(product => product.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(product => product.Description)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(product => product.StockQuantity)
                .IsRequired();

            entity.Property(product => product.CreatedAt)
                .IsRequired();

            entity.Property(product => product.UpdatedAt)
                .IsRequired();
        });
    }
}