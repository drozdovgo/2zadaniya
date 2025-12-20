using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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

            System.Diagnostics.Debug.WriteLine($"=== MyDatabaseContext создан ===");
            System.Diagnostics.Debug.WriteLine($"Строка подключения: {connectionString}");

            try
            {
                // Создаем базу и таблицы, если их нет
                var created = Database.EnsureCreated();
                System.Diagnostics.Debug.WriteLine($"База данных создана/проверена: {created}");

                // Проверяем и добавляем тестовые данные
                CheckAndSeedData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка при создании/подключении к базе данных: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(_connectionString);
                System.Diagnostics.Debug.WriteLine($"✅ SQLite сконфигурирован для: {_connectionString}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация таблиц (оставить без изменений)
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

        private void CheckAndSeedData()
        {
            try
            {
                // Проверяем, есть ли хоть один пользователь
                if (!Пользователь.Any())
                {
                    System.Diagnostics.Debug.WriteLine("В базе нет пользователей, добавляем тестовые данные...");
                    SeedTestData();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"В базе уже есть {Пользователь.Count()} пользователей");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка проверки данных: {ex.Message}");
            }
        }

        private void SeedTestData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📝 Начинаем добавление тестовых данных...");

                // 1. Специализации
                var specializations = new List<Специализация>
                {
                    new Специализация { название = "Терапевт", описание = "Врач общей практики", категория = "Общая медицина" },
                    new Специализация { название = "Хирург", описание = "Операции и хирургические вмешательства", категория = "Хирургия" },
                    new Специализация { название = "Кардиолог", описание = "Лечение заболеваний сердца", категория = "Кардиология" },
                    new Специализация { название = "Невролог", описание = "Лечение нервной системы", категория = "Неврология" }
                };

                Специализация.AddRange(specializations);
                SaveChanges();
                System.Diagnostics.Debug.WriteLine($"✅ Добавлено {specializations.Count} специализаций");

                // 2. Простые тестовые пользователи
                var users = new List<Пользователь>
                {
                    // Администратор
                    new Пользователь {
                        email = "admin@clinic.ru",
                        пароль = "admin123",
                        роль = "администратор",
                        имя = "Админ",
                        фамилия = "Админов",
                        телефон = "+7 (000) 000-00-00",
                        активен = true
                    },
                    
                    // Врач
                    new Пользователь {
                        email = "doctor@clinic.ru",
                        пароль = "doctor1",
                        роль = "врач",
                        имя = "Доктор",
                        фамилия = "Врачев",
                        телефон = "+7 (111) 111-11-11",
                        активен = true
                    },

                    // Пациент
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
                System.Diagnostics.Debug.WriteLine($"✅ Добавлено {users.Count} пользователей");

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
                System.Diagnostics.Debug.WriteLine($"✅ Добавлен врач");

                // 4. Медицинская карта пациента
                var medicalCard = new Медицинская_карта
                {
                    пациент_id = users.First(u => u.email == "patient@clinic.ru").id,
                    группа_крови = "I (0) Rh+",
                    аллергии = "Нет",
                    хронические_заболевания = "Нет"
                };

                Медицинская_карта.Add(medicalCard);
                SaveChanges();
                System.Diagnostics.Debug.WriteLine($"✅ Добавлена медицинская карта");

                // 5. Расписание врача
                var schedule = new Расписание
                {
                    врач_id = doctor.id,
                    день_недели = "Понедельник",
                    время_начала = new TimeSpan(9, 0, 0),
                    время_окончания = new TimeSpan(18, 0, 0),
                    перерыв_начала = new TimeSpan(13, 0, 0),
                    перерыв_окончания = new TimeSpan(14, 0, 0),
                    активен = true
                };

                Расписание.Add(schedule);
                SaveChanges();
                System.Diagnostics.Debug.WriteLine($"✅ Добавлено расписание");

                // 6. Запись на прием
                var appointment = new Запись
                {
                    пациент_id = users.First(u => u.email == "patient@clinic.ru").id,
                    врач_id = doctor.id,
                    дата_записи = DateTime.Now.AddDays(1).Date,
                    время_записи = new TimeSpan(10, 0, 0),
                    статус = "запланирована",
                    симптомы = "Температура, кашель",
                    дата_создания = DateTime.Now
                };

                Запись.Add(appointment);
                SaveChanges();
                System.Diagnostics.Debug.WriteLine($"✅ Добавлена запись");

                System.Diagnostics.Debug.WriteLine("🎉 Тестовые данные успешно добавлены!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка при добавлении тестовых данных: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }
    }
}