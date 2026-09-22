using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

/// <summary>
/// Entity Framework Core configuration for the Sale entity.
/// Defines table mapping, column types, relationships, and constraints.
/// </summary>
public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        // Table configuration
        builder.ToTable("Sales");

        // Primary key configuration
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        // Sale number configuration - unique identifier for business use
        builder.Property(s => s.SaleNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.SaleNumber)
            .IsUnique();

        // Date configuration
        builder.Property(s => s.Date)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        // External Identity Pattern - Customer reference
        builder.Property(s => s.CustomerId)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(s => s.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        // External Identity Pattern - Branch reference
        builder.Property(s => s.BranchId)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(s => s.BranchName)
            .IsRequired()
            .HasMaxLength(200);

        // Financial fields - using decimal(18,2) for monetary values
        builder.Property(s => s.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // Cancellation tracking
        builder.Property(s => s.Cancelled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.CancelledAt)
            .HasColumnType("timestamp with time zone");

        // Audit fields
        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(s => s.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        // Navigation property configuration
        // Configure EF Core to use the backing field _items directly
        // Items property is read-only (no setter), so EF Core writes to the field
        builder.HasMany(s => s.Items)
            .WithOne(si => si.Sale)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Tell EF Core to access the collection through the backing field
        // This allows Items to be a read-only property while EF Core writes to _items
        builder.Navigation(s => s.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Indexes for common queries
        builder.HasIndex(s => s.CustomerId);
        builder.HasIndex(s => s.BranchId);
        builder.HasIndex(s => s.Date);
        builder.HasIndex(s => s.Cancelled);

        // Optimistic Concurrency Control
        // PostgreSQL: Map to xmin system column (concurrency token)
        builder.Property(s => s.RowVersion)
            .IsRowVersion()
            .HasColumnType("bytea")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}
