using Billing.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Api.Data.Configurations;

public sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("invoice_items", tableBuilder =>
            tableBuilder.HasCheckConstraint(
                "ck_invoice_items_quantity_positive",
                "quantity > 0"));

        builder.HasKey(item => item.Id)
            .HasName("pk_invoice_items");

        builder.Property(item => item.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(item => item.InvoiceId)
            .HasColumnName("invoice_id")
            .IsRequired();

        builder.HasIndex(item => item.InvoiceId)
            .HasDatabaseName("ix_invoice_items_invoice_id");

        builder.Property(item => item.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(item => item.ProductCode)
            .HasColumnName("product_code")
            .HasMaxLength(InvoiceItem.MaxProductCodeLength)
            .IsRequired();

        builder.Property(item => item.ProductDescription)
            .HasColumnName("product_description")
            .HasMaxLength(InvoiceItem.MaxProductDescriptionLength)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .HasColumnName("quantity")
            .IsRequired();
    }
}
