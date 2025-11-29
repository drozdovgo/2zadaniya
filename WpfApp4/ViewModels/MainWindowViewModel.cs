using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using WpfApp4.Interfaces;
using WpfApp4.Models;
using WpfApp4.Models.Entities;
using WpfApp4.Utils;

namespace WpfApp4.ViewModels
{
    public class MainWindowViewModel : PropertyChangedBase
    {
        private readonly IRepository<Пользователь> _userRepository; 
        private ObservableCollection<Пользователь> _users;

        public ObservableCollection<Пользователь> Users
        {
            get => _users;
            set
            {
                if (value != null)
                {
                    _users = value;
                    OnPropertyChanged(nameof(Users));
                    UpdateStatistics();
                    System.Diagnostics.Debug.WriteLine($"✅ ViewModel: Установлено {_users.Count} пользователей");
                }
            }
        }


        private int _totalUsersCount;
        public int TotalUsersCount
        {
            get => _totalUsersCount;
            set { _totalUsersCount = value; OnPropertyChanged(); }
        }

        private int _activeUsersCount;
        public int ActiveUsersCount
        {
            get => _activeUsersCount;
            set { _activeUsersCount = value; OnPropertyChanged(); }
        }

        private int _doctorUsersCount;
        public int DoctorUsersCount
        {
            get => _doctorUsersCount;
            set { _doctorUsersCount = value; OnPropertyChanged(); }
        }

        private string _connectionStatus = "Подключение...";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set { _connectionStatus = value; OnPropertyChanged(); }
        }

        
        private MyCommand _refreshCommand;
        private MyCommand _addTestUserCommand;

        public MyCommand RefreshCommand =>
            _refreshCommand ??= new MyCommand(
                param => RefreshData(),
                param => true
            );

        public MyCommand AddTestUserCommand =>
            _addTestUserCommand ??= new MyCommand(
                param => AddTestUser(),
                param => true
            );

        public MainWindowViewModel()
        {
            System.Diagnostics.Debug.WriteLine("🚀 MainWindowViewModel создан");


            _userRepository = new SimpleUserRepository();
            ConnectionStatus = "Используется SQLite база данных";

            RefreshData();
        }

        private void RefreshData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Начало загрузки данных...");
                var collection = _userRepository.GetAll();
                Users = new ObservableCollection<Пользователь>(collection);
                Users.CollectionChanged += OnListChanged;
                UpdateStatistics();
                System.Diagnostics.Debug.WriteLine($"✅ Данные загружены: {Users.Count} пользователей");
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Ошибка: {ex.Message}";
                Users = new ObservableCollection<Пользователь>();
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка при загрузке: {ex.Message}");
            }
        }

        private void AddTestUser()
        {
            var random = new Random();
            var roles = new[] { "пациент", "врач", "администратор" };
            var newUser = new Пользователь
            {
                id = 0,
                email = $"user{random.Next(1000)}@test.ru",
                пароль = "123456",
                роль = roles[random.Next(roles.Length)],
                имя = $"Имя{random.Next(1000)}",
                фамилия = $"Фамилия{random.Next(1000)}",
                телефон = $"+7{random.Next(900000000, 999999999)}",
                дата_рождения = DateTime.Now.AddYears(-random.Next(18, 70)),
                активен = true
            };

            if (_userRepository.Add(newUser))
            {
                RefreshData();
            }
        }

        private void OnListChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (Пользователь user in e.NewItems)
                {
                    if (!_userRepository.Add(user))
                    {
                        Users.CollectionChanged -= OnListChanged;
                        Users.Remove(user);
                        Users.CollectionChanged += OnListChanged;
                    }
                }
            }
            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            if (Users == null) return;

            TotalUsersCount = Users.Count;
            ActiveUsersCount = Users.Count(u => u.активен);
            DoctorUsersCount = Users.Count(u => u.роль == "врач");

            System.Diagnostics.Debug.WriteLine($"📊 Статистика: Всего={TotalUsersCount}, Активных={ActiveUsersCount}, Врачей={DoctorUsersCount}");
        }
    }
}