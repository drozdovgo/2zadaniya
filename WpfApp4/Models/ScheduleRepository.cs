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
    public class ScheduleRepository : IRepository<Расписание>
    {
        private readonly string _connectionString;

        public ScheduleRepository()
        {
            _connectionString = ConnectionManager.GetConnectionString();
        }

        public IEnumerable<Расписание> GetAll()
        {
            using var context = new MyDatabaseContext(_connectionString);
            var schedules = context.Расписание
                .Include(r => r.Врач)
                .ThenInclude(d => d.Пользователь)
                .Include(r => r.Врач.Специализация)
                .ToList(); // Сначала загружаем в память

            return schedules;
        }

        public Расписание? Get(int id)
        {
            using var context = new MyDatabaseContext(_connectionString);
            return context.Расписание
                .Include(r => r.Врач)
                .FirstOrDefault(r => r.id == id);
        }

        public bool Add(Расписание entity)
        {
            using var context = new MyDatabaseContext(_connectionString);
            context.Расписание.Add(entity);
            return context.SaveChanges() > 0;
        }

        public bool Remove(int id)
        {
            using var context = new MyDatabaseContext(_connectionString);
            var schedule = context.Расписание.Find(id);
            if (schedule != null)
            {
                context.Расписание.Remove(schedule);
                return context.SaveChanges() > 0;
            }
            return false;
        }

        public bool Update(int id, Расписание entity)
        {
            using var context = new MyDatabaseContext(_connectionString);
            var schedule = context.Расписание.Find(id);
            if (schedule != null)
            {
                schedule.врач_id = entity.врач_id;
                schedule.день_недели = entity.день_недели;
                schedule.время_начала = entity.время_начала;
                schedule.время_окончания = entity.время_окончания;
                schedule.перерыв_начала = entity.перерыв_начала;
                schedule.перерыв_окончания = entity.перерыв_окончания;
                schedule.активен = entity.активен;
                return context.SaveChanges() > 0;
            }
            return false;
        }

        public List<Расписание> GetDoctorSchedule(int doctorId)
        {
            using var context = new MyDatabaseContext(_connectionString);
            var schedules = context.Расписание
                .Include(r => r.Врач)
                .ThenInclude(d => d.Пользователь)
                .Where(r => r.врач_id == doctorId && r.активен)
                .ToList(); // Сначала загружаем в память

            // Сортируем в памяти
            return schedules
                .OrderBy(r => r.день_недели)
                .ThenBy(r => r.время_начала)
                .ToList();
        }
    }
}