using System;
using WpfApp4.Domain;

namespace WpfApp4.Utils
{
    public static class DatabaseManager
    {
        private static readonly object _lock = new object();
        private static MyDatabaseContext _dbContext;

        public static MyDatabaseContext GetContext()
        {
            lock (_lock)
            {
                if (_dbContext == null)
                {
                    _dbContext = new MyDatabaseContext("Data Source=medicalclinic.db");
                    System.Diagnostics.Debug.WriteLine("✅ Создан единый контекст базы данных");
                }
                return _dbContext;
            }
        }

        public static void CloseConnection()
        {
            lock (_lock)
            {
                _dbContext?.Dispose();
                _dbContext = null;
                System.Diagnostics.Debug.WriteLine("🔌 Контекст базы данных закрыт");
            }
        }

        public static bool TestConnection()
        {
            lock (_lock)
            {
                try
                {
                    // Используем новый контекст для теста, не основной
                    using var context = new MyDatabaseContext("Data Source=medicalclinic.db");
                    var canConnect = context.Database.CanConnect();
                    System.Diagnostics.Debug.WriteLine($"🔌 Проверка подключения: {canConnect}");

                    if (canConnect)
                    {
                        // Дополнительная проверка
                        var appointmentsCount = context.Запись.Count();
                        System.Diagnostics.Debug.WriteLine($"✅ База доступна, записей: {appointmentsCount}");
                    }

                    return canConnect;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ошибка подключения: {ex.Message}");
                    return false;
                }
            }
        }
    }
}