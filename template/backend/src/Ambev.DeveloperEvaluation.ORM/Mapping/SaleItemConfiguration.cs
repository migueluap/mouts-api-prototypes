using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

/// <summary>
/// Entity Framework Core configuration for the SaleItem entity.
/// Defines table mapping, column types, relationships, and constraints.
/// </summary>
public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        // Table configuration
        builder.ToTable("SaleItems");

        // Primary key configuration
        builder.HasKey(si => si.Id);
        builder.Property(si => si.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        // Foreign key to Sale (aggregate root)
        builder.Property(si => si.SaleId)
            .IsRequired()
            .HasColumnType("uuid");

        // External Identity Pattern - Product reference
        builder.Property(si => si.ProductId)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(si => si.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        // Quantity configuration with business rule constraint
        builder.Property(si => si.Quantity)
            .IsRequired();

        // Add check constraint for quantity business rule (1-20)
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_SaleItems_Quantity",
            "\"Quantity\" >= 1 AND \"Quantity\" <= 20"));

        // Financial fields - using decimal(18,2) for monetary values
        builder.Property(si => si.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(si => si.Discount)
            .IsRequired()
            .HasColumnType("decimal(5,2)");

        builder.Property(si => si.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // Cancellation tracking
        builder.Property(si => si.Cancelled)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(si => si.CancelledAt)
            .HasColumnType("timestamp with time zone");

        // Relationship configuration is defined in SaleConfiguration

        // Indexes for common queries
        builder.HasIndex(si => si.SaleId);
        builder.HasIndex(si => si.ProductId);
        builder.HasIndex(si => si.Cancelled);
    }
}
