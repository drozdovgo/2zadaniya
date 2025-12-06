using System;
using System.Windows;
using WpfApp4.Models;
using WpfApp4.Models.Auth;
using WpfApp4.Models.Entities;
using WpfApp4.Services;
using WpfApp4.Utils;

namespace WpfApp4.ViewModels
{
    public class LoginViewModel : PropertyChangedBase
    {
        private readonly AuthService _authService;
        private LoginModel _loginModel;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public LoginModel LoginModel
        {
            get => _loginModel;
            set { _loginModel = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // Команды
        public MyCommand LoginCommand { get; }
        public MyCommand RegisterCommand { get; }
        public MyCommand CloseCommand { get; }

        // События для навигации
        public event Action<Пользователь>? LoginSuccessful;
        public event Action? ShowRegisterRequested;
        public event Action? CloseApplicationRequested;

        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginModel = new LoginModel();

            LoginCommand = new MyCommand(
                _ => Login(),
                _ => CanLogin()
            );

            RegisterCommand = new MyCommand(
                _ => ShowRegisterRequested?.Invoke()
            );

            CloseCommand = new MyCommand(
                _ => CloseApplicationRequested?.Invoke()
            );
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(LoginModel.Email) &&
                   !string.IsNullOrWhiteSpace(LoginModel.Password) &&
                   !IsLoading;
        }

        private void Login()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var result = _authService.Login(LoginModel.Email, LoginModel.Password);

                if (result.success)
                {
                    LoginSuccessful?.Invoke(result.user);
                }
                else
                {
                    ErrorMessage = result.message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}