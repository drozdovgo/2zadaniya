using System;
using System.IO;
using System.Windows;
using WpfApp4.Domain;

namespace WpfApp4
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Проверка пути к базе данных
            string dbPath = "medicalclinic.db";
            System.Diagnostics.Debug.WriteLine($"Путь к базе данных: {Path.GetFullPath(dbPath)}");

            // Проверка создания базы
            try
            {
                using var context = new MyDatabaseContext("Data Source=medicalclinic.db");
                bool canConnect = context.Database.CanConnect();
                System.Diagnostics.Debug.WriteLine($"Подключение к БД: {canConnect}");

                if (canConnect)
                {
                    System.Diagnostics.Debug.WriteLine($"Специализаций: {context.Специализация.Count()}");
                    System.Diagnostics.Debug.WriteLine($"Пользователей: {context.Пользователь.Count()}");
                    System.Diagnostics.Debug.WriteLine($"Врачей: {context.Врач.Count()}");
                    System.Diagnostics.Debug.WriteLine($"Мед. карт: {context.Медицинская_карта.Count()}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка БД: {ex.Message}");
            }
        }
    }
}