using System;
using WpfApp4.Domain;

namespace WpfApp4.Utils
{
    public static class DatabaseTester
    {
        public static void TestDatabaseCreation()
        {
            try
            {
                string databasePath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "test.db"
                );

                System.Diagnostics.Debug.WriteLine($"=== Тест создания базы ===");
                System.Diagnostics.Debug.WriteLine($"Путь: {databasePath}");

                using var context = new MyDatabaseContext($"Data Source={databasePath}");

                bool canConnect = context.Database.CanConnect();
                System.Diagnostics.Debug.WriteLine($"Подключение возможно: {canConnect}");

                if (canConnect)
                {
                    System.Diagnostics.Debug.WriteLine($"Пользователей: {context.Пользователь.Count()}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка: {ex.Message}");
            }
        }
    }
}