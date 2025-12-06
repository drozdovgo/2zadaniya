using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfApp4.Models.Auth
{
    public class RegisterModel : INotifyPropertyChanged
    {
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _phone = string.Empty;
        private DateTime _birthDate = DateTime.Now.AddYears(-18);
        private string _selectedRole = "пациент";

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
                OnPropertyChanged(nameof(CanRegister)); // Добавлено
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
                OnPropertyChanged(nameof(CanRegister)); // Добавлено
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
                OnPropertyChanged(nameof(CanRegister)); // Добавлено
            }
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
                OnPropertyChanged(nameof(CanRegister)); // Добавлено
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
                OnPropertyChanged(nameof(CanRegister)); // Добавлено

            }
        }

        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(nameof(Phone)); OnPropertyChanged(nameof(CanRegister)); }
        }

        public DateTime BirthDate
        {
            get => _birthDate;
            set { _birthDate = value; OnPropertyChanged(nameof(BirthDate)); OnPropertyChanged(nameof(CanRegister)); }
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set { _selectedRole = value; OnPropertyChanged(nameof(SelectedRole)); OnPropertyChanged(nameof(CanRegister)); }
        }

        public string[] AvailableRoles => new[] { "пациент", "врач", "администратор" };

        // Свойство для привязки в XAML (опционально)
        public bool CanRegister =>
            !string.IsNullOrWhiteSpace(Email) &&
            !string.IsNullOrWhiteSpace(Password) &&
            !string.IsNullOrWhiteSpace(ConfirmPassword) &&
            !string.IsNullOrWhiteSpace(FirstName) &&
            !string.IsNullOrWhiteSpace(LastName) &&
            Password.Length >= 6 &&
            Password == ConfirmPassword;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // Обновляем состояние команд при любом изменении свойства
            CommandManager.InvalidateRequerySuggested();
        }
    }
}