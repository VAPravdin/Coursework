using Coursework.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.DataAccess.Configurations
{
    public class OrderServiceConfiguration : IEntityTypeConfiguration<OrderServiceEntity>
    {
        public void Configure(EntityTypeBuilder<OrderServiceEntity> builder)
        {
            builder.HasKey(x => new { x.OrderId, x.ServiceId });

            builder.Property(x => x.PriceAtPurchase)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder
                .HasOne(x => x.Order)
                .WithMany(x => x.OrderServices)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(x => x.Service)
                .WithMany(x => x.OrderServices)
                .HasForeignKey(x => x.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
