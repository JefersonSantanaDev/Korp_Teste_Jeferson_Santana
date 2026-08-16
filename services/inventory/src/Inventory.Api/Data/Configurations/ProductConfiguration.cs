using Inventory.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Api.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", tableBuilder =>
            tableBuilder.HasCheckConstraint("ck_products_stock_non_negative", "stock >= 0"));

        builder.HasKey(product => product.Id)
            .HasName("pk_products");

        builder.Property(product => product.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(product => product.Code)
            .HasColumnName("code")
            .HasMaxLength(Product.MaxCodeLength)
            .IsRequired();

        builder.HasIndex(product => product.Code)
            .IsUnique()
            .HasDatabaseName("ux_products_code");

        builder.Property(product => product.Description)
            .HasColumnName("description")
            .HasMaxLength(Product.MaxDescriptionLength)
            .IsRequired();

        builder.Property(product => product.Stock)
            .HasColumnName("stock")
            .IsRequired();

        builder.Property(product => product.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(product => product.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}
