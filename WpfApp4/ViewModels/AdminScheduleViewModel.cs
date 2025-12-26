using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WpfApp4.Domain;
using WpfApp4.Models;
using WpfApp4.Models.Entities;
using WpfApp4.Utils;

namespace WpfApp4.ViewModels
{
    public class AdminScheduleViewModel : PropertyChangedBase
    {
        private readonly Пользователь _currentUser;
        private readonly AppointmentRepository _appointmentRepository;
        private readonly ScheduleRepository _scheduleRepository;
        private readonly ПользовательRepository _userRepository;
        private ObservableCollection<Запись> _appointments;
        private ObservableCollection<Расписание> _schedules;
        private ObservableCollection<Врач> _doctors;
        private ObservableCollection<Пользователь> _patients;
        private Запись _selectedAppointment;
        private Расписание _selectedSchedule;
        private Расписание _newSchedule = new Расписание();
        private Запись _newAppointment = new Запись();
        private Врач _selectedDoctorForSchedule;
        private Пользователь _selectedPatientForAppointment;
        private Врач _selectedDoctorForAppointment;
        private string _selectedDayOfWeek = "Понедельник";
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private DateTime _selectedDate = DateTime.Now;

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

        public ObservableCollection<Врач> Doctors
        {
            get => _doctors;
            set { _doctors = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Пользователь> Patients
        {
            get => _patients;
            set { _patients = value; OnPropertyChanged(); }
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

        public Расписание NewSchedule
        {
            get => _newSchedule;
            set { _newSchedule = value; OnPropertyChanged(); }
        }

        public Запись NewAppointment
        {
            get => _newAppointment;
            set { _newAppointment = value; OnPropertyChanged(); }
        }

        public Врач SelectedDoctorForSchedule
        {
            get => _selectedDoctorForSchedule;
            set
            {
                _selectedDoctorForSchedule = value;
                OnPropertyChanged();
                if (value != null)
                {
                    NewSchedule.врач_id = value.id;
                }
            }
        }

        public Пользователь SelectedPatientForAppointment
        {
            get => _selectedPatientForAppointment;
            set
            {
                _selectedPatientForAppointment = value;
                OnPropertyChanged();
                if (value != null)
                {
                    NewAppointment.пациент_id = value.id;
                }
            }
        }

        public Врач SelectedDoctorForAppointment
        {
            get => _selectedDoctorForAppointment;
            set
            {
                _selectedDoctorForAppointment = value;
                OnPropertyChanged();
                if (value != null)
                {
                    NewAppointment.врач_id = value.id;
                }
            }
        }

        public string SelectedDayOfWeek
        {
            get => _selectedDayOfWeek;
            set
            {
                _selectedDayOfWeek = value;
                OnPropertyChanged();
                NewSchedule.день_недели = value;
            }
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

        public string WelcomeMessage => $"Управление записями и расписанием ({_currentUser.ПолноеИмя})";

        public ObservableCollection<string> DaysOfWeek { get; } = new ObservableCollection<string>
        {
            "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье"
        };

        public MyCommand DeleteAppointmentCommand { get; }
        public MyCommand DeleteScheduleCommand { get; }
        public MyCommand AddScheduleCommand { get; }
        public MyCommand AddAppointmentCommand { get; }
        public MyCommand ApproveCancelCommand { get; }
        public MyCommand BackCommand { get; }
        public MyCommand RefreshCommand { get; }

        public event Action BackRequested;

        public AdminScheduleViewModel(Пользователь currentUser)
        {
            _currentUser = currentUser;
            _appointmentRepository = new AppointmentRepository();
            _scheduleRepository = new ScheduleRepository();
            _userRepository = new ПользовательRepository();

            DeleteAppointmentCommand = new MyCommand(_ => DeleteAppointment());
            DeleteScheduleCommand = new MyCommand(_ => DeleteSchedule());
            AddScheduleCommand = new MyCommand(_ => AddSchedule());
            AddAppointmentCommand = new MyCommand(_ => AddAppointment());
            ApproveCancelCommand = new MyCommand(_ => ApproveCancel());
            BackCommand = new MyCommand(_ => BackRequested?.Invoke());
            RefreshCommand = new MyCommand(_ => LoadData());

            InitializeNewSchedule();
            InitializeNewAppointment();
            LoadData();
        }

        private void InitializeNewSchedule()
        {
            NewSchedule = new Расписание
            {
                день_недели = SelectedDayOfWeek,
                время_начала = new TimeSpan(9, 0, 0),
                время_окончания = new TimeSpan(18, 0, 0),
                перерыв_начала = new TimeSpan(13, 0, 0),
                перерыв_окончания = new TimeSpan(14, 0, 0),
                активен = true
            };
        }

        private void InitializeNewAppointment()
        {
            NewAppointment = new Запись
            {
                дата_записи = DateTime.Now.AddDays(1),
                время_записи = new TimeSpan(9, 0, 0),
                статус = "запланирована",
                дата_создания = DateTime.Now
            };
        }

        private void LoadData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Загрузка данных администратора ===");

                var appointments = _appointmentRepository.GetAll().ToList();
                Appointments = new ObservableCollection<Запись>(appointments);
                System.Diagnostics.Debug.WriteLine($"Записей в системе: {appointments.Count}");

                var schedules = _scheduleRepository.GetAll().ToList();
                Schedules = new ObservableCollection<Расписание>(schedules);
                System.Diagnostics.Debug.WriteLine($"Расписаний в системе: {schedules.Count}");

                LoadDoctors();
                LoadPatients();

                // Принудительно обновляем UI
                OnPropertyChanged(nameof(Appointments));
                OnPropertyChanged(nameof(Schedules));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки данных: {ex.Message}");
                Appointments = new ObservableCollection<Запись>();
                Schedules = new ObservableCollection<Расписание>();
            }
        }


        private void LoadAppointmentsForDate()
        {
            try
            {
                var appointments = _appointmentRepository.GetAppointmentsByDate(SelectedDate);
                Appointments = new ObservableCollection<Запись>(appointments);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки записей по дате: {ex.Message}");
            }
        }

        private void LoadDoctors()
        {
            try
            {
                using var context = new MyDatabaseContext(ConnectionManager.GetConnectionString());
                var doctors = context.Врач
                    .Include(d => d.Пользователь)
                    .Include(d => d.Специализация)
                    .Where(d => d.статус == "активен")
                    .ToList();
                Doctors = new ObservableCollection<Врач>(doctors);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки врачей: {ex.Message}");
                Doctors = new ObservableCollection<Врач>();
            }
        }

        private void LoadPatients()
        {
            try
            {
                using var context = new MyDatabaseContext("Data Source=medicalclinic.db");
                var patients = context.Пользователь
                    .Where(p => p.роль == "пациент" && p.активен)
                    .ToList();
                Patients = new ObservableCollection<Пользователь>(patients);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки пациентов: {ex.Message}");
                Patients = new ObservableCollection<Пользователь>();
            }
        }


        private void DeleteAppointment()
        {
            if (SelectedAppointment == null)
            {
                ErrorMessage = "Выберите запись для удаления";
                System.Diagnostics.Debug.WriteLine("❌ Не выбрана запись для удаления");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"=== Удаление записи администратором ===");
            System.Diagnostics.Debug.WriteLine($"ID записи: {SelectedAppointment.id}");
            System.Diagnostics.Debug.WriteLine($"Пациент: {SelectedAppointment.пациент_id}");
            System.Diagnostics.Debug.WriteLine($"Врач: {SelectedAppointment.врач_id}");

            // Подтверждение (опционально, можно добавить MessageBox)
            if (MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (_appointmentRepository.Remove(SelectedAppointment.id))
            {
                SuccessMessage = "Запись успешно удалена";
                System.Diagnostics.Debug.WriteLine($"✅ Запись ID={SelectedAppointment.id} удалена");

                // Обновляем список
                LoadAppointmentsForDate();

                // Сбрасываем выделение
                SelectedAppointment = null;
            }
            else
            {
                ErrorMessage = "Ошибка при удалении записи";
                System.Diagnostics.Debug.WriteLine($"❌ Не удалось удалить запись ID={SelectedAppointment.id}");
            }
        }


        private void DeleteSchedule()
        {
            if (SelectedSchedule == null)
            {
                ErrorMessage = "Выберите расписание для удаления";
                return;
            }

            if (_scheduleRepository.Remove(SelectedSchedule.id))
            {
                SuccessMessage = "Расписание удалено";
                LoadData();
            }
            else
            {
                ErrorMessage = "Ошибка при удалении расписания";
            }
        }

        private void AddSchedule()
        {
            try
            {
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                if (SelectedDoctorForSchedule == null)
                {
                    ErrorMessage = "Выберите врача";
                    return;
                }

                if (NewSchedule.время_начала >= NewSchedule.время_окончания)
                {
                    ErrorMessage = "Время начала должно быть раньше времени окончания";
                    return;
                }

                NewSchedule.врач_id = SelectedDoctorForSchedule.id;
                NewSchedule.день_недели = SelectedDayOfWeek;

                if (_scheduleRepository.Add(NewSchedule))
                {
                    SuccessMessage = "Расписание добавлено";
                    InitializeNewSchedule();
                    LoadData();
                }
                else
                {
                    ErrorMessage = "Ошибка при добавлении расписания";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
        }

        private void AddAppointment()
        {
            try
            {
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;

                if (SelectedPatientForAppointment == null)
                {
                    ErrorMessage = "Выберите пациента";
                    return;
                }

                if (SelectedDoctorForAppointment == null)
                {
                    ErrorMessage = "Выберите врача";
                    return;
                }

                NewAppointment.пациент_id = SelectedPatientForAppointment.id;
                NewAppointment.врач_id = SelectedDoctorForAppointment.id;

                if (_appointmentRepository.Add(NewAppointment))
                {
                    SuccessMessage = "Запись добавлена";
                    InitializeNewAppointment();
                    LoadData();
                }
                else
                {
                    ErrorMessage = "Ошибка при добавлении записи";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
        }

        private void ApproveCancel()
        {
            if (SelectedAppointment == null)
            {
                ErrorMessage = "Выберите запись";
                return;
            }

            if (SelectedAppointment.статус == "отметка_об_отмене")
            {
                SelectedAppointment.статус = "отменена";
                if (_appointmentRepository.Update(SelectedAppointment.id, SelectedAppointment))
                {
                    SuccessMessage = "Отмена записи подтверждена";
                    LoadAppointmentsForDate();
                }
                else
                {
                    ErrorMessage = "Ошибка при подтверждении отмены";
                }
            }
            else
            {
                ErrorMessage = "Запись не отмечена для отмены";
            }
        }
    }
}