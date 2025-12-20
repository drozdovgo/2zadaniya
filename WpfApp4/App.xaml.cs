using System;
using System.Windows;
using WpfApp4.Domain;
using WpfApp4.Utils;

namespace WpfApp4
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                System.Diagnostics.Debug.WriteLine("=== ЗАПУСК ПРИЛОЖЕНИЯ ===");
                System.Diagnostics.Debug.WriteLine($"Версия: {GetType().Assembly.GetName().Version}");
                System.Diagnostics.Debug.WriteLine($"Дата запуска: {DateTime.Now}");

                // Печатаем информацию о базе данных
                WpfApp4.Utils.DatabaseHelper.PrintDatabaseInfo();

                // Проверяем текущую директорию
                System.Diagnostics.Debug.WriteLine($"Текущая директория: {AppDomain.CurrentDomain.BaseDirectory}");

                // Создаем контекст с использованием DatabaseHelper
                string connectionString = WpfApp4.Utils.DatabaseHelper.GetConnectionString();
                System.Diagnostics.Debug.WriteLine($"Используем строку подключения: {connectionString}");

                using var context = new WpfApp4.Domain.MyDatabaseContext(connectionString);

                // Проверяем подключение
                var canConnect = context.Database.CanConnect();
                System.Diagnostics.Debug.WriteLine($"База данных доступна: {canConnect}");

                if (canConnect)
                {
                    System.Diagnostics.Debug.WriteLine($"=== ДАННЫЕ В БАЗЕ ===");
                    System.Diagnostics.Debug.WriteLine($"• Пользователей: {context.Пользователь.Count()}");
                    System.Diagnostics.Debug.WriteLine($"• Врачей: {context.Врач.Count()}");
                    System.Diagnostics.Debug.WriteLine($"• Пациентов: {context.Пользователь.Count(u => u.роль == "пациент")}");
                    System.Diagnostics.Debug.WriteLine($"• Администраторов: {context.Пользователь.Count(u => u.роль == "администратор")}");
                    System.Diagnostics.Debug.WriteLine($"• Записей: {context.Запись.Count()}");
                    System.Diagnostics.Debug.WriteLine($"• Специализаций: {context.Специализация.Count()}");

                    // Выводим первых 5 пользователей для проверки
                    var sampleUsers = context.Пользователь.Take(5).ToList();
                    System.Diagnostics.Debug.WriteLine($"Пример пользователей ({sampleUsers.Count}):");
                    foreach (var user in sampleUsers)
                    {
                        System.Diagnostics.Debug.WriteLine($"  - {user.email} ({user.роль})");
                    }
                }

                // Открываем папку с базой данных (опционально)
                // WpfApp4.Utils.DatabaseHelper.OpenDatabaseFolder();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Критическая ошибка при запуске: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                MessageBox.Show($"Ошибка при запуске приложения: {ex.Message}\n\nПроверьте права доступа к папке приложения.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}