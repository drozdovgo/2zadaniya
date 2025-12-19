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
                System.Diagnostics.Debug.WriteLine($"=== Попытка входа ===");

                var result = _authService.Login(LoginModel.Email, LoginModel.Password);
                System.Diagnostics.Debug.WriteLine($"Результат входа: success={result.success}, message={result.message}");

                if (result.success)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Вход успешен для пользователя: {result.user?.ПолноеИмя}");
                    LoginSuccessful?.Invoke(result.user);
                }
                else
                {
                    ErrorMessage = result.message;
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка входа: {result.message}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Критическая ошибка: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ Критическая ошибка входа: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}