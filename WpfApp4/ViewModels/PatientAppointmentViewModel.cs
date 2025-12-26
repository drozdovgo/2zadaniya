using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WpfApp4.Domain;
using WpfApp4.Models;
using WpfApp4.Models.Entities;
using WpfApp4.Utils;

namespace WpfApp4.ViewModels
{
    public class PatientAppointmentViewModel : PropertyChangedBase
    {
        private readonly Пользователь _currentUser;
        private readonly AppointmentRepository _appointmentRepository;
        private ObservableCollection<Запись> _appointments;
        private ObservableCollection<Врач> _doctors;
        private Врач _selectedDoctor;
        private DateTime _selectedDate = DateTime.Now.AddDays(1);
        private TimeSpan _selectedTime = new TimeSpan(9, 0, 0);
        private string _symptoms = string.Empty;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private Запись _selectedAppointment;

        public ObservableCollection<Запись> Appointments
        {
            get => _appointments;
            set { _appointments = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Врач> Doctors
        {
            get => _doctors;
            set { _doctors = value; OnPropertyChanged(); }
        }

        public Врач SelectedDoctor
        {
            get => _selectedDoctor;
            set
            {
                _selectedDoctor = value;
                OnPropertyChanged();
                if (value != null)
                    System.Diagnostics.Debug.WriteLine($"Выбран врач: ID={value.id}, Имя={value.Пользователь?.ПолноеИмя}");
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                System.Diagnostics.Debug.WriteLine($"Выбрана дата: {value:dd.MM.yyyy}");
            }
        }

        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set
            {
                _selectedTime = value;
                OnPropertyChanged();
                System.Diagnostics.Debug.WriteLine($"Выбрано время: {value:hh\\:mm}");
            }
        }

        public string Symptoms
        {
            get => _symptoms;
            set { _symptoms = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set { _successMessage = value; OnPropertyChanged(); }
        }

        public Запись SelectedAppointment
        {
            get => _selectedAppointment;
            set { _selectedAppointment = value; OnPropertyChanged(); }
        }

        public string WelcomeMessage => $"Мои записи ({_currentUser.ПолноеИмя})";

        public ObservableCollection<TimeSpan> AvailableTimes { get; } = new ObservableCollection<TimeSpan>
        {
            new TimeSpan(9, 0, 0),
            new TimeSpan(9, 30, 0),
            new TimeSpan(10, 0, 0),
            new TimeSpan(10, 30, 0),
            new TimeSpan(11, 0, 0),
            new TimeSpan(11, 30, 0),
            new TimeSpan(12, 0, 0),
            new TimeSpan(13, 0, 0),
            new TimeSpan(13, 30, 0),
            new TimeSpan(14, 0, 0),
            new TimeSpan(14, 30, 0),
            new TimeSpan(15, 0, 0),
            new TimeSpan(15, 30, 0),
            new TimeSpan(16, 0, 0),
            new TimeSpan(16, 30, 0)
        };

        public MyCommand CreateAppointmentCommand { get; }
        public MyCommand CancelAppointmentCommand { get; }
        public MyCommand BackCommand { get; }
        public MyCommand RefreshCommand { get; }

        public event Action BackRequested;

        public PatientAppointmentViewModel(Пользователь currentUser)
        {
            _currentUser = currentUser;
            _appointmentRepository = new AppointmentRepository();

            System.Diagnostics.Debug.WriteLine($"=== Инициализация PatientAppointmentViewModel ===");
            System.Diagnostics.Debug.WriteLine($"Пациент: ID={_currentUser.id}, Имя={_currentUser.ПолноеИмя}");

            CreateAppointmentCommand = new MyCommand(_ => CreateAppointment());
            CancelAppointmentCommand = new MyCommand(_ => CancelAppointment());
            BackCommand = new MyCommand(_ => BackRequested?.Invoke());
            RefreshCommand = new MyCommand(_ => LoadData());

            LoadData();
            LoadDoctors();

            // Проверяем базу данных
            CheckDatabase();
        }

        private void LoadData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Загрузка записей пациента ===");
                System.Diagnostics.Debug.WriteLine($"ID пациента: {_currentUser.id}");

                var appointments = _appointmentRepository.GetPatientAppointments(_currentUser.id);

                System.Diagnostics.Debug.WriteLine($"Найдено записей: {appointments.Count}");
                foreach (var app in appointments)
                {
                    System.Diagnostics.Debug.WriteLine($"  Запись ID={app.id}, " +
                        $"Врач={app.Врач?.Пользователь?.ПолноеИмя ?? "Нет данных"}, " +
                        $"Дата={app.дата_записи:dd.MM.yyyy}, " +
                        $"Время={app.время_записи:hh\\:mm}, " +
                        $"Статус={app.статус}");
                }

                Appointments = new ObservableCollection<Запись>(appointments);

                // Принудительно обновляем UI
                OnPropertyChanged(nameof(Appointments));

                if (appointments.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ У пациента нет записей");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки записей: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                Appointments = new ObservableCollection<Запись>();
            }
        }

        private void LoadDoctors()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Загрузка врачей ===");

                using var context = new MyDatabaseContext(ConnectionManager.GetConnectionString());

                var doctors = context.Врач
                    .Include(d => d.Пользователь)
                    .Include(d => d.Специализация)
                    .Where(d => d.статус == "активен" && d.Пользователь != null)
                    .ToList();

                Doctors = new ObservableCollection<Врач>(doctors);
                System.Diagnostics.Debug.WriteLine($"✅ Загружено {doctors.Count} врачей");

                // Логируем детали врачей
                foreach (var doctor in doctors)
                {
                    System.Diagnostics.Debug.WriteLine($"  Врач ID={doctor.id}: " +
                        $"{doctor.Пользователь?.ПолноеИмя ?? "Нет данных"}, " +
                        $"Специализация: {doctor.Специализация?.название ?? "Нет данных"}");
                }

                // Выбираем первого врача по умолчанию
                if (doctors.Count > 0)
                {
                    SelectedDoctor = doctors[0];
                    System.Diagnostics.Debug.WriteLine($"Выбран врач по умолчанию: {SelectedDoctor.Пользователь?.ПолноеИмя}");
                }
                else
                {
                    ErrorMessage = "Нет доступных врачей";
                    System.Diagnostics.Debug.WriteLine("⚠️ Нет доступных врачей");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки врачей: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                Doctors = new ObservableCollection<Врач>();
                ErrorMessage = $"Ошибка загрузки врачей: {ex.Message}";
            }
        }

        private void CreateAppointment()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Создание записи ===");
                System.Diagnostics.Debug.WriteLine($"Пациент ID: {_currentUser.id}");
                System.Diagnostics.Debug.WriteLine($"Выбран врач ID: {SelectedDoctor?.id}");
                System.Diagnostics.Debug.WriteLine($"Дата: {SelectedDate:dd.MM.yyyy}");
                System.Diagnostics.Debug.WriteLine($"Время: {SelectedTime:hh\\:mm}");
                System.Diagnostics.Debug.WriteLine($"Симптомы: {Symptoms}");

                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                // Валидация
                if (SelectedDoctor == null)
                {
                    ErrorMessage = "Выберите врача";
                    System.Diagnostics.Debug.WriteLine("❌ Врач не выбран");
                    return;
                }

                if (SelectedDate < DateTime.Today)
                {
                    ErrorMessage = "Нельзя записаться на прошедшую дату";
                    System.Diagnostics.Debug.WriteLine($"❌ Дата {SelectedDate:dd.MM.yyyy} в прошлом");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Symptoms))
                {
                    ErrorMessage = "Опишите симптомы";
                    System.Diagnostics.Debug.WriteLine("❌ Симптомы не указаны");
                    return;
                }

                // Проверяем доступность времени через репозиторий
                if (!_appointmentRepository.IsTimeAvailable(SelectedDoctor.id, SelectedDate, SelectedTime))
                {
                    ErrorMessage = "Выбранное время уже занято. Пожалуйста, выберите другое время.";
                    System.Diagnostics.Debug.WriteLine($"❌ Время {SelectedTime} уже занято");
                    return;
                }

                // Проверяем, что врач активен
                using (var context = new MyDatabaseContext(ConnectionManager.GetConnectionString()))
                {
                    var doctor = context.Врач.FirstOrDefault(d => d.id == SelectedDoctor.id);
                    if (doctor == null || doctor.статус != "активен")
                    {
                        ErrorMessage = "Выбранный врач не активен или не существует";
                        System.Diagnostics.Debug.WriteLine($"❌ Врач ID={SelectedDoctor.id} не активен или не найден");
                        return;
                    }
                }

                // Создаем новую запись
                var newAppointment = new Запись
                {
                    пациент_id = _currentUser.id,
                    врач_id = SelectedDoctor.id,
                    дата_записи = SelectedDate.Date, // Убедимся, что это только дата
                    время_записи = SelectedTime,
                    симптомы = Symptoms.Trim(),
                    статус = "запланирована",
                    дата_создания = DateTime.Now
                };

                System.Diagnostics.Debug.WriteLine("Попытка сохранения записи...");

                if (_appointmentRepository.Add(newAppointment))
                {
                    SuccessMessage = "Запись успешно создана!";
                    Symptoms = string.Empty;

                    // Обновляем список записей
                    LoadData();

                    // Также перезагружаем врачей на случай изменений
                    LoadDoctors();

                    System.Diagnostics.Debug.WriteLine("✅ Запись успешно создана");

                    // Показываем сообщение 5 секунд
                    Task.Delay(5000).ContinueWith(t =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            SuccessMessage = string.Empty;
                        });
                    });
                }
                else
                {
                    ErrorMessage = "Ошибка при создании записи. Пожалуйста, попробуйте позже.";
                    System.Diagnostics.Debug.WriteLine("❌ Ошибка при создании записи (Add вернул false)");
                }
            }
            catch (DbUpdateException dbEx)
            {
                string detailedError = GetDetailedErrorMessage(dbEx);
                ErrorMessage = $"Ошибка базы данных: {detailedError}";
                System.Diagnostics.Debug.WriteLine($"❌ DbUpdateException: {detailedError}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {dbEx.StackTrace}");

                // Показываем более понятное сообщение пользователю
                if (detailedError.Contains("FOREIGN KEY constraint failed"))
                {
                    ErrorMessage = "Ошибка: выбранный врач не существует или не активен";
                }
                else if (detailedError.Contains("UNIQUE constraint failed"))
                {
                    ErrorMessage = "Ошибка: на это время уже есть запись";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        private string GetDetailedErrorMessage(DbUpdateException dbEx)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(dbEx.Message);

            var inner = dbEx.InnerException;
            while (inner != null)
            {
                sb.AppendLine($"  -> {inner.Message}");
                inner = inner.InnerException;
            }

            return sb.ToString();
        }

        private void CancelAppointment()
        {
            if (SelectedAppointment == null)
            {
                ErrorMessage = "Выберите запись для отмены";
                System.Diagnostics.Debug.WriteLine("❌ Не выбрана запись для отмены");
                return;
            }

            if (SelectedAppointment.статус == "отменена")
            {
                ErrorMessage = "Эта запись уже отменена";
                System.Diagnostics.Debug.WriteLine($"❌ Запись ID={SelectedAppointment.id} уже отменена");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"=== Отмена записи пациентом ===");
            System.Diagnostics.Debug.WriteLine($"Запись ID: {SelectedAppointment.id}");

            if (_appointmentRepository.CancelAppointment(SelectedAppointment.id, "Отменено пациентом"))
            {
                SuccessMessage = "Запись отменена";
                System.Diagnostics.Debug.WriteLine($"✅ Запись ID={SelectedAppointment.id} отменена");

                // Обновляем список
                LoadData();

                // Показываем сообщение 5 секунд
                Task.Delay(5000).ContinueWith(t =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        SuccessMessage = string.Empty;
                    });
                });
            }
            else
            {
                ErrorMessage = "Ошибка при отмене записи";
                System.Diagnostics.Debug.WriteLine($"❌ Не удалось отменить запись ID={SelectedAppointment.id}");
            }
        }

        private void CheckDatabase()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Проверка базы данных ===");

                using var context = new MyDatabaseContext(ConnectionManager.GetConnectionString());

                // Проверяем таблицы
                var userCount = context.Пользователь.Count();
                var doctorCount = context.Врач.Count();
                var appointmentCount = context.Запись.Count();

                System.Diagnostics.Debug.WriteLine($"Всего пользователей: {userCount}");
                System.Diagnostics.Debug.WriteLine($"Всего врачей: {doctorCount}");
                System.Diagnostics.Debug.WriteLine($"Всего записей: {appointmentCount}");

                // Проверяем конкретного пациента
                var patient = context.Пользователь.FirstOrDefault(p => p.id == _currentUser.id);
                if (patient != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Пациент найден: ID={patient.id}, {patient.ПолноеИмя}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Пациент ID={_currentUser.id} не найден в БД");
                }

                // Проверяем все записи пациента
                var allPatientApps = context.Запись
                    .Where(z => z.пациент_id == _currentUser.id)
                    .Include(z => z.Врач)
                        .ThenInclude(d => d.Пользователь)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Записей пациента {_currentUser.id}: {allPatientApps.Count}");
                foreach (var app in allPatientApps)
                {
                    System.Diagnostics.Debug.WriteLine($"  ID={app.id}, " +
                        $"Врач={app.Врач?.Пользователь?.ПолноеИмя ?? "Нет"}, " +
                        $"Дата={app.дата_записи:dd.MM.yyyy}, " +
                        $"Время={app.время_записи:hh\\:mm}, " +
                        $"Статус={app.статус}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка проверки БД: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
    }
}