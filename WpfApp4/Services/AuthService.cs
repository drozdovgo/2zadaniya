using System;
using System.Linq;
using WpfApp4.Domain;
using WpfApp4.Models.Auth;
using WpfApp4.Models.Entities;

namespace WpfApp4.Services
{
    public class AuthService
    {
        private readonly string _connectionString = "Data Source=medicalclinic.db";

        public (bool success, string message, Пользователь? user) Login(string email, string password)
        {
            try
            {
                using var context = new MyDatabaseContext(_connectionString);
                var user = context.Пользователь
                    .FirstOrDefault(u => u.email == email && u.пароль == password && u.активен);

                if (user == null)
                {
                    return (false, "Неверный email или пароль", null);
                }

                return (true, "Успешный вход", user);
            }
            catch (System.Exception ex)
            {
                return (false, $"Ошибка при входе: {ex.Message}", null);
            }
        }

        public (bool success, string message) Register(RegisterModel model)
        {
            try
            {
                if (model.Password != model.ConfirmPassword)
                {
                    return (false, "Пароли не совпадают");
                }

                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    return (false, "Email обязателен для заполнения");
                }

                using var context = new MyDatabaseContext(_connectionString);

                if (context.Пользователь.Any(u => u.email == model.Email))
                {
                    return (false, "Пользователь с таким email уже существует");
                }

                var newUser = new Пользователь
                {
                    email = model.Email,
                    пароль = model.Password,
                    роль = model.SelectedRole,
                    имя = model.FirstName,
                    фамилия = model.LastName,
                    телефон = model.Phone,
                    дата_рождения = model.BirthDate,
                    активен = true
                };

                context.Пользователь.Add(newUser);
                context.SaveChanges();

                return (true, "Регистрация прошла успешно");
            }
            catch (System.Exception ex)
            {
                return (false, $"Ошибка при регистрации: {ex.Message}");
            }
        }

        public bool ValidatePassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
        }
    }
}