using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WpfApp4.Domain;
using WpfApp4.Interfaces;
using WpfApp4.Models.Entities;

namespace WpfApp4.Models.Entities
{
    public class AppointmentRepository : IRepository<Запись>
    {
        private static readonly object _dbLock = new object();
        private readonly string _connectionString = "Data Source=medicalclinic.db";

        public IEnumerable<Запись> GetAll()
        {
            lock (_dbLock)
            {
                try
                {
                    using var context = new MyDatabaseContext(_connectionString);
                    var appointments = context.Запись
                        .Include(z => z.Пациент)
                        .Include(z => z.Врач)
                        .ThenInclude(d => d.Пользователь)
                        .Include(z => z.Врач.Специализация)
                        .AsNoTracking()
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"✅ Загружено {appointments.Count} записей");
                    return appointments;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения записей: {ex.Message}");
                    return new List<Запись>();
                }
            }
        }

        public Запись? Get(int id)
        {
            lock (_dbLock)
            {
                try
                {
                    using var context = new MyDatabaseContext(_connectionString);
                    var appointment = context.Запись
                        .Include(z => z.Пациент)
                        .Include(z => z.Врач)
                        .FirstOrDefault(z => z.id == id);

                    System.Diagnostics.Debug.WriteLine($"✅ Получена запись ID={id}: {appointment != null}");
                    return appointment;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения записи ID={id}: {ex.Message}");
                    return null;
                }
            }
        }

        public bool Add(Запись entity)
        {
            lock (_dbLock)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== Добавление записи ===");
                    System.Diagnostics.Debug.WriteLine($"Пациент ID: {entity.пациент_id}");
                    System.Diagnostics.Debug.WriteLine($"Врач ID: {entity.врач_id}");
                    System.Diagnostics.Debug.WriteLine($"Дата: {entity.дата_записи:dd.MM.yyyy}");
                    System.Diagnostics.Debug.WriteLine($"Время: {entity.время_записи}");

                    using var context = new MyDatabaseContext(_connectionString);

                    // Проверяем существование пациента
                    var patientExists = context.Пользователь
                        .Any(p => p.id == entity.пациент_id && p.роль == "пациент" && p.активен);

                    if (!patientExists)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Пациент с ID={entity.пациент_id} не найден или неактивен");
                        return false;
                    }

                    // Проверяем существование врача
                    var doctorExists = context.Врач
                        .Any(d => d.id == entity.врач_id && d.статус == "активен");

                    if (!doctorExists)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Врач с ID={entity.врач_id} не найден или неактивен");
                        return false;
                    }

                    // Проверяем, что дата записи не в прошлом
                    if (entity.дата_записи.Date < DateTime.Now.Date)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Дата записи {entity.дата_записи:dd.MM.yyyy} в прошлом");
                        return false;
                    }

                    // Проверяем, не занято ли время у врача на эту дату
                    var existingAppointment = context.Запись
                        .Any(a => a.врач_id == entity.врач_id
                               && a.дата_записи.Date == entity.дата_записи.Date
                               && a.время_записи == entity.время_записи
                               && a.статус != "отменена");

                    System.Diagnostics.Debug.WriteLine($"Проверка занятости времени: {existingAppointment}");

                    if (existingAppointment)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Время {entity.время_записи} уже занято у врача {entity.врач_id}");
                        return false;
                    }

                    var newAppointment = new Запись
                    {
                        пациент_id = entity.пациент_id,
                        врач_id = entity.врач_id,
                        дата_записи = entity.дата_записи,
                        время_записи = entity.время_записи,
                        симптомы = entity.симптомы ?? string.Empty,
                        диагноз = entity.диагноз ?? string.Empty,
                        рекомендации = entity.рекомендации ?? string.Empty,
                        статус = entity.статус ?? "запланирована",
                        дата_создания = DateTime.Now
                    };

                    context.Запись.Add(newAppointment);
                    var result = context.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"✅ Добавлена запись: ID={newAppointment.id}, " +
                        $"Пациент={newAppointment.пациент_id}, " +
                        $"Врач={newAppointment.врач_id}, " +
                        $"Дата={newAppointment.дата_записи:dd.MM.yyyy}, " +
                        $"Время={newAppointment.время_записи}, " +
                        $"Результат: {result} изменений");

                    return result > 0;
                }
                catch (DbUpdateException dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка базы данных при добавлении записи: {dbEx.Message}");

                    if (dbEx.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");

                        // Определяем тип ошибки
                        if (dbEx.InnerException.Message.Contains("FOREIGN KEY"))
                        {
                            System.Diagnostics.Debug.WriteLine("Причина: нарушение внешнего ключа");
                        }
                        else if (dbEx.InnerException.Message.Contains("UNIQUE"))
                        {
                            System.Diagnostics.Debug.WriteLine("Причина: нарушение уникальности");
                        }
                        else if (dbEx.InnerException.Message.Contains("locked"))
                        {
                            System.Diagnostics.Debug.WriteLine("Причина: база данных заблокирована");
                        }
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Общая ошибка добавления записи: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
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
                    System.Diagnostics.Debug.WriteLine($"=== Удаление записи ID={id} ===");

                    using var context = new MyDatabaseContext(_connectionString);
                    var appointment = context.Запись.Find(id);

                    if (appointment != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Найдена запись для удаления: ID={appointment.id}, " +
                            $"Пациент={appointment.пациент_id}, Врач={appointment.врач_id}");

                        context.Запись.Remove(appointment);
                        var result = context.SaveChanges();

                        System.Diagnostics.Debug.WriteLine($"✅ Удаление завершено. Изменений: {result}");
                        return result > 0;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Запись с ID={id} не найдена");
                        return false;
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка базы данных при удалении: {dbEx.Message}");

                    if (dbEx.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Общая ошибка удаления записи: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                    return false;
                }
            }
        }

        public bool Update(int id, Запись entity)
        {
            lock (_dbLock)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== Обновление записи ID={id} ===");

                    using var context = new MyDatabaseContext(_connectionString);
                    var appointment = context.Запись.Find(id);

                    if (appointment != null)
                    {
                        // Сохраняем оригинальные значения для логов
                        var oldStatus = appointment.статус;
                        var oldDiagnosis = appointment.диагноз;

                        // Обновляем только изменяемые поля
                        appointment.пациент_id = entity.пациент_id;
                        appointment.врач_id = entity.врач_id;
                        appointment.дата_записи = entity.дата_записи;
                        appointment.время_записи = entity.время_записи;
                        appointment.статус = entity.статус ?? "запланирована";
                        appointment.симптомы = entity.симптомы ?? string.Empty;
                        appointment.диагноз = entity.диагноз ?? string.Empty;
                        appointment.рекомендации = entity.рекомендации ?? string.Empty;

                        var result = context.SaveChanges();

                        System.Diagnostics.Debug.WriteLine($"✅ Обновлена запись ID={id}, " +
                            $"Статус: {oldStatus} → {appointment.статус}, " +
                            $"Изменений: {result}");
                        return result > 0;
                    }

                    System.Diagnostics.Debug.WriteLine($"❌ Запись с ID={id} не найдена для обновления");
                    return false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка обновления записи: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                    return false;
                }
            }
        }

        public List<Запись> GetPatientAppointments(int patientId)
        {
            lock (_dbLock)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== Получение записей пациента ID={patientId} ===");

                    using var context = new MyDatabaseContext(_connectionString);
                    var appointments = context.Запись
                        .Include(z => z.Врач)
                        .ThenInclude(d => d.Пользователь)
                        .Include(z => z.Врач.Специализация)
                        .Where(z => z.пациент_id == patientId)
                        .OrderByDescending(z => z.дата_записи)
                        .ThenBy(z => z.время_записи)
                        .AsNoTracking()
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"✅ Загружено {appointments.Count} записей для пациента {patientId}");
                    return appointments;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения записей пациента: {ex.Message}");
                    return new List<Запись>();
                }
            }
        }

        public List<Запись> GetDoctorAppointments(int doctorId)
        {
            lock (_dbLock)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== Получение записей врача ID={doctorId} ===");

                    using var context = new MyDatabaseContext(_connectionString);
                    var appointments = context.Запись
                        .Include(z => z.Пациент)
                        .Where(z => z.врач_id == doctorId)
                        .OrderByDescending(z => z.дата_записи)
                        .ThenBy(z => z.время_записи)
                        .AsNoTracking()
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"✅ Загружено {appointments.Count} записей для врача {doctorId}");
                    return appointments;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения записей врача: {ex.Message}");
                    return new List<Запись>();
                }
            }
        }

        public bool CancelAppointment(int id, string reason = "")
        {
            lock (_dbLock)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== Отмена записи ID={id} ===");
                    System.Diagnostics.Debug.WriteLine($"Причина: {reason}");

                    using var context = new MyDatabaseContext(_connectionString);
                    var appointment = context.Запись.Find(id);

                    if (appointment != null)
                    {
                        var oldStatus = appointment.статус;
                        appointment.статус = "отменена";

                        if (!string.IsNullOrEmpty(reason))
                        {
                            appointment.симптомы = $"{appointment.симптомы} (Причина отмены: {reason})";
                        }

                        var result = context.SaveChanges();

                        System.Diagnostics.Debug.WriteLine($"✅ Отменена запись ID={id}, " +
                            $"Статус: {oldStatus} → {appointment.статус}, " +
                            $"Изменений: {result}");
                        return result > 0;
                    }

                    System.Diagnostics.Debug.WriteLine($"❌ Запись с ID={id} не найдена для отмены");
                    return false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка отмены записи: {ex.Message}");
                    return false;
                }
            }
        }

        public bool CompleteAppointment(int id, string diagnosis, string recommendations)
        {
            lock (_dbLock)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== Завершение записи ID={id} ===");
                    System.Diagnostics.Debug.WriteLine($"Диагноз: {diagnosis}");
                    System.Diagnostics.Debug.WriteLine($"Рекомендации: {recommendations}");

                    using var context = new MyDatabaseContext(_connectionString);
                    var appointment = context.Запись.Find(id);

                    if (appointment != null)
                    {
                        var oldStatus = appointment.статус;
                        appointment.статус = "завершена";
                        appointment.диагноз = diagnosis ?? string.Empty;
                        appointment.рекомендации = recommendations ?? string.Empty;

                        var result = context.SaveChanges();

                        System.Diagnostics.Debug.WriteLine($"✅ Завершена запись ID={id}, " +
                            $"Статус: {oldStatus} → {appointment.статус}, " +
                            $"Изменений: {result}");
                        return result > 0;
                    }

                    System.Diagnostics.Debug.WriteLine($"❌ Запись с ID={id} не найдена для завершения");
                    return false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка завершения записи: {ex.Message}");
                    return false;
                }
            }
        }

        public List<Запись> GetAppointmentsByDate(DateTime date, int? doctorId = null)
        {
            lock (_dbLock)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"=== Получение записей на дату {date:dd.MM.yyyy} ===");
                    if (doctorId.HasValue)
                        System.Diagnostics.Debug.WriteLine($"Для врача ID={doctorId.Value}");

                    using var context = new MyDatabaseContext(_connectionString);
                    var query = context.Запись
                        .Include(z => z.Пациент)
                        .Include(z => z.Врач)
                        .ThenInclude(d => d.Пользователь)
                        .Where(z => z.дата_записи.Date == date.Date);

                    if (doctorId.HasValue)
                    {
                        query = query.Where(z => z.врач_id == doctorId.Value);
                    }

                    var appointments = query.OrderBy(z => z.время_записи).AsNoTracking().ToList();

                    System.Diagnostics.Debug.WriteLine($"✅ Загружено {appointments.Count} записей на {date:dd.MM.yyyy}");
                    return appointments;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка получения записей по дате: {ex.Message}");
                    return new List<Запись>();
                }
            }
        }

        public bool TestConnection()
        {
            lock (_dbLock)
            {
                try
                {
                    using var context = new MyDatabaseContext(_connectionString);
                    var canConnect = context.Database.CanConnect();

                    if (canConnect)
                    {
                        // Дополнительная проверка
                        var appointmentsCount = context.Запись.Count();
                        var patientsCount = context.Пользователь.Count(p => p.роль == "пациент");
                        var doctorsCount = context.Врач.Count();

                        System.Diagnostics.Debug.WriteLine($"🔌 Проверка подключения: УСПЕШНО");
                        System.Diagnostics.Debug.WriteLine($"   Записей: {appointmentsCount}");
                        System.Diagnostics.Debug.WriteLine($"   Пациентов: {patientsCount}");
                        System.Diagnostics.Debug.WriteLine($"   Врачей: {doctorsCount}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"🔌 Проверка подключения: НЕ УДАЛОСЬ");
                    }

                    return canConnect;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка подключения: {ex.Message}");
                    return false;
                }
            }
        }

        public bool IsTimeAvailable(int doctorId, DateTime date, TimeSpan time)
        {
            lock (_dbLock)
            {
                try
                {
                    using var context = new MyDatabaseContext(_connectionString);

                    var isAvailable = !context.Запись
                        .Any(a => a.врач_id == doctorId
                               && a.дата_записи.Date == date.Date
                               && a.время_записи == time
                               && a.статус != "отменена");

                    System.Diagnostics.Debug.WriteLine($"Проверка доступности времени {time} " +
                        $"для врача {doctorId} на {date:dd.MM.yyyy}: {(isAvailable ? "СВОБОДНО" : "ЗАНЯТО")}");

                    return isAvailable;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка проверки времени: {ex.Message}");
                    return false;
                }
            }
        }

        public void CleanOldAppointments()
        {
            lock (_dbLock)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== Очистка старых записей ===");

                    using var context = new MyDatabaseContext(_connectionString);

                    // Находим записи старше 30 дней со статусом "отменена" или "завершена"
                    var cutoffDate = DateTime.Now.AddDays(-30);
                    var oldAppointments = context.Запись
                        .Where(a => a.дата_записи < cutoffDate &&
                                   (a.статус == "отменена" || a.статус == "завершена"))
                        .ToList();

                    if (oldAppointments.Any())
                    {
                        System.Diagnostics.Debug.WriteLine($"Найдено {oldAppointments.Count} старых записей для удаления");
                        context.Запись.RemoveRange(oldAppointments);
                        var result = context.SaveChanges();
                        System.Diagnostics.Debug.WriteLine($"Удалено {result} старых записей");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Старых записей для удаления не найдено");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка очистки старых записей: {ex.Message}");
                }
            }
        }
    }
}