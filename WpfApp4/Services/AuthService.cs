using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
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
            _connectionString = DatabaseHelper.GetConnectionString();
            Debug.WriteLine($"AuthService: Используем строку подключения: {_connectionString}");
        }

        public (bool success, string message, Пользователь? user) Login(string email, string password)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Попытка входа: email={email}");

                using var context = new MyDatabaseContext(_connectionString);

                // Проверяем подключение к базе
                if (!context.Database.CanConnect())
                {
                    return (false, "Не удалось подключиться к базе данных", null);
                }

                // Ищем пользователя
                var user = context.Пользователь
                    .FirstOrDefault(u => u.email == email && u.пароль == password && u.активен);

                if (user == null)
                {
                    // Проверяем отдельно каждую возможную причину
                    var userExists = context.Пользователь.Any(u => u.email == email);
                    if (!userExists)
                    {
                        return (false, "Пользователь с таким email не найден", null);
                    }

                    var correctPassword = context.Пользователь.Any(u => u.email == email && u.пароль == password);
                    if (!correctPassword)
                    {
                        return (false, "Неверный пароль", null);
                    }

                    var isActive = context.Пользователь.Any(u => u.email == email && u.активен);
                    if (!isActive)
                    {
                        return (false, "Пользователь неактивен", null);
                    }

                    return (false, "Неверный email или пароль", null);
                }

                System.Diagnostics.Debug.WriteLine($"✅ Успешный вход: {user.email}, роль: {user.роль}");
                return (true, "Успешный вход", user);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка при входе: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return (false, "Ошибка при подключении к базе данных", null);
            }
        }

        public (bool success, string message) Register(RegisterModel model)
        {
            try
            {
                Debug.WriteLine($"Попытка регистрации: email={model.Email}, имя={model.FirstName}, роль={model.SelectedRole}");

                if (model.Password != model.ConfirmPassword)
                {
                    Debug.WriteLine("Ошибка: пароли не совпадают");
                    return (false, "Пароли не совпадают");
                }

                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    Debug.WriteLine("Ошибка: email обязателен");
                    return (false, "Email обязателен для заполнения");
                }

                using var context = new MyDatabaseContext(_connectionString);

                if (context.Пользователь.Any(u => u.email == model.Email))
                {
                    Debug.WriteLine($"Ошибка: пользователь с email {model.Email} уже существует");
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
                    активен = true,
                    дата_регистрации = DateTime.Now
                };

                Debug.WriteLine($"Создаем пользователя: {newUser.ПолноеИмя}");

                // Используем простой подход без транзакций
                context.Пользователь.Add(newUser);

                try
                {
                    var result = context.SaveChanges();
                    Debug.WriteLine($"Сохранено изменений: {result}");

                    if (result > 0)
                    {
                        Debug.WriteLine($"✅ Регистрация успешна для пользователя {model.Email}");

                        // Если пользователь - пациент, создаем для него медицинскую карту
                        if (model.SelectedRole == "пациент")
                        {
                            try
                            {
                                var medicalCard = new Медицинская_карта
                                {
                                    пациент_id = newUser.id,
                                    группа_крови = "Не указана",
                                    аллергии = "Не указаны",
                                    хронические_заболевания = "Не указаны",
                                    дата_создания = DateTime.Now,
                                    дата_обновления = DateTime.Now
                                };

                                context.Медицинская_карта.Add(medicalCard);
                                context.SaveChanges();
                                Debug.WriteLine($"✅ Создана медицинская карта для пациента {newUser.id}");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"⚠️ Не удалось создать медицинскую карту: {ex.Message}");
                                // Не прерываем регистрацию из-за этой ошибки
                            }
                        }

                        return (true, "Регистрация прошла успешно");
                    }
                    else
                    {
                        Debug.WriteLine("⚠️ Не удалось сохранить пользователя");
                        return (false, "Не удалось сохранить пользователя");
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    Debug.WriteLine($"❌ Ошибка базы данных при регистрации: {dbEx.Message}");

                    if (dbEx.InnerException != null)
                    {
                        Debug.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                    }

                    return (false, $"Ошибка базы данных: {dbEx.InnerException?.Message ?? dbEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Общая ошибка при регистрации: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return (false, $"Ошибка при регистрации: {ex.Message}");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidatePassword(string password)
        {
            bool isValid = !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
            Debug.WriteLine($"Валидация пароля: {isValid} (длина: {password?.Length})");
            return isValid;
        }
    }
}