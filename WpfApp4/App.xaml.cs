using System;
using System.Windows;
using WpfApp4.Domain;
using WpfApp4.Utils;

namespace WpfApp4
{
    public partial class App : Application
    {
        private static readonly object _dbLock = new object();
        private static MyDatabaseContext _dbContext;

        public static MyDatabaseContext GetDbContext()
        {
            lock (_dbLock)
            {
                if (_dbContext == null)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Создан единый контекст базы данных");
                }
                return _dbContext;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Инициализируем базу при старте
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Инициализация базы данных ===");
                var canConnect = DatabaseManager.TestConnection();
                System.Diagnostics.Debug.WriteLine($"База данных доступна: {canConnect}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка инициализации базы: {ex.Message}");
            }

            SetupExceptionHandling();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            DatabaseManager.CloseConnection();
            base.OnExit(e);
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
                {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
                }

        private void LogUnhandledException(Exception exception, string source)
        {
            string message = $"Необработанное исключение ({source}): {exception.Message}";
            try
            {
                System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                message = string.Format("Необработанное исключение в {0} v{1}\nИсточник: {2}\n{3}\n\n{4}",
                    assemblyName.Name, assemblyName.Version, source, exception.Message, exception.StackTrace);
            }
            catch (Exception ex)
            {
                message = $"Ошибка при создании сообщения об исключении: {ex.Message}\nИсходное исключение: {exception.Message}";
            }

            System.Diagnostics.Debug.WriteLine(message);
            MessageBox.Show(message, "Необработанное исключение", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}