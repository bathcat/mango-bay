using System;
using MBC.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MBC.Persistence;

public class MBCDbContext : IdentityDbContext<MBCUser, IdentityRole<Guid>, Guid>
{
    public MBCDbContext(DbContextOptions<MBCDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Pilot> Pilots => Set<Pilot>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<DeliveryReview> DeliveryReviews => Set<DeliveryReview>();
    public DbSet<DeliveryProof> DeliveryProofs => Set<DeliveryProof>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.RegisterAllInVogenEfCoreConverters();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Pilot>(entity =>
        {
            entity.HasIndex(p => p.ShortName)
                .IsUnique();
        });

        modelBuilder.Entity<Site>(entity =>
        {
            entity.OwnsOne(s => s.Location);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(p => p.Amount)
                .HasPrecision(18, 2);

            entity.OwnsOne(p => p.CreditCard, cc =>
            {
                cc.Property(c => c.CardNumber)
                    .HasColumnType("char(16)")
                    .IsRequired();

                cc.Property(c => c.Cvc)
                    .HasColumnType("char(3)")
                    .IsRequired();

                cc.Property(c => c.CardholderName)
                    .HasColumnType("nvarchar(100)")
                    .IsRequired();
            });
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.OwnsOne(d => d.Details, details =>
            {
                details.Property(d => d.CargoWeightKg)
                    .HasPrecision(18, 2);
            });

            entity.HasOne(d => d.Payment)
                  .WithOne(p => p.Delivery)
                  .HasForeignKey<Payment>(p => p.DeliveryId);
        });

        modelBuilder.Entity<DeliveryProof>(entity =>
        {
            entity.HasIndex(p => p.DeliveryId)
                .IsUnique();

            entity.HasOne(p => p.Delivery)
                .WithOne(d => d.Proof)
                .HasForeignKey<DeliveryProof>(p => p.DeliveryId);

            entity.HasOne(p => p.Customer)
                .WithMany()
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(p => p.Pilot)
                .WithMany()
                .HasForeignKey(p => p.PilotId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<DeliveryReview>(entity =>
        {
            entity.HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(r => r.Pilot)
                .WithMany()
                .HasForeignKey(r => r.PilotId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(rt => rt.FamilyId);

            entity.HasIndex(rt => rt.TokenHash);

            entity.Property(rt => rt.TokenHash)
                .HasMaxLength(450)
                .IsRequired();

            entity.Property(rt => rt.Fingerprint)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(rt => rt.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
        });
    }
}

