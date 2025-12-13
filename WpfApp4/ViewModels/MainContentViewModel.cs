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
        private readonly MyDatabaseContext _dbContext;
        private ObservableCollection<Пользователь> _users;

        public Пользователь CurrentUser => _currentUser;
        public ObservableCollection<Пользователь> Users
        {
            get => _users;
            set { _users = value; OnPropertyChanged(); }
        }

        public string WelcomeMessage => $"Добро пожаловать, {_currentUser.ПолноеИмя} ({_currentUser.роль})";

        // Команды
        public MyCommand LogoutCommand { get; }
        public MyCommand RefreshCommand { get; }
        public MyCommand ViewDoctorSpecializationsCommand { get; }

        // События
        public event Action? LogoutRequested;
        public event Action? ViewDoctorSpecializationsRequested;

        public MainContentViewModel(Пользователь currentUser)
        {
            _currentUser = currentUser;
            _dbContext = new MyDatabaseContext("Data Source=medicalclinic.db");

            LogoutCommand = new MyCommand(_ => LogoutRequested?.Invoke());
            RefreshCommand = new MyCommand(_ => RefreshData());
            ViewDoctorSpecializationsCommand = new MyCommand(_ => ViewDoctorSpecializationsRequested?.Invoke());

            RefreshData();
        }

        private void RefreshData()
        {
            try
            {
                var filteredUsers = _dbContext.Пользователь
                    .Where(u => u.роль == "врач" || u.роль == "администратор")
                    .ToList();

                // Отладочный вывод
                System.Diagnostics.Debug.WriteLine($"=== MainContentView ===");
                System.Diagnostics.Debug.WriteLine($"Всего пользователей в БД: {_dbContext.Пользователь.Count()}");
                System.Diagnostics.Debug.WriteLine($"Из них врачей: {_dbContext.Пользователь.Count(u => u.роль == "врач")}");
                System.Diagnostics.Debug.WriteLine($"Из них администраторов: {_dbContext.Пользователь.Count(u => u.роль == "администратор")}");
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