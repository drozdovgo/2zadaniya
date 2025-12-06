using System;
using System.Collections.ObjectModel;
using System.Linq;
using WpfApp4.Interfaces;
using WpfApp4.Models;
using WpfApp4.Models.Entities;
using WpfApp4.Utils;

namespace WpfApp4.ViewModels
{
    public class MainContentViewModel : PropertyChangedBase
    {
        private readonly Пользователь _currentUser;
        private readonly IRepository<Пользователь> _userRepository;
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

        // События
        public event Action? LogoutRequested;

        public MainContentViewModel(Пользователь currentUser)
        {
            _currentUser = currentUser;
            _userRepository = new SimpleUserRepository();

            LogoutCommand = new MyCommand(_ => LogoutRequested?.Invoke());
            RefreshCommand = new MyCommand(_ => RefreshData());

            RefreshData();
        }

        private void RefreshData()
        {
            var users = _userRepository.GetAll();
            Users = new ObservableCollection<Пользователь>(users);
        }
    }
}