using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WpfApp4.Domain;
using WpfApp4.Interfaces;
using WpfApp4.Models.Entities;

namespace WpfApp4.Models.Entities
{
    public class ScheduleRepository : IRepository<Расписание>
    {
        public IEnumerable<Расписание> GetAll()
        {
            try
            {
                using var context = new MyDatabaseContext("Data Source=medicalclinic.db");
                lock (context)
                {
                    return context.Расписание
                        .Include(r => r.Врач)
                        .ThenInclude(d => d.Пользователь)
                        .AsNoTracking()
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения расписания: {ex.Message}");
                return new List<Расписание>();
            }
        }

        public Расписание? Get(int id)
        {
            try
            {
                using var context = new MyDatabaseContext("Data Source=medicalclinic.db");
                lock (context)
                {
                    return context.Расписание
                        .Include(r => r.Врач)
                        .FirstOrDefault(r => r.id == id);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения расписания: {ex.Message}");
                return null;
            }
        }

        public bool Add(Расписание entity)
        {
            try
            {
                using var context = new MyDatabaseContext("Data Source=medicalclinic.db");
                lock (context)
                {
                    context.Расписание.Add(entity);
                    var result = context.SaveChanges();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка добавления расписания: {ex.Message}");
                return false;
            }
        }

        public bool Remove(int id)
        {
            try
            {
                using var context = new MyDatabaseContext("Data Source=medicalclinic.db");
                lock (context)
                {
                    var schedule = context.Расписание.Find(id);
                    if (schedule != null)
                    {
                        context.Расписание.Remove(schedule);
                        var result = context.SaveChanges();
                        return result > 0;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления расписания: {ex.Message}");
                return false;
            }
        }

        public bool Update(int id, Расписание entity)
        {
            try
            {
                using var context = new MyDatabaseContext("Data Source=medicalclinic.db");
                lock (context)
                {
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
                        return result > 0;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка обновления расписания: {ex.Message}");
                return false;
            }
        }

        public List<Расписание> GetDoctorSchedule(int doctorId)
        {
            try
            {
                using var context = new MyDatabaseContext("Data Source=medicalclinic.db");
                lock (context)
                {
                    return context.Расписание
                        .Where(r => r.врач_id == doctorId && r.активен)
                        .AsNoTracking()
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения расписания врача: {ex.Message}");
                return new List<Расписание>();
            }
        }
    }
}