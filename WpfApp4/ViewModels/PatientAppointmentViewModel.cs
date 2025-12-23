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
            set { _selectedDoctor = value; OnPropertyChanged(); }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); }
        }

        public TimeSpan SelectedTime
        {
            get => _selectedTime;
            set { _selectedTime = value; OnPropertyChanged(); }
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

            CreateAppointmentCommand = new MyCommand(_ => CreateAppointment());
            CancelAppointmentCommand = new MyCommand(_ => CancelAppointment());
            BackCommand = new MyCommand(_ => BackRequested?.Invoke());
            RefreshCommand = new MyCommand(_ => LoadData());

            LoadData();
            LoadDoctors();
        }

        private void LoadData()
        {
            try
            {
                var appointments = _appointmentRepository.GetPatientAppointments(_currentUser.id);
                Appointments = new ObservableCollection<Запись>(appointments);
                System.Diagnostics.Debug.WriteLine($"✅ Загружено {appointments.Count} записей для пациента {_currentUser.id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки записей: {ex.Message}");
                Appointments = new ObservableCollection<Запись>();
            }
        }

        private void LoadDoctors()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Загрузка врачей для пациента ===");

                using var context = new MyDatabaseContext(ConnectionManager.GetConnectionString());

                var doctors = context.Врач
                    .Include(d => d.Пользователь)  // Включаем данные пользователя
                    .Include(d => d.Специализация) // Включаем специализацию
                    .Where(d => d.статус == "активен")
                    .ToList();

                Doctors = new ObservableCollection<Врач>(doctors);
                System.Diagnostics.Debug.WriteLine($"✅ Загружено {doctors.Count} врачей");

                // Логируем детали врачей
                foreach (var doctor in doctors)
                {
                    System.Diagnostics.Debug.WriteLine($"  Врач {doctor.id}: " +
                        $"{doctor.Пользователь?.ПолноеИмя ?? "Нет данных"}, " +
                        $"Специализация: {doctor.Специализация?.название ?? "Нет данных"}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки врачей: {ex.Message}");
                Doctors = new ObservableCollection<Врач>();
            }
        }


        private void CreateAppointment()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== Создание записи пациентом ===");

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

                // Создаем новую запись
                var newAppointment = new Запись
                {
                    пациент_id = _currentUser.id,
                    врач_id = SelectedDoctor.id,
                    дата_записи = SelectedDate,
                    время_записи = SelectedTime,
                    симптомы = Symptoms,
                    диагноз = string.Empty,
                    рекомендации = string.Empty,
                    статус = "запланирована",
                    дата_создания = DateTime.Now
                };

                System.Diagnostics.Debug.WriteLine($"Отправка записи в репозиторий: " +
                    $"Пациент={_currentUser.id}, " +
                    $"Врач={SelectedDoctor.id}, " +
                    $"Дата={SelectedDate:dd.MM.yyyy}, " +
                    $"Время={SelectedTime}");

                if (_appointmentRepository.Add(newAppointment))
                {
                    SuccessMessage = "Запись успешно создана!";
                    Symptoms = string.Empty;

                    // Обновляем список записей
                    LoadData();

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
                    ErrorMessage = "Ошибка при создании записи. Возможно:\n" +
                                 "1. Выбранное время уже занято\n" +
                                 "2. Врач не активен\n" +
                                 "3. Проблема с подключением к базе данных";
                    System.Diagnostics.Debug.WriteLine("❌ Ошибка при создании записи (вернулось false)");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ Исключение при создании записи: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
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

            if (SelectedAppointment.дата_записи < DateTime.Now)
            {
                ErrorMessage = "Нельзя отменить прошедшую запись";
                System.Diagnostics.Debug.WriteLine($"❌ Запись ID={SelectedAppointment.id} уже прошла");
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
    }
}