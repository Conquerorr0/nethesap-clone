using Microsoft.EntityFrameworkCore;
using Nethesap.Domain.Entities;
using System;
using System.IO;

namespace Nethesap.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        private static readonly string DefaultDbPath;

        static AppDbContext()
        {
            // Statik olarak veritabanı yolu belirle
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dbFolder = Path.Combine(appDataPath, "Nethesap");
            
            if (!Directory.Exists(dbFolder))
            {
                Directory.CreateDirectory(dbFolder);
            }
            
            DefaultDbPath = Path.Combine(dbFolder, "nethesap.db");
            Console.WriteLine($"Veritabanı yolu: {DefaultDbPath}");
        }
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        public AppDbContext() 
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentItem> PaymentItems { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                try
                {
                    // SQLite bağlantısını yapılandır
                    optionsBuilder.UseSqlite($"Data Source={DefaultDbPath}");
                    Console.WriteLine($"SQLite veritabanı yapılandırıldı");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Veritabanı yapılandırılırken hata oluştu: {ex.Message}");
                    Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                    throw;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product konfigürasyonu
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.Barcode).HasMaxLength(50);
                entity.Property(e => e.Category).HasMaxLength(100);
            });

            // Customer konfigürasyonu
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Balance).HasPrecision(18, 2);
            });

            // Payment konfigürasyonu
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // PaymentItem konfigürasyonu
            modelBuilder.Entity<PaymentItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            });

            // Transaction konfigürasyonu
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.TransactionDate).IsRequired();
            });

            // Global query filtreleri
            modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Payment>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<PaymentItem>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Transaction>().HasQueryFilter(e => !e.IsDeleted);
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseEntity)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }
                else if (entityEntry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                }
            }

            try
            {
                return base.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Veritabanı kayıt hatası: {ex.Message}");
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
} 