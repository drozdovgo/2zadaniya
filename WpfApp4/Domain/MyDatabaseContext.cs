using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WpfApp4.Models.Entities;
using System.ComponentModel;

namespace WpfApp4.Domain
{
    public class MyDatabaseContext : DbContext
    {
        private readonly string connectionString = null!;


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
            Database.EnsureCreated();
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(connectionString);
        //}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=medicalclinic.db");
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
                entity.Property(e => e.дата_регистрации).HasDefaultValueSql("GETDATE()");
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
                entity.Property(e => e.дата_создания).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.дата_обновления).HasDefaultValueSql("GETDATE()");
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
                entity.Property(e => e.дата_создания).HasDefaultValueSql("GETDATE()");
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
                entity.Property(e => e.дата_создания).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.одобрен).HasDefaultValue(false);
            });


            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Специализация>().HasData(
                new Специализация { id = 1, название = "Кардиолог", описание = "Специалист по заболеваниям сердечно-сосудистой системы", категория = "Терапия" },
                new Специализация { id = 2, название = "Невролог", описание = "Специалист по заболеваниям нервной системы", категория = "Неврология" },
                new Специализация { id = 3, название = "Терапевт", описание = "Врач общей практики", категория = "Терапия" },
                new Специализация { id = 4, название = "Хирург", описание = "Специалист по оперативным вмешательствам", категория = "Хирургия" },
                new Специализация { id = 5, название = "Офтальмолог", описание = "Специалист по заболеваниям глаз", категория = "Офтальмология" },
                new Специализация { id = 6, название = "Отоларинголог", описание = "Специалист по заболеваниям уха, горла и носа", категория = "ЛОР" },
                new Специализация { id = 7, название = "Дерматолог", описание = "Специалист по заболеваниям кожи", категория = "Дерматология" },
                new Специализация { id = 8, название = "Гастроэнтеролог", описание = "Специалист по заболеваниям ЖКТ", категория = "Терапия" },
                new Специализация { id = 9, название = "Эндокринолог", описание = "Специалист по заболеваниям эндокринной системы", категория = "Терапия" },
                new Специализация { id = 10, название = "Педиатр", описание = "Специалист по детским заболеваниям", категория = "Педиатрия" }
            );


            modelBuilder.Entity<Пользователь>().HasData(

                new Пользователь { id = 1, email = "ivan.petrov@mail.ru", пароль = "123456", роль = "пациент", имя = "Иван", фамилия = "Петров", телефон = "+79161234567", дата_рождения = new DateTime(1985, 5, 15), дата_регистрации = DateTime.Now.AddDays(-30), активен = true },
                new Пользователь { id = 2, email = "olga.ivanova@gmail.com", пароль = "123456", роль = "пациент", имя = "Ольга", фамилия = "Иванова", телефон = "+79219876543", дата_рождения = new DateTime(1992, 3, 10), дата_регистрации = DateTime.Now.AddDays(-10), активен = true },
                new Пользователь { id = 3, email = "sergey.sidorov@yandex.ru", пароль = "123456", роль = "пациент", имя = "Сергей", фамилия = "Сидоров", телефон = "+79031234567", дата_рождения = new DateTime(1978, 11, 25), дата_регистрации = DateTime.Now.AddDays(-45), активен = true },
                new Пользователь { id = 4, email = "ekaterina.smirnova@mail.ru", пароль = "123456", роль = "пациент", имя = "Екатерина", фамилия = "Смирнова", телефон = "+79167654321", дата_рождения = new DateTime(1989, 7, 18), дата_регистрации = DateTime.Now.AddDays(-20), активен = true },
                new Пользователь { id = 5, email = "dmitry.kuznetsov@gmail.com", пароль = "123456", роль = "пациент", имя = "Дмитрий", фамилия = "Кузнецов", телефон = "+79098765432", дата_рождения = new DateTime(1995, 12, 5), дата_регистрации = DateTime.Now.AddDays(-5), активен = true },
                new Пользователь { id = 6, email = "marina.volkova@yandex.ru", пароль = "123456", роль = "пациент", имя = "Марина", фамилия = "Волкова", телефон = "+79151112233", дата_рождения = new DateTime(1982, 9, 30), дата_регистрации = DateTime.Now.AddDays(-60), активен = true },


                new Пользователь { id = 7, email = "anna.medvedeva@clinic.ru", пароль = "123456", роль = "врач", имя = "Анна", фамилия = "Медведева", телефон = "+79035554466", дата_рождения = new DateTime(1975, 4, 12), дата_регистрации = DateTime.Now.AddDays(-365), активен = true },
                new Пользователь { id = 8, email = "alexander.popov@clinic.ru", пароль = "123456", роль = "врач", имя = "Александр", фамилия = "Попов", телефон = "+79163332211", дата_рождения = new DateTime(1980, 8, 22), дата_регистрации = DateTime.Now.AddDays(-300), активен = true },
                new Пользователь { id = 9, email = "natalia.fedorova@clinic.ru", пароль = "123456", роль = "врач", имя = "Наталья", фамилия = "Федорова", телефон = "+79024445566", дата_рождения = new DateTime(1972, 11, 8), дата_регистрации = DateTime.Now.AddDays(-400), активен = true },
                new Пользователь { id = 10, email = "mikhail.novikov@clinic.ru", пароль = "123456", роль = "врач", имя = "Михаил", фамилия = "Новиков", телефон = "+79167778899", дата_рождения = new DateTime(1983, 6, 14), дата_регистрации = DateTime.Now.AddDays(-200), активен = true },


                new Пользователь { id = 11, email = "admin@clinic.ru", пароль = "admin123", роль = "администратор", имя = "Алексей", фамилия = "Козлов", телефон = "+79998887766", дата_рождения = new DateTime(1990, 8, 22), дата_регистрации = DateTime.Now.AddDays(-500), активен = true },
                new Пользователь { id = 12, email = "manager@clinic.ru", пароль = "manager123", роль = "администратор", имя = "Светлана", фамилия = "Морозова", телефон = "+79997776655", дата_рождения = new DateTime(1988, 2, 28), дата_регистрации = DateTime.Now.AddDays(-350), активен = true }
            );


            modelBuilder.Entity<Врач>().HasData(
                new Врач { id = 1, пользователь_id = 7, специализация_id = 1, лицензия = "LIC-CARD-001", страхование = "Полис ДМС №12345", программа = "Кардиологическая программа", рейтинг = 4.8m, статус = "активен" },
                new Врач { id = 2, пользователь_id = 8, специализация_id = 3, лицензия = "LIC-THER-001", страхование = "Полис ДМС №12346", программа = "Терапевтическая программа", рейтинг = 4.6m, статус = "активен" },
                new Врач { id = 3, пользователь_id = 9, специализация_id = 2, лицензия = "LIC-NEUR-001", страхование = "Полис ДМС №12347", программа = "Неврологическая программа", рейтинг = 4.9m, статус = "активен" },
                new Врач { id = 4, пользователь_id = 10, специализация_id = 4, лицензия = "LIC-SURG-001", страхование = "Полис ДМС №12348", программа = "Хирургическая программа", рейтинг = 4.7m, статус = "активен" }
            );


            modelBuilder.Entity<Медицинская_карта>().HasData(
                new Медицинская_карта { id = 1, пациент_id = 1, группа_крови = "A+", аллергии = "Пенициллин, аспирин", хронические_заболевания = "Гипертония 1 степени", дата_создания = DateTime.Now.AddDays(-30), дата_обновления = DateTime.Now.AddDays(-5) },
                new Медицинская_карта { id = 2, пациент_id = 2, группа_крови = "B+", аллергии = "Нет", хронические_заболевания = "Нет", дата_создания = DateTime.Now.AddDays(-10), дата_обновления = DateTime.Now.AddDays(-10) },
                new Медицинская_карта { id = 3, пациент_id = 3, группа_крови = "O+", аллергии = "Пыльца, шерсть животных", хронические_заболевания = "Бронхиальная астма", дата_создания = DateTime.Now.AddDays(-45), дата_обновления = DateTime.Now.AddDays(-15) },
                new Медицинская_карта { id = 4, пациент_id = 4, группа_крови = "AB+", аллергии = "Морепродукты", хронические_заболевания = "Гастрит", дата_создания = DateTime.Now.AddDays(-20), дата_обновления = DateTime.Now.AddDays(-20) },
                new Медицинская_карта { id = 5, пациент_id = 5, группа_крови = "A-", аллергии = "Нет", хронические_заболевания = "Нет", дата_создания = DateTime.Now.AddDays(-5), дата_обновления = DateTime.Now.AddDays(-5) },
                new Медицинская_карта { id = 6, пациент_id = 6, группа_крови = "B-", аллергии = "Антибиотики", хронические_заболевания = "Сахарный диабет 2 типа", дата_создания = DateTime.Now.AddDays(-60), дата_обновления = DateTime.Now.AddDays(-10) }
            );
        }
    }
}
