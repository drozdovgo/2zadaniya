using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using WpfApp4.Models.Entities;

namespace WpfApp4.Domain
{
    public class MyDatabaseContext : DbContext
    {
        public DbSet<Пользователь> Пользователь { get; set; }
        public DbSet<Специализация> Специализация { get; set; }
        public DbSet<Врач> Врач { get; set; }
        public DbSet<Медицинская_карта> Медицинская_карта { get; set; }
        public DbSet<Запись> Запись { get; set; }
        public DbSet<Расписание> Расписание { get; set; }
        public DbSet<Отзыв> Отзыв { get; set; }

        private readonly string _connectionString;

        public MyDatabaseContext(string connectionString)
        {
            _connectionString = connectionString;
            Database.EnsureCreated();
            SeedTestData();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlite(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
                      .HasForeignKey<Врач>(e => e.пользователь_id)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Специализация)
                      .WithMany()
                      .HasForeignKey(e => e.специализация_id)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.Property(e => e.рейтинг).HasDefaultValue(0.00m);
                entity.Property(e => e.статус).HasConversion<string>().HasDefaultValue("активен");
            });

            modelBuilder.Entity<Медицинская_карта>(entity =>
            {
                entity.ToTable("Медицинская_карта");
                entity.HasKey(e => e.id);
                entity.HasOne(e => e.Пользователь)
                      .WithOne()
                      .HasForeignKey<Медицинская_карта>(e => e.пациент_id)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.дата_создания).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.дата_обновления).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<Запись>(entity =>
            {
                entity.ToTable("Запись");
                entity.HasKey(e => e.id);
                entity.HasOne(e => e.Пациент)
                      .WithMany()
                      .HasForeignKey(e => e.пациент_id)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Врач)
                      .WithMany()
                      .HasForeignKey(e => e.врач_id)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.Property(e => e.статус).HasConversion<string>().HasDefaultValue("запланирована");
                entity.Property(e => e.дата_создания).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.симптомы).HasDefaultValue("").IsRequired();
                entity.Property(e => e.диагноз).HasDefaultValue("");
                entity.Property(e => e.рекомендации).HasDefaultValue("");
                entity.Property(e => e.дата_записи).IsRequired();
                entity.Property(e => e.время_записи).IsRequired();
            });

            modelBuilder.Entity<Расписание>(entity =>
            {
                entity.ToTable("Расписание");
                entity.HasKey(e => e.id);
                entity.HasOne(e => e.Врач)
                      .WithMany()
                      .HasForeignKey(e => e.врач_id)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.день_недели).HasConversion<string>();
                entity.Property(e => e.активен).HasDefaultValue(true);
            });

            modelBuilder.Entity<Отзыв>(entity =>
            {
                entity.ToTable("Отзыв");
                entity.HasKey(e => e.id);
                entity.HasOne(e => e.Запись)
                      .WithOne()
                      .HasForeignKey<Отзыв>(e => e.запись_id)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.дата_создания).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.одобрен).HasDefaultValue(false);
            });
        }

        private void SeedTestData()
        {
            try
            {
                if (!Пользователь.Any())
                {
                    // 1. Специализации
                    var specializations = new[]
                    {
                        new Специализация { название = "Терапевт", описание = "Врач общей практики", категория = "Общая медицина" },
                        new Специализация { название = "Хирург", описание = "Операции и хирургические вмешательства", категория = "Хирургия" },
                        new Специализация { название = "Кардиолог", описание = "Лечение заболеваний сердца", категория = "Кардиология" },
                        new Специализация { название = "Невролог", описание = "Лечение нервной системы", категория = "Неврология" }
                    };

                    Специализация.AddRange(specializations);
                    SaveChanges();

                    // 2. Пользователи
                    var users = new[]
                    {
                        new Пользователь {
                            email = "admin@clinic.ru",
                            пароль = "admin123",
                            роль = "администратор",
                            имя = "Админ",
                            фамилия = "Админов",
                            телефон = "+7 (000) 000-00-00",
                            активен = true
                        },
                        new Пользователь {
                            email = "doctor@clinic.ru",
                            пароль = "doctor1",
                            роль = "врач",
                            имя = "Доктор",
                            фамилия = "Врачев",
                            телефон = "+7 (111) 111-11-11",
                            активен = true
                        },
                        new Пользователь {
                            email = "patient@clinic.ru",
                            пароль = "patient1",
                            роль = "пациент",
                            имя = "Пациент",
                            фамилия = "Пациентов",
                            телефон = "+7 (222) 222-22-22",
                            активен = true
                        }
                    };

                    Пользователь.AddRange(users);
                    SaveChanges();

                    // 3. Врач
                    var doctor = new Врач
                    {
                        пользователь_id = users.First(u => u.email == "doctor@clinic.ru").id,
                        специализация_id = specializations.First().id,
                        лицензия = "Л-12345",
                        страхование = "ДМС",
                        программа = "Стандарт",
                        рейтинг = 4.8m,
                        статус = "активен"
                    };

                    Врач.Add(doctor);
                    SaveChanges();

                    // 4. Медицинская карта пациента
                    Медицинская_карта.Add(new Медицинская_карта
                    {
                        пациент_id = users.First(u => u.email == "patient@clinic.ru").id,
                        группа_крови = "I (0) Rh+",
                        аллергии = "Нет",
                        хронические_заболевания = "Нет"
                    });
                    SaveChanges();

                    // 5. Расписание врача
                    Расписание.Add(new Расписание
                    {
                        врач_id = doctor.id,
                        день_недели = "Понедельник",
                        время_начала = new TimeSpan(9, 0, 0),
                        время_окончания = new TimeSpan(18, 0, 0),
                        перерыв_начала = new TimeSpan(13, 0, 0),
                        перерыв_окончания = new TimeSpan(14, 0, 0),
                        активен = true
                    });
                    SaveChanges();

                    // 6. Запись на прием
                    Запись.Add(new Запись
                    {
                        пациент_id = users.First(u => u.email == "patient@clinic.ru").id,
                        врач_id = doctor.id,
                        дата_записи = DateTime.Now.AddDays(1).Date,
                        время_записи = new TimeSpan(10, 0, 0),
                        статус = "запланирована",
                        симптомы = "Температура, кашель",
                        дата_создания = DateTime.Now
                    });
                    SaveChanges();
                }
            }
            catch
            {
                // Игнорируем ошибки при добавлении тестовых данных
            }
        }
    }
}