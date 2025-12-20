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
        private static readonly object _dbLock = new object();
        private readonly string _connectionString = DatabaseHelper.GetConnectionString();

        public IEnumerable<Расписание> GetAll()
        {
            lock (_dbLock)
            {
                try
                {
                    using var context = new MyDatabaseContext(_connectionString);
                    return context.Расписание
                        .Include(r => r.Врач)
                        .ThenInclude(d => d.Пользователь)
                        .Include(r => r.Врач.Специализация)  // Добавьте эту строку
                        .AsNoTracking()
                        .ToList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения расписания: {ex.Message}");
                    return new List<Расписание>();
                }
            }
        }

        public Расписание? Get(int id)
        {
            lock (_dbLock)
            {
                try
                {
                    using var context = new MyDatabaseContext(_connectionString);
                    return context.Расписание
                        .Include(r => r.Врач)
                        .FirstOrDefault(r => r.id == id);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения расписания по ID {id}: {ex.Message}");
                    return null;
                }
            }
        }

        public bool Add(Расписание entity)
        {
            lock (_dbLock)
            {
                try
                {
                    using var context = new MyDatabaseContext(_connectionString);
                    context.Расписание.Add(entity);
                    var result = context.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"✅ Расписание добавлено, изменений: {result}");
                    return result > 0;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка добавления расписания: {ex.Message}");
                    return false;
                }
            }
        }

        public bool Remove(int id)
        {
            lock (_dbLock)
            {
                try
                {
                    using var context = new MyDatabaseContext(_connectionString);
                    var schedule = context.Расписание.Find(id);
                    if (schedule != null)
                    {
                        context.Расписание.Remove(schedule);
                        var result = context.SaveChanges();
                        System.Diagnostics.Debug.WriteLine($"✅ Расписание удалено, изменений: {result}");
                        return result > 0;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка удаления расписания: {ex.Message}");
                    return false;
                }
            }
        }

        public bool Update(int id, Расписание entity)
        {
            lock (_dbLock)
            {
                try
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
                        var result = context.SaveChanges();
                        System.Diagnostics.Debug.WriteLine($"✅ Расписание обновлено, изменений: {result}");
                        return result > 0;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка обновления расписания: {ex.Message}");
                    return false;
                }
            }
        }

        public List<Расписание> GetDoctorSchedule(int doctorId)
        {
            lock (_dbLock)
            {
                try
                {
                    using var context = new MyDatabaseContext(_connectionString);
                    return context.Расписание
                        .Include(r => r.Врач)
                        .ThenInclude(d => d.Пользователь)
                        .Where(r => r.врач_id == doctorId && r.активен)
                        .OrderBy(r => r.день_недели)
                        .ThenBy(r => r.время_начала)
                        .AsNoTracking()
                        .ToList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения расписания врача {doctorId}: {ex.Message}");
                    return new List<Расписание>();
                }
            }
        }
    }
}