using System;
using System.Windows;
using System.Windows.Input;
using WpfApp4.Models;
using WpfApp4.Models.Auth;
using WpfApp4.Models.Entities;
using WpfApp4.Services;
using WpfApp4.Utils;

namespace WpfApp4.ViewModels
{
    public class RegisterViewModel : PropertyChangedBase
    {
        private readonly AuthService _authService;
        private RegisterModel _registerModel;
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private bool _isLoading;

        public RegisterModel RegisterModel
        {
            get => _registerModel;
            set { _registerModel = value; OnPropertyChanged(); }
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

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // Команды
        public MyCommand RegisterCommand { get; }
        public MyCommand BackToLoginCommand { get; }

        // События для навигации
        public event Action? ShowLoginRequested;
        public event Action<Пользователь>? RegistrationSuccessful;

        public RegisterViewModel()
        {
            _authService = new AuthService();
            RegisterModel = new RegisterModel();

            // Временно: команда всегда доступна для тестирования
            RegisterCommand = new MyCommand(
                _ => Register(),
                _ => true  // Всегда true для тестирования
            );

            BackToLoginCommand = new MyCommand(
                _ => ShowLoginRequested?.Invoke()
            );

            // Подпишемся на изменения свойств для обновления команд
            RegisterModel.PropertyChanged += (s, e) =>
            {
                // Принудительно обновляем состояние команды при изменении свойств
                CommandManager.InvalidateRequerySuggested();
            };
        }

        private bool CanRegister()
        {
            // Добавим отладку
            return !string.IsNullOrWhiteSpace(RegisterModel.Email) &&
               !string.IsNullOrWhiteSpace(RegisterModel.Password) &&
               !string.IsNullOrWhiteSpace(RegisterModel.ConfirmPassword) &&
               !string.IsNullOrWhiteSpace(RegisterModel.FirstName) &&
               !string.IsNullOrWhiteSpace(RegisterModel.LastName) &&
               !string.IsNullOrWhiteSpace(RegisterModel.Phone) &&
               RegisterModel.Password.Length >= 6 &&
               RegisterModel.Password == RegisterModel.ConfirmPassword &&
               !IsLoading;

        }


        private void Register()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                SuccessMessage = string.Empty;
                System.Diagnostics.Debug.WriteLine($"=== Попытка регистрации ===");

                // Валидация
                if (RegisterModel.Password != RegisterModel.ConfirmPassword)
                {
                    ErrorMessage = "Пароли не совпадают";
                    System.Diagnostics.Debug.WriteLine("❌ Пароли не совпадают");
                    return;
                }

                if (!_authService.ValidatePassword(RegisterModel.Password))
                {
                    ErrorMessage = "Пароль должен содержать минимум 6 символов";
                    System.Diagnostics.Debug.WriteLine("❌ Пароль слишком короткий");
                    return;
                }

                var result = _authService.Register(RegisterModel);
                System.Diagnostics.Debug.WriteLine($"Результат регистрации: success={result.success}, message={result.message}");

                if (result.success)
                {
                    SuccessMessage = result.message;
                    System.Diagnostics.Debug.WriteLine("✅ Регистрация успешна");

                    // Автоматический вход после регистрации
                    var loginResult = _authService.Login(RegisterModel.Email, RegisterModel.Password);
                    if (loginResult.success && loginResult.user != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ Автоматический вход после регистрации: {loginResult.user.ПолноеИмя}");
                        RegistrationSuccessful?.Invoke(loginResult.user);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ Автоматический вход не удался: {loginResult.message}");
                        ShowLoginRequested?.Invoke();
                    }
                }
                else
                {
                    ErrorMessage = result.message;
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка регистрации: {result.message}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Критическая ошибка: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ Критическая ошибка регистрации: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}