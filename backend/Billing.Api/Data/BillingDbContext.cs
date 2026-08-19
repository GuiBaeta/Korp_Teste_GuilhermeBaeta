using Billing.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Billing.Api.Data;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("invoices");

            entity.HasKey(invoice => invoice.Id);

            entity.Property(invoice => invoice.Number)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(invoice => invoice.Status)
                .IsRequired();

            entity.Property(invoice => invoice.CreatedAt)
                .IsRequired();

            entity.Property(invoice => invoice.ClosedAt);

            entity.HasMany(invoice => invoice.Items)
                .WithOne()
                .HasForeignKey(item => item.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.ToTable("invoice_items");

            entity.HasKey(item => item.Id);

            entity.Property(item => item.ProductId)
                .IsRequired();

            entity.Property(item => item.ProductCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(item => item.ProductDescription)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(item => item.Quantity)
                .IsRequired();
        });
    }
}