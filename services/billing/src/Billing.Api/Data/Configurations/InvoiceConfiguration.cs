using Billing.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Billing.Api.Data.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices", tableBuilder =>
            tableBuilder.HasCheckConstraint(
                "ck_invoices_status",
                "status IN ('Open', 'Closed')"));

        builder.HasKey(invoice => invoice.Id)
            .HasName("pk_invoices");

        builder.Property(invoice => invoice.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(invoice => invoice.Number)
            .HasColumnName("number")
            .UseIdentityByDefaultColumn()
            .ValueGeneratedOnAdd();

        builder.HasIndex(invoice => invoice.Number)
            .IsUnique()
            .HasDatabaseName("ux_invoices_number");

        builder.Property(invoice => invoice.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(invoice => invoice.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(invoice => invoice.ClosedAt)
            .HasColumnName("closed_at")
            .HasColumnType("timestamp with time zone");

        builder.HasMany(invoice => invoice.Items)
            .WithOne()
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_invoice_items_invoices_invoice_id");

        builder.Navigation(invoice => invoice.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
