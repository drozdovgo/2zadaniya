using System.Windows;

namespace WpfApp4
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Удаляем старую базу и создаем новую
            try
            {
                var dbFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "medicalclinic.db");
                if (System.IO.File.Exists(dbFile))
                {
                    System.IO.File.Delete(dbFile);
                    System.Diagnostics.Debug.WriteLine("✅ Старая база данных удалена");
                }

                var connectionString = Utils.ConnectionManager.GetConnectionString();
                using var context = new Domain.MyDatabaseContext(connectionString);
                context.Database.EnsureCreated();
                System.Diagnostics.Debug.WriteLine("✅ Новая база данных создана");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Ошибка создания БД: {ex.Message}");
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                var connectionString = Utils.ConnectionManager.GetConnectionString();
                System.Diagnostics.Debug.WriteLine($"Подключение к базе: {connectionString}");

                using var context = new Domain.MyDatabaseContext(connectionString);
                context.Database.EnsureCreated();

                // Проверяем таблицы
                var patientCount = context.Пользователь.Count();
                var doctorCount = context.Врач.Count();
                System.Diagnostics.Debug.WriteLine($"Пациентов: {patientCount}, Врачей: {doctorCount}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации БД: {ex.Message}");
                MessageBox.Show($"Ошибка базы данных: {ex.Message}");
            }
        }

    }
}