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
                    var context = GetContext();
                    var canConnect = context.Database.CanConnect();
                    System.Diagnostics.Debug.WriteLine($"🔌 Проверка подключения: {canConnect}");
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