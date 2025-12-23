using System.Windows;

namespace WpfApp4
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                var connectionString = Utils.ConnectionManager.GetConnectionString();
                using var context = new Domain.MyDatabaseContext(connectionString);
                context.Database.EnsureCreated();
            }
            catch
            {
                // Игнорируем ошибки - база создастся при попытке работы
            }
        }
    }
}