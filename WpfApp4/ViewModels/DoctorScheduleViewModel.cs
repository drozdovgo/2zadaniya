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
    public class DoctorScheduleViewModel : PropertyChangedBase
    {
        private readonly Пользователь _currentUser;
        private readonly AppointmentRepository _appointmentRepository;
        private readonly ScheduleRepository _scheduleRepository;
        private ObservableCollection<Запись> _appointments;
        private ObservableCollection<Расписание> _schedules;
        private Запись _selectedAppointment;
        private Расписание _selectedSchedule;
        private string _cancelReason = string.Empty;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private string _diagnosis = string.Empty;
        private string _recommendations = string.Empty;
        private DateTime _selectedDate = DateTime.Now;
        private Врач _currentDoctor;

        public ObservableCollection<Запись> Appointments
        {
            get => _appointments;
            set { _appointments = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Расписание> Schedules
        {
            get => _schedules;
            set { _schedules = value; OnPropertyChanged(); }
        }

        public Запись SelectedAppointment
        {
            get => _selectedAppointment;
            set { _selectedAppointment = value; OnPropertyChanged(); }
        }

        public Расписание SelectedSchedule
        {
            get => _selectedSchedule;
            set { _selectedSchedule = value; OnPropertyChanged(); }
        }

        public string CancelReason
        {
            get => _cancelReason;
            set { _cancelReason = value; OnPropertyChanged(); }
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

        public string Diagnosis
        {
            get => _diagnosis;
            set { _diagnosis = value; OnPropertyChanged(); }
        }

        public string Recommendations
        {
            get => _recommendations;
            set { _recommendations = value; OnPropertyChanged(); }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                LoadAppointmentsForDate();
            }
        }

        public string WelcomeMessage => $"Расписание и записи ({_currentUser.ПолноеИмя})";

        public MyCommand CancelAppointmentCommand { get; }
        public MyCommand CompleteAppointmentCommand { get; }
        public MyCommand MarkForCancelCommand { get; }
        public MyCommand ToggleScheduleCommand { get; }
        public MyCommand BackCommand { get; }
        public MyCommand RefreshCommand { get; }

        public event Action BackRequested;

        public DoctorScheduleViewModel(Пользователь currentUser)
        {
            _currentUser = currentUser;
            _appointmentRepository = new AppointmentRepository();
            _scheduleRepository = new ScheduleRepository();
            LoadCurrentDoctor();

            CancelAppointmentCommand = new MyCommand(_ => CancelAppointment());
            CompleteAppointmentCommand = new MyCommand(_ => CompleteAppointment());
            MarkForCancelCommand = new MyCommand(_ => MarkForCancel());
            ToggleScheduleCommand = new MyCommand(_ => ToggleSchedule());
            BackCommand = new MyCommand(_ => BackRequested?.Invoke());
            RefreshCommand = new MyCommand(_ => LoadData());

            // Отложенная загрузка данных после инициализации
            LoadData();
        }


        private void LoadCurrentDoctor()
        {
            try
            {
                using var context = new MyDatabaseContext("Data Source=medicalclinic.db");
                _currentDoctor = context.Врач
                    .Include(d => d.Пользователь)      // Включаем данные пользователя
                    .Include(d => d.Специализация)     // Включаем специализацию
                    .FirstOrDefault(d => d.пользователь_id == _currentUser.id);

                if (_currentDoctor != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Загружен врач: ID={_currentDoctor.id}, " +
                        $"{_currentDoctor.Пользователь?.ПолноеИмя ?? "Нет данных"}, " +
                        $"Специализация: {_currentDoctor.Специализация?.название ?? "Нет данных"}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Врач не найден для пользователя {_currentUser.id}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки врача: {ex.Message}");
            }
        }


        private void LoadData()
        {
            try
            {
                if (_currentDoctor != null)
                {
                    System.Diagnostics.Debug.WriteLine($"=== Загрузка данных врача ID={_currentDoctor.id} ===");

                    var appointments = _appointmentRepository.GetDoctorAppointments(_currentDoctor.id);
                    Appointments = new ObservableCollection<Запись>(appointments);
                    System.Diagnostics.Debug.WriteLine($"✅ Загружено {appointments.Count} записей для врача {_currentDoctor.id}");

                    var schedules = _scheduleRepository.GetDoctorSchedule(_currentDoctor.id);
                    Schedules = new ObservableCollection<Расписание>(schedules);
                    System.Diagnostics.Debug.WriteLine($"✅ Загружено {schedules.Count} расписаний для врача {_currentDoctor.id}");

                    // Принудительно обновляем UI
                    OnPropertyChanged(nameof(Appointments));
                    OnPropertyChanged(nameof(Schedules));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ Текущий пользователь не является врачом");
                    Appointments = new ObservableCollection<Запись>();
                    Schedules = new ObservableCollection<Расписание>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки данных: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                Appointments = new ObservableCollection<Запись>();
                Schedules = new ObservableCollection<Расписание>();
            }
        }




        private void LoadAppointmentsForDate()
        {
            try
            {
                if (_currentDoctor != null)
                {
                    var appointments = _appointmentRepository.GetAppointmentsByDate(SelectedDate, _currentDoctor.id);
                    Appointments = new ObservableCollection<Запись>(appointments);
                    System.Diagnostics.Debug.WriteLine($"Загружено {appointments.Count} записей на {SelectedDate.ToShortDateString()}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки записей по дате: {ex.Message}");
            }
        }

        private void CancelAppointment()
        {
            System.Diagnostics.Debug.WriteLine($"=== Отмена записи врачом ===");
            System.Diagnostics.Debug.WriteLine($"Запись ID: {SelectedAppointment?.id}");

            if (SelectedAppointment == null)
            {
                ErrorMessage = "Выберите запись для отмены";
                return;
            }

            if (SelectedAppointment.статус == "отменена")
            {
                ErrorMessage = "Эта запись уже отменена";
                return;
            }

            if (string.IsNullOrWhiteSpace(CancelReason))
            {
                ErrorMessage = "Укажите причину отмены";
                return;
            }

            if (_appointmentRepository.CancelAppointment(SelectedAppointment.id, CancelReason))
            {
                SuccessMessage = "Запись отменена";
                CancelReason = string.Empty;
                LoadAppointmentsForDate();

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
            }
        }

        private void CompleteAppointment()
        {
            System.Diagnostics.Debug.WriteLine($"=== Завершение записи врачом ===");
            System.Diagnostics.Debug.WriteLine($"Запись ID: {SelectedAppointment?.id}");

            if (SelectedAppointment == null)
            {
                ErrorMessage = "Выберите запись для завершения";
                return;
            }

            if (SelectedAppointment.статус == "завершена")
            {
                ErrorMessage = "Эта запись уже завершена";
                return;
            }

            if (string.IsNullOrWhiteSpace(Diagnosis))
            {
                ErrorMessage = "Укажите диагноз";
                return;
            }

            if (_appointmentRepository.CompleteAppointment(SelectedAppointment.id, Diagnosis, Recommendations))
            {
                SuccessMessage = "Запись завершена";
                Diagnosis = string.Empty;
                Recommendations = string.Empty;
                LoadAppointmentsForDate();

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
                ErrorMessage = "Ошибка при завершении записи";
            }
        }

        private void MarkForCancel()
        {
            System.Diagnostics.Debug.WriteLine($"=== Отметка записи для отмены врачом ===");
            System.Diagnostics.Debug.WriteLine($"Запись ID: {SelectedAppointment?.id}");

            if (SelectedAppointment == null)
            {
                ErrorMessage = "Выберите запись для отметки об отмене";
                return;
            }

            if (SelectedAppointment.статус != "запланирована")
            {
                ErrorMessage = "Можно отмечать только запланированные записи";
                return;
            }

            // Создаем копию объекта для обновления
            var updatedAppointment = new Запись
            {
                id = SelectedAppointment.id,
                пациент_id = SelectedAppointment.пациент_id,
                врач_id = SelectedAppointment.врач_id,
                дата_записи = SelectedAppointment.дата_записи,
                время_записи = SelectedAppointment.время_записи,
                симптомы = SelectedAppointment.симптомы,
                диагноз = SelectedAppointment.диагноз,
                рекомендации = SelectedAppointment.рекомендации,
                статус = "отметка_об_отмене",
                дата_создания = SelectedAppointment.дата_создания
            };

            if (_appointmentRepository.Update(SelectedAppointment.id, updatedAppointment))
            {
                SuccessMessage = "Запись отмечена для отмены администратором";
                LoadAppointmentsForDate();

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
                ErrorMessage = "Ошибка при отметке записи";
            }
        }

        private void ToggleSchedule()
        {
            System.Diagnostics.Debug.WriteLine($"=== Изменение статуса расписания ===");
            System.Diagnostics.Debug.WriteLine($"Расписание ID: {SelectedSchedule?.id}");

            if (SelectedSchedule == null)
            {
                ErrorMessage = "Выберите расписание для изменения";
                return;
            }

            try
            {
                // Создаем копию объекта для обновления
                var updatedSchedule = new Расписание
                {
                    id = SelectedSchedule.id,
                    врач_id = SelectedSchedule.врач_id,
                    день_недели = SelectedSchedule.день_недели,
                    время_начала = SelectedSchedule.время_начала,
                    время_окончания = SelectedSchedule.время_окончания,
                    перерыв_начала = SelectedSchedule.перерыв_начала,
                    перерыв_окончания = SelectedSchedule.перерыв_окончания,
                    активен = !SelectedSchedule.активен
                };

                if (_scheduleRepository.Update(SelectedSchedule.id, updatedSchedule))
                {
                    SuccessMessage = updatedSchedule.активен ?
                        "Расписание активировано" : "Расписание деактивировано";
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
                    ErrorMessage = "Ошибка при изменении расписания";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}
