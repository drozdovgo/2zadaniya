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
                    System.Diagnostics.Debug.WriteLine("=== Начало заполнения тестовыми данными ===");

                    // 1. Специализации
                    var specializations = new[]
                    {
                new Специализация {
                    название = "Терапевт",
                    описание = "Врач общей практики",
                    категория = "Общая медицина"
                },
                new Специализация {
                    название = "Хирург",
                    описание = "Операции и хирургические вмешательства",
                    категория = "Хирургия"
                },
                new Специализация {
                    название = "Кардиолог",
                    описание = "Лечение заболеваний сердца",
                    категория = "Кардиология"
                },
                new Специализация {
                    название = "Невролог",
                    описание = "Лечение нервной системы",
                    категория = "Неврология"
                },
                new Специализация {
                    название = "Офтальмолог",
                    описание = "Лечение заболеваний глаз",
                    категория = "Офтальмология"
                },
                new Специализация {
                    название = "Отоларинголог",
                    описание = "Лечение уха, горла, носа",
                    категория = "ЛОР"
                },
                new Специализация {
                    название = "Гастроэнтеролог",
                    описание = "Лечение заболеваний ЖКТ",
                    категория = "Гастроэнтерология"
                },
                new Специализация {
                    название = "Дерматолог",
                    описание = "Лечение заболеваний кожи",
                    категория = "Дерматология"
                },
                new Специализация {
                    название = "Ортопед",
                    описание = "Лечение заболеваний костей и суставов",
                    категория = "Травматология"
                },
                new Специализация {
                    название = "Эндокринолог",
                    описание = "Лечение заболеваний эндокринной системы",
                    категория = "Эндокринология"
                }
            };

                    Специализация.AddRange(specializations);
                    SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"✅ Добавлено {specializations.Length} специализаций");

                    // 2. Пользователи
                    var users = new List<Пользователь>();

                    // Администраторы
                    users.Add(new Пользователь
                    {
                        email = "admin@clinic.ru",
                        пароль = "admin123",
                        роль = "администратор",
                        имя = "Алексей",
                        фамилия = "Иванов",
                        телефон = "+7 (495) 111-11-11",
                        дата_рождения = new DateTime(1980, 5, 15),
                        активен = true
                    });

                    users.Add(new Пользователь
                    {
                        email = "admin2@clinic.ru",
                        пароль = "admin456",
                        роль = "администратор",
                        имя = "Ольга",
                        фамилия = "Петрова",
                        телефон = "+7 (495) 222-22-22",
                        дата_рождения = new DateTime(1985, 8, 20),
                        активен = true
                    });

                    // Врачи - теперь с почтами по имени и фамилии
                    // Терапевты
                    users.Add(new Пользователь
                    {
                        email = "dmitry.vrachev@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Дмитрий",
                        фамилия = "Врачев",
                        телефон = "+7 (495) 333-33-33",
                        дата_рождения = new DateTime(1975, 3, 10),
                        активен = true
                    });

                    users.Add(new Пользователь
                    {
                        email = "ekaterina.smirnova@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Екатерина",
                        фамилия = "Смирнова",
                        телефон = "+7 (495) 444-44-44",
                        дата_рождения = new DateTime(1982, 7, 25),
                        активен = true
                    });

                    // Хирурги
                    users.Add(new Пользователь
                    {
                        email = "andrey.karpov@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Андрей",
                        фамилия = "Карпов",
                        телефон = "+7 (495) 555-55-55",
                        дата_рождения = new DateTime(1978, 11, 5),
                        активен = true
                    });

                    users.Add(new Пользователь
                    {
                        email = "sergey.lebedeff@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Сергей",
                        фамилия = "Лебедев",
                        телефон = "+7 (495) 666-66-66",
                        дата_рождения = new DateTime(1970, 9, 30),
                        активен = true
                    });

                    // Кардиологи
                    users.Add(new Пользователь
                    {
                        email = "maria.novikova@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Мария",
                        фамилия = "Новикова",
                        телефон = "+7 (495) 777-77-77",
                        дата_рождения = new DateTime(1988, 2, 14),
                        активен = true
                    });

                    users.Add(new Пользователь
                    {
                        email = "alexey.kuznetsov@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Алексей",
                        фамилия = "Кузнецов",
                        телефон = "+7 (495) 888-88-88",
                        дата_рождения = new DateTime(1973, 6, 18),
                        активен = true
                    });

                    // Неврологи
                    users.Add(new Пользователь
                    {
                        email = "anna.ivanova@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Анна",
                        фамилия = "Иванова",
                        телефон = "+7 (495) 999-99-99",
                        дата_рождения = new DateTime(1985, 4, 12),
                        активен = true
                    });

                    users.Add(new Пользователь
                    {
                        email = "vladimir.petrov@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Владимир",
                        фамилия = "Петров",
                        телефон = "+7 (495) 101-01-01",
                        дата_рождения = new DateTime(1968, 11, 30),
                        активен = true
                    });

                    // Офтальмологи
                    users.Add(new Пользователь
                    {
                        email = "elena.sokolova@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Елена",
                        фамилия = "Соколова",
                        телефон = "+7 (495) 202-02-02",
                        дата_рождения = new DateTime(1977, 8, 22),
                        активен = true
                    });

                    users.Add(new Пользователь
                    {
                        email = "mikhail.popov@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Михаил",
                        фамилия = "Попов",
                        телефон = "+7 (495) 303-03-03",
                        дата_рождения = new DateTime(1980, 1, 14),
                        активен = true
                    });

                    // ЛОР-врачи
                    users.Add(new Пользователь
                    {
                        email = "natalya.morozova@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Наталья",
                        фамилия = "Морозова",
                        телефон = "+7 (495) 404-04-04",
                        дата_рождения = new DateTime(1983, 12, 5),
                        активен = true
                    });

                    users.Add(new Пользователь
                    {
                        email = "dmitry.volkov@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Дмитрий",
                        фамилия = "Волков",
                        телефон = "+7 (495) 505-05-05",
                        дата_рождения = new DateTime(1976, 7, 19),
                        активен = true
                    });

                    // Гастроэнтерологи
                    users.Add(new Пользователь
                    {
                        email = "irina.pavlova@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Ирина",
                        фамилия = "Павлова",
                        телефон = "+7 (495) 606-06-06",
                        дата_рождения = new DateTime(1981, 3, 28),
                        активен = true
                    });

                    users.Add(new Пользователь
                    {
                        email = "alexander.romanov@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Александр",
                        фамилия = "Романов",
                        телефон = "+7 (495) 707-07-07",
                        дата_рождения = new DateTime(1972, 10, 8),
                        активен = true
                    });

                    // Дерматологи
                    users.Add(new Пользователь
                    {
                        email = "olga.vasilyeva@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Ольга",
                        фамилия = "Васильева",
                        телефон = "+7 (495) 808-08-08",
                        дата_рождения = new DateTime(1979, 5, 17),
                        активен = true
                    });

                    users.Add(new Пользователь
                    {
                        email = "viktoria.fedorova@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Виктория",
                        фамилия = "Федорова",
                        телефон = "+7 (495) 909-09-09",
                        дата_рождения = new DateTime(1986, 9, 3),
                        активен = true
                    });

                    // Ортопеды
                    users.Add(new Пользователь
                    {
                        email = "stanislav.egorov@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Станислав",
                        фамилия = "Егоров",
                        телефон = "+7 (495) 121-21-21",
                        дата_рождения = new DateTime(1965, 2, 25),
                        активен = true
                    });

                    // Эндокринологи
                    users.Add(new Пользователь
                    {
                        email = "svetlana.kirillova@clinic.ru",
                        пароль = "doctor123",
                        роль = "врач",
                        имя = "Светлана",
                        фамилия = "Кириллова",
                        телефон = "+7 (495) 131-31-31",
                        дата_рождения = new DateTime(1974, 6, 11),
                        активен = true
                    });

                    // Пациенты
                    users.Add(new Пользователь
                    {
                        email = "ivan.patientov@clinic.ru",
                        пароль = "patient123",
                        роль = "пациент",
                        имя = "Иван",
                        фамилия = "Пациентов",
                        телефон = "+7 (495) 141-41-41",
                        дата_рождения = new DateTime(1990, 1, 12),
                        активен = true
                    });

                    users.Add(new Пользователь
                    {
                        email = "anna.sidorova@clinic.ru",
                        пароль = "patient123",
                        роль = "пациент",
                        имя = "Анна",
                        фамилия = "Сидорова",
                        телефон = "+7 (495) 151-51-51",
                        дата_рождения = new DateTime(1985, 6, 18),
                        активен = true
                    });

                    users.Add(new Пользователь
                    {
                        email = "petr.kozlov@clinic.ru",
                        пароль = "patient123",
                        роль = "пациент",
                        имя = "Петр",
                        фамилия = "Козлов",
                        телефон = "+7 (495) 161-61-61",
                        дата_рождения = new DateTime(1972, 12, 3),
                        активен = true
                    });

                    users.Add(new Пользователь
                    {
                        email = "elena.morozova@clinic.ru",
                        пароль = "patient123",
                        роль = "пациент",
                        имя = "Елена",
                        фамилия = "Морозова",
                        телефон = "+7 (495) 171-71-71",
                        дата_рождения = new DateTime(1995, 4, 22),
                        активен = true
                    });

                    users.Add(new Пользователь
                    {
                        email = "alexey.semenov@clinic.ru",
                        пароль = "patient123",
                        роль = "пациент",
                        имя = "Алексей",
                        фамилия = "Семенов",
                        телефон = "+7 (495) 181-81-81",
                        дата_рождения = new DateTime(1988, 8, 7),
                        активен = true
                    });

                    users.Add(new Пользователь
                    {
                        email = "marina.orlova@clinic.ru",
                        пароль = "patient123",
                        роль = "пациент",
                        имя = "Марина",
                        фамилия = "Орлова",
                        телефон = "+7 (495) 191-91-91",
                        дата_рождения = new DateTime(1992, 11, 15),
                        активен = true
                    });

                    Пользователь.AddRange(users);
                    SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"✅ Добавлено {users.Count} пользователей");

                    // 3. Врачи (связываем с пользователями и специализациями)
                    var doctors = new List<Врач>();

                    // Терапевты
                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "dmitry.vrachev@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Терапевт").id,
                        лицензия = "Л-ТЕР-001",
                        страхование = "ДМС Премиум",
                        программа = "Стандарт+",
                        рейтинг = 4.8m,
                        статус = "активен"
                    });

                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "ekaterina.smirnova@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Терапевт").id,
                        лицензия = "Л-ТЕР-002",
                        страхование = "ОМС + ДМС",
                        программа = "Расширенная",
                        рейтинг = 4.7m,
                        статус = "активен"
                    });

                    // Хирурги
                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "andrey.karpov@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Хирург").id,
                        лицензия = "Л-ХИР-001",
                        страхование = "ДМС Премиум",
                        программа = "Хирургическая",
                        рейтинг = 4.9m,
                        статус = "активен"
                    });

                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "sergey.lebedeff@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Хирург").id,
                        лицензия = "Л-ХИР-002",
                        страхование = "ОМС",
                        программа = "Общая хирургия",
                        рейтинг = 4.6m,
                        статус = "активен"
                    });

                    // Кардиологи
                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "maria.novikova@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Кардиолог").id,
                        лицензия = "Л-КАР-001",
                        страхование = "ДМС Премиум",
                        программа = "Кардиологическая",
                        рейтинг = 4.8m,
                        статус = "активен"
                    });

                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "alexey.kuznetsov@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Кардиолог").id,
                        лицензия = "Л-КАР-002",
                        страхование = "ДМС",
                        программа = "Кардиология+",
                        рейтинг = 4.7m,
                        статус = "активен"
                    });

                    // Неврологи
                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "anna.ivanova@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Невролог").id,
                        лицензия = "Л-НЕВ-001",
                        страхование = "ДМС",
                        программа = "Неврологическая",
                        рейтинг = 4.6m,
                        статус = "активен"
                    });

                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "vladimir.petrov@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Невролог").id,
                        лицензия = "Л-НЕВ-002",
                        страхование = "ОМС",
                        программа = "Общая неврология",
                        рейтинг = 4.5m,
                        статус = "активен"
                    });

                    // Офтальмологи
                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "elena.sokolova@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Офтальмолог").id,
                        лицензия = "Л-ОФТ-001",
                        страхование = "ДМС",
                        программа = "Офтальмологическая",
                        рейтинг = 4.7m,
                        статус = "активен"
                    });

                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "mikhail.popov@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Офтальмолог").id,
                        лицензия = "Л-ОФТ-002",
                        страхование = "ОМС",
                        программа = "Общая офтальмология",
                        рейтинг = 4.4m,
                        статус = "активен"
                    });

                    // ЛОР-врачи
                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "natalya.morozova@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Отоларинголог").id,
                        лицензия = "Л-ЛОР-001",
                        страхование = "ДМС",
                        программа = "ЛОР",
                        рейтинг = 4.6m,
                        статус = "активен"
                    });

                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "dmitry.volkov@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Отоларинголог").id,
                        лицензия = "Л-ЛОР-002",
                        страхование = "ОМС",
                        программа = "Общая ЛОР",
                        рейтинг = 4.5m,
                        статус = "активен"
                    });

                    // Гастроэнтерологи
                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "irina.pavlova@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Гастроэнтеролог").id,
                        лицензия = "Л-ГАС-001",
                        страхование = "ДМС Премиум",
                        программа = "Гастроэнтерологическая",
                        рейтинг = 4.8m,
                        статус = "активен"
                    });

                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "alexander.romanov@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Гастроэнтеролог").id,
                        лицензия = "Л-ГАС-002",
                        страхование = "ОМС",
                        программа = "Общая гастроэнтерология",
                        рейтинг = 4.6m,
                        статус = "активен"
                    });

                    // Дерматологи
                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "olga.vasilyeva@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Дерматолог").id,
                        лицензия = "Л-ДЕР-001",
                        страхование = "ДМС",
                        программа = "Дерматологическая",
                        рейтинг = 4.7m,
                        статус = "активен"
                    });

                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "viktoria.fedorova@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Дерматолог").id,
                        лицензия = "Л-ДЕР-002",
                        страхование = "ОМС",
                        программа = "Общая дерматология",
                        рейтинг = 4.5m,
                        статус = "активен"
                    });

                    // Ортопеды
                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "stanislav.egorov@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Ортопед").id,
                        лицензия = "Л-ОРТ-001",
                        страхование = "ДМС Премиум",
                        программа = "Ортопедическая",
                        рейтинг = 4.9m,
                        статус = "активен"
                    });

                    // Эндокринологи
                    doctors.Add(new Врач
                    {
                        пользователь_id = users.First(u => u.email == "svetlana.kirillova@clinic.ru").id,
                        специализация_id = specializations.First(s => s.название == "Эндокринолог").id,
                        лицензия = "Л-ЭНД-001",
                        страхование = "ДМС",
                        программа = "Эндокринологическая",
                        рейтинг = 4.8m,
                        статус = "активен"
                    });

                    Врач.AddRange(doctors);
                    SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"✅ Добавлено {doctors.Count} врачей");

                    // 4. Медицинские карты пациентов
                    var medicalRecords = new[]
                    {
                new Медицинская_карта
                {
                    пациент_id = users.First(u => u.email == "ivan.patientov@clinic.ru").id,
                    группа_крови = "I (0) Rh+",
                    аллергии = "Пенициллин",
                    хронические_заболевания = "Гипертония 1 степени"
                },
                new Медицинская_карта
                {
                    пациент_id = users.First(u => u.email == "anna.sidorova@clinic.ru").id,
                    группа_крови = "II (A) Rh+",
                    аллергии = "Нет",
                    хронические_заболевания = "Астма"
                },
                new Медицинская_карта
                {
                    пациент_id = users.First(u => u.email == "petr.kozlov@clinic.ru").id,
                    группа_крови = "III (B) Rh-",
                    аллергии = "Аспирин, цитрусовые",
                    хронические_заболевания = "Сахарный диабет 2 типа"
                },
                new Медицинская_карта
                {
                    пациент_id = users.First(u => u.email == "elena.morozova@clinic.ru").id,
                    группа_крови = "IV (AB) Rh+",
                    аллергии = "Антибиотики",
                    хронические_заболевания = "Нет"
                },
                new Медицинская_карта
                {
                    пациент_id = users.First(u => u.email == "alexey.semenov@clinic.ru").id,
                    группа_крови = "I (0) Rh-",
                    аллергии = "Пыльца",
                    хронические_заболевания = "Аллергический ринит"
                },
                new Медицинская_карта
                {
                    пациент_id = users.First(u => u.email == "marina.orlova@clinic.ru").id,
                    группа_крови = "II (A) Rh-",
                    аллергии = "Шерсть животных",
                    хронические_заболевания = "Мигрень"
                }
            };

                    Медицинская_карта.AddRange(medicalRecords);
                    SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"✅ Добавлено {medicalRecords.Length} медицинских карт");

                    // 5. Расписание врачей (упрощенное - все врачи работают пн-пт с 9 до 18)
                    var schedules = new List<Расписание>();

                    foreach (var doctor in doctors)
                    {
                        // Добавляем расписание на будние дни для каждого врача
                        var daysOfWeek = new[] { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница" };
                        foreach (var day in daysOfWeek)
                        {
                            schedules.Add(new Расписание
                            {
                                врач_id = doctor.id,
                                день_недели = day,
                                время_начала = new TimeSpan(9, 0, 0),
                                время_окончания = new TimeSpan(18, 0, 0),
                                перерыв_начала = new TimeSpan(13, 0, 0),
                                перерыв_окончания = new TimeSpan(14, 0, 0),
                                активен = true
                            });
                        }
                    }

                    Расписание.AddRange(schedules);
                    SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"✅ Добавлено {schedules.Count} расписаний");

                    // 6. Записи на прием (по 1 записи для каждого пациента к разным врачам)
                    var appointments = new List<Запись>();
                    var patientIds = users.Where(u => u.роль == "пациент").Select(u => u.id).ToList();

                    for (int i = 0; i < patientIds.Count; i++)
                    {
                        var patientId = patientIds[i];
                        // Каждому пациенту назначаем запись к врачу из его "группы" (по индексу)
                        var doctorIndex = i % doctors.Count;
                        var doctorId = doctors[doctorIndex].id;

                        appointments.Add(new Запись
                        {
                            пациент_id = patientId,
                            врач_id = doctorId,
                            дата_записи = DateTime.Now.AddDays(i + 1).Date, // На разные дни
                            время_записи = new TimeSpan(9 + i, 0, 0), // Разное время
                            статус = i % 3 == 0 ? "завершена" : "запланирована", // Чередуем статусы
                            симптомы = i % 2 == 0 ? "Головная боль, температура" : "Боль в горле, кашель",
                            диагноз = i % 3 == 0 ? "ОРВИ" : "",
                            рекомендации = i % 3 == 0 ? "Постельный режим, обильное питье" : "",
                            дата_создания = DateTime.Now.AddDays(-i - 1)
                        });
                    }

                    Запись.AddRange(appointments);
                    SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"✅ Добавлено {appointments.Count} записей на прием");

                    System.Diagnostics.Debug.WriteLine("=== Заполнение тестовыми данными завершено ===");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("База данных уже содержит данные, пропускаем заполнение");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка при заполнении тестовыми данными: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner: {ex.InnerException.Message}");
                }
                // Игнорируем ошибки при добавлении тестовых данных
            }
        }
    }
}