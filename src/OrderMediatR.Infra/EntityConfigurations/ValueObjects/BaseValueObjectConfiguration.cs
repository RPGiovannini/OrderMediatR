using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderMediatR.Domain.Entities;
using OrderMediatR.Domain.ValueObjects;

namespace OrderMediatR.Infra.EntityConfigurations.ValueObjects
{
    public static class BaseValueObjectConfiguration
    {
        public static void ConfigureEmail<T>(OwnedNavigationBuilder<T, Email> email) where T : class
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(255)
                .IsRequired();
        }

        public static void ConfigurePhone<T>(OwnedNavigationBuilder<T, Phone> phone) where T : class
        {
            phone.Property(p => p.Value)
                .HasColumnName("Phone")
                .HasMaxLength(20)
                .IsRequired();
        }

        public static void ConfigureMoney<T>(OwnedNavigationBuilder<T, Money> money, string propertyPrefix) where T : class
        {
            money.Property(m => m.Amount)
                .HasColumnName($"{propertyPrefix}")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName($"{propertyPrefix}Currency")
                .HasMaxLength(3)
                .IsRequired();
        }

        public static void ConfigureOptionalMoney<T>(OwnedNavigationBuilder<T, Money> money, string propertyPrefix) where T : class
        {
            money.Property(m => m.Amount)
                .HasColumnName($"{propertyPrefix}")
                .HasColumnType("decimal(18,2)");

            money.Property(m => m.Currency)
                .HasColumnName($"{propertyPrefix}Currency")
                .HasMaxLength(3);
        }

        public static void ConfigureSku<T>(OwnedNavigationBuilder<T, Sku> sku) where T : class
        {
            sku.Property(s => s.Value)
                .HasColumnName("Sku")
                .HasMaxLength(50)
                .IsRequired();
        }

        public static void ConfigureOrderNumber<T>(OwnedNavigationBuilder<T, OrderNumber> orderNumber) where T : class
        {
            orderNumber.Property(on => on.Value)
                .HasColumnName("OrderNumber")
                .HasMaxLength(50)
                .IsRequired();
        }
    }
}
