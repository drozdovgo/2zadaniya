using System;
using System.Collections.ObjectModel;
using System.Linq;
using WpfApp4.Domain;
using WpfApp4.Models;
using WpfApp4.Models.Entities;
using WpfApp4.Utils;

namespace WpfApp4.ViewModels
{
    public class MainContentViewModel : PropertyChangedBase
    {
        private readonly Пользователь _currentUser;
        private ObservableCollection<Пользователь> _users;
        public Пользователь CurrentUser => _currentUser;
        public ObservableCollection<Пользователь> Users
        {
            get => _users;
            set { _users = value; OnPropertyChanged(); }
        }

        public string WelcomeMessage => $"Добро пожаловать, {_currentUser.ПолноеИмя} ({_currentUser.роль})";

        // Команды для всех пользователей
        public MyCommand LogoutCommand { get; }
        public MyCommand RefreshCommand { get; }

        // Команды для разных ролей
        public MyCommand ViewDoctorSpecializationsCommand { get; }
        public MyCommand ViewPatientAppointmentsCommand { get; }
        public MyCommand ViewDoctorScheduleCommand { get; }
        public MyCommand ViewAdminScheduleCommand { get; }

        // События
        public event Action? LogoutRequested;
        public event Action? ViewDoctorSpecializationsRequested;
        public event Action? ViewPatientAppointmentsRequested;
        public event Action? ViewDoctorScheduleRequested;
        public event Action? ViewAdminScheduleRequested;

        public MainContentViewModel(Пользователь currentUser)
        {
            _currentUser = currentUser;

            LogoutCommand = new MyCommand(_ => LogoutRequested?.Invoke());
            RefreshCommand = new MyCommand(_ => RefreshData());
            ViewDoctorSpecializationsCommand = new MyCommand(_ => ViewDoctorSpecializationsRequested?.Invoke());

            // Инициализация команд в зависимости от роли
            if (_currentUser.роль == "пациент")
            {
                ViewPatientAppointmentsCommand = new MyCommand(_ => ViewPatientAppointmentsRequested?.Invoke());
            }
            else if (_currentUser.роль == "врач")
            {
                ViewDoctorScheduleCommand = new MyCommand(_ => ViewDoctorScheduleRequested?.Invoke());
            }
            else if (_currentUser.роль == "администратор")
            {
                ViewAdminScheduleCommand = new MyCommand(_ => ViewAdminScheduleRequested?.Invoke());
            }

            RefreshData();
        }

        private void RefreshData()
        {
            try
            {
                // Создаем новый контекст вместо использования _dbContext
                using var context = new MyDatabaseContext("Data Source=medicalclinic.db");

                var filteredUsers = context.Пользователь
                    .Where(u => u.роль == "врач" || u.роль == "администратор")
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"=== MainContentView ===");
                System.Diagnostics.Debug.WriteLine($"Всего пользователей в БД: {context.Пользователь.Count()}");
                System.Diagnostics.Debug.WriteLine($"Из них врачей: {context.Пользователь.Count(u => u.роль == "врач")}");
                System.Diagnostics.Debug.WriteLine($"Из них администраторов: {context.Пользователь.Count(u => u.роль == "администратор")}");
                System.Diagnostics.Debug.WriteLine($"Отображается: {filteredUsers.Count}");

                Users = new ObservableCollection<Пользователь>(filteredUsers);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка загрузки данных: {ex.Message}");
                Users = new ObservableCollection<Пользователь>();
            }
        }
    }
}