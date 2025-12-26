using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp4.Domain;
using WpfApp4.Interfaces;
using WpfApp4.Models.Entities;
using WpfApp4.Utils;

namespace WpfApp4.Models.Entities
{
    public class ПользовательRepository : IRepository<Запись>
    {
        private readonly string _connectionString;

        public ПользовательRepository()
        {
            _connectionString = ConnectionManager.GetConnectionString();
        }

        public IEnumerable<Запись> GetAll()
        {
            using var context = new MyDatabaseContext(_connectionString);
            var appointments = context.Запись
                .Include(z => z.Пациент)
                .Include(z => z.Врач)
                .ThenInclude(d => d.Пользователь)
                .Include(z => z.Врач.Специализация)
                .ToList(); // Сначала загружаем в память

            // Сортируем в памяти
            return appointments
                .OrderByDescending(z => z.дата_записи)
                .ThenBy(z => z.время_записи)
                .ToList();
        }

        public Запись? Get(int id)
        {
            using var context = new MyDatabaseContext(_connectionString);
            return context.Запись
                .Include(z => z.Пациент)
                .Include(z => z.Врач)
                .FirstOrDefault(z => z.id == id);
        }

        public bool Add(Запись entity)
        {
            using var context = new MyDatabaseContext(_connectionString);
            context.Запись.Add(entity);
            return context.SaveChanges() > 0;
        }

        public bool Remove(int id)
        {
            using var context = new MyDatabaseContext(_connectionString);
            var appointment = context.Запись.Find(id);
            if (appointment != null)
            {
                context.Запись.Remove(appointment);
                return context.SaveChanges() > 0;
            }
            return false;
        }

        public bool Update(int id, Запись entity)
        {
            using var context = new MyDatabaseContext(_connectionString);
            var appointment = context.Запись.Find(id);
            if (appointment != null)
            {
                appointment.пациент_id = entity.пациент_id;
                appointment.врач_id = entity.врач_id;
                appointment.дата_записи = entity.дата_записи;
                appointment.время_записи = entity.время_записи;
                appointment.статус = entity.статус ?? "запланирована";
                appointment.симптомы = entity.симптомы ?? string.Empty;
                appointment.диагноз = entity.диагноз ?? string.Empty;
                appointment.рекомендации = entity.рекомендации ?? string.Empty;
                return context.SaveChanges() > 0;
            }
            return false;
        }
    }
}