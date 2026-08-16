using Inventory.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Api.Data.Configurations;

public sealed class StockOperationConfiguration : IEntityTypeConfiguration<StockOperation>
{
    public void Configure(EntityTypeBuilder<StockOperation> builder)
    {
        builder.ToTable("stock_operations");

        builder.HasKey(operation => operation.Id)
            .HasName("pk_stock_operations");

        builder.Property(operation => operation.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(operation => operation.OperationId)
            .HasColumnName("operation_id")
            .IsRequired();

        builder.HasIndex(operation => operation.OperationId)
            .IsUnique()
            .HasDatabaseName("ux_stock_operations_operation_id");

        builder.Property(operation => operation.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}
