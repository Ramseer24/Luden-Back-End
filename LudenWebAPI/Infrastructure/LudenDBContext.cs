using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using File = Entities.Models.File;
using License = Entities.Models.License;

namespace Infrastructure
{

   
    public class LudenDbContext : DbContext
    {
        public DbSet<Region> Regions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<BillItem> BillItems { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<PhotoFile> PhotoFiles { get; set; }
        public DbSet<ProductFile> ProductFiles { get; set; }
        public DbSet<License> Licenses { get; set; }
        public DbSet<PaymentOrder> PaymentOrders { get; set; }
        public LudenDbContext(DbContextOptions<LudenDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Вызов базового метода (рекомендуется)

            // Настройка уникальных индексов (HasIndex + IsUnique)
            modelBuilder.Entity<Region>()
                .HasIndex(r => r.Code)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<License>()
                .HasIndex(l => l.LicenseKey)
                .IsUnique();

            // Настройка значений по умолчанию (HasDefaultValue)
            modelBuilder.Entity<User>()
                .Property(u => u.Role);

            modelBuilder.Entity<Bill>()
                .Property(b => b.Status);

            modelBuilder.Entity<License>()
                .Property(l => l.Status);

            // Настройка наследования для File (Table Per Hierarchy - TPH)
            modelBuilder.Entity<File>()
                .HasDiscriminator<string>("FileCategory")
                .HasValue<PhotoFile>("Photo")
                .HasValue<ProductFile>("ProductFile");

            // Настройка связи User -> PhotoFile (аватар)
            modelBuilder.Entity<User>()
                .HasOne(u => u.AvatarFile)
                .WithOne(pf => pf.User)
                .HasForeignKey<PhotoFile>(pf => pf.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Настройка связи Product -> ProductFile
            modelBuilder.Entity<ProductFile>()
                .HasOne(pf => pf.Product)
                .WithMany(p => p.Files)
                .HasForeignKey(pf => pf.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            // Bill → User
            modelBuilder.Entity<Bill>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bills)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // BillItem → Bill
            modelBuilder.Entity<BillItem>()
                .HasOne(bi => bi.Bill)
                .WithMany(b => b.BillItems)
                .HasForeignKey(bi => bi.BillId)
                .OnDelete(DeleteBehavior.Cascade);

            // BillItem → Product
            modelBuilder.Entity<BillItem>()
                .HasOne(bi => bi.Product)
                .WithMany(p => p.BillItems)
                .HasForeignKey(bi => bi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BillItem>()
                .HasOne(bi => bi.Product)
                .WithMany(p => p.BillItems)
                .HasForeignKey(bi => bi.ProductId);

            // License → Product
            modelBuilder.Entity<License>()
                .HasOne(l => l.Product)
                .WithMany(p => p.Licenses)
                .HasForeignKey(l => l.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product → Region
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Region)
                .WithMany(r => r.Products)
                .HasForeignKey(p => p.RegionId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
