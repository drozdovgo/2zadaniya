using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WpfApp4.Models.Entities;

namespace WpfApp4.Domain
{
    public class MyDatabaseContext : DbContext
    {
        private readonly string connectionString;

        public DbSet<Пользователь> Пользователь { get; set; }
        public DbSet<Специализация> Специализация { get; set; }
        public DbSet<Врач> Врач { get; set; }
        public DbSet<Медицинская_карта> Медицинская_карта { get; set; }
        public DbSet<Запись> Запись { get; set; }
        public DbSet<Расписание> Расписание { get; set; }
        public DbSet<Отзыв> Отзыв { get; set; }

        public MyDatabaseContext(string connString)
        {
            connectionString = connString;

            System.Diagnostics.Debug.WriteLine($"=== MyDatabaseContext создан ===");

            try
            {
                Database.EnsureCreated();
                System.Diagnostics.Debug.WriteLine("✅ База данных создана/проверена");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка при создании базы данных: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=medicalclinic.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация таблиц
            modelBuilder.Entity<Пользователь>(entity =>
            {
                entity.ToTable("Пользователь");
                entity.HasKey(e => e.id);
                entity.HasIndex(e => e.email).IsUnique();
                entity.Property(e => e.роль).HasConversion<string>();
                entity.Property(e => e.дата_регистрации).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.активен).HasDefaultValue(true);
            });

            modelBuilder.Entity<Специализация>(entity =>
            {
                entity.ToTable("Специализация");
                entity.HasKey(e => e.id);
            });

            modelBuilder.Entity<Врач>(entity =>
            {
                entity.ToTable("Врач");
                entity.HasKey(e => e.id);
                entity.HasOne(e => e.Пользователь)
                      .WithOne()
                      .HasForeignKey<Врач>(e => e.пользователь_id);
                entity.HasOne(e => e.Специализация)
                      .WithMany()
                      .HasForeignKey(e => e.специализация_id);
                entity.Property(e => e.рейтинг).HasDefaultValue(0.00m);
                entity.Property(e => e.статус).HasConversion<string>().HasDefaultValue("активен");
            });

            modelBuilder.Entity<Медицинская_карта>(entity =>
            {
                entity.ToTable("Медицинская_карта");
                entity.HasKey(e => e.id);
                entity.HasOne(e => e.Пользователь)
                      .WithOne()
                      .HasForeignKey<Медицинская_карта>(e => e.пациент_id);
                entity.Property(e => e.дата_создания).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.дата_обновления).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<Запись>(entity =>
            {
                entity.ToTable("Запись");
                entity.HasKey(e => e.id);
                entity.HasOne(e => e.Пациент)
                      .WithMany()
                      .HasForeignKey(e => e.пациент_id);
                entity.HasOne(e => e.Врач)
                      .WithMany()
                      .HasForeignKey(e => e.врач_id);
                entity.Property(e => e.статус).HasConversion<string>().HasDefaultValue("запланирована");
                entity.Property(e => e.дата_создания).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.симптомы).HasDefaultValue("");
                entity.Property(e => e.диагноз).HasDefaultValue("");
                entity.Property(e => e.рекомендации).HasDefaultValue("");
            });

            modelBuilder.Entity<Расписание>(entity =>
            {
                entity.ToTable("Расписание");
                entity.HasKey(e => e.id);
                entity.HasOne(e => e.Врач)
                      .WithMany()
                      .HasForeignKey(e => e.врач_id);
                entity.Property(e => e.день_недели).HasConversion<string>();
                entity.Property(e => e.активен).HasDefaultValue(true);
            });

            modelBuilder.Entity<Отзыв>(entity =>
            {
                entity.ToTable("Отзыв");
                entity.HasKey(e => e.id);
                entity.HasOne(e => e.Запись)
                      .WithOne()
                      .HasForeignKey<Отзыв>(e => e.запись_id);
                entity.Property(e => e.дата_создания).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.одобрен).HasDefaultValue(false);
            });
        }
    }
}