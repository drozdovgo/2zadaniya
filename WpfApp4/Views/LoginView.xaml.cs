using System.Windows;
using System.Windows.Controls;

namespace WpfApp4.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.LoginViewModel viewModel)
            {
                viewModel.LoginModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void CheckDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("=== РУЧНАЯ ПРОВЕРКА БАЗЫ ДАННЫХ ===");
            WpfApp4.Utils.DatabaseHelper.PrintDatabaseInfo();

            MessageBox.Show(
                $"Информация о базе данных:\n\n" +
                $"Файл: medicalclinic.db\n" +
                $"Папка: {WpfApp4.Utils.DatabaseHelper.GetDatabaseFolderPath()}\n" +
                $"Существует: {WpfApp4.Utils.DatabaseHelper.DatabaseExists()}\n" +
                $"Размер: {WpfApp4.Utils.DatabaseHelper.GetDatabaseSize()} байт\n\n" +
                $"Подробности смотрите в Output окне.",
                "Информация о базе данных",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }
}