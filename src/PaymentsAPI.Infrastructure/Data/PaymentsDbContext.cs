using Microsoft.EntityFrameworkCore;
using PaymentsAPI.Domain.Entities;

namespace PaymentsAPI.Infrastructure.Data;

public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");

            entity.HasKey(o => o.Id);

            entity.Property(o => o.UserId)
                .IsRequired();

            entity.Property(o => o.GameId)
                .IsRequired();

            entity.Property(o => o.GameTitle)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(o => o.Quantity)
                .IsRequired();

            entity.Property(o => o.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(o => o.TotalPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(o => o.Status)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(o => o.CreatedAt)
                .IsRequired();

            entity.Property(o => o.UpdatedAt);
            entity.HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(o => o.UserId);
            entity.HasIndex(o => o.Status);
            entity.HasIndex(o => o.CreatedAt);
        });
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.OrderId)
                .IsRequired();

            entity.Property(p => p.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(p => p.PaymentMethod)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.Status)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(p => p.CreatedAt)
                .IsRequired();

            entity.Property(p => p.ProcessedAt);

            entity.Property(p => p.FailureReason)
                .HasMaxLength(500);
            entity.HasIndex(p => p.OrderId)
                .IsUnique();
        });
    }
}
