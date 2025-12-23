using Microsoft.EntityFrameworkCore;
using System.Linq;
using WpfApp4.Domain;
using WpfApp4.Models.Auth;
using WpfApp4.Models.Entities;
using WpfApp4.Utils;

namespace WpfApp4.Services
{
    public class AuthService
    {
        private readonly string _connectionString;

        public AuthService()
        {
            _connectionString = ConnectionManager.GetConnectionString();
        }

        public (bool success, string message, Пользователь? user) Login(string email, string password)
        {
            using var context = new MyDatabaseContext(_connectionString);
            var user = context.Пользователь
                .FirstOrDefault(u => u.email == email && u.пароль == password && u.активен);

            return user == null
                ? (false, "Неверный email или пароль", null)
                : (true, "Успешный вход", user);
        }

        public (bool success, string message) Register(RegisterModel model)
        {
            if (model.Password != model.ConfirmPassword)
                return (false, "Пароли не совпадают");

            if (string.IsNullOrWhiteSpace(model.Email))
                return (false, "Email обязателен для заполнения");

            using var context = new MyDatabaseContext(_connectionString);

            if (context.Пользователь.Any(u => u.email == model.Email))
                return (false, "Пользователь с таким email уже существует");

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
            var result = context.SaveChanges() > 0;

            // Если пользователь - пациент, создаем медицинскую карту
            if (result && model.SelectedRole == "пациент")
            {
                try
                {
                    context.Медицинская_карта.Add(new Медицинская_карта
                    {
                        пациент_id = newUser.id,
                        группа_крови = "Не указана",
                        аллергии = "Не указаны",
                        хронические_заболевания = "Не указаны"
                    });
                    context.SaveChanges();
                }
                catch
                {
                    // Игнорируем ошибку создания медкарты
                }
            }

            return result
                ? (true, "Регистрация прошла успешно")
                : (false, "Не удалось сохранить пользователя");
        }

        // Метод проверки пароля (добавляем, если он используется)
        public bool ValidatePassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
        }
    }
}