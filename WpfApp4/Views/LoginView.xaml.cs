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
    }
}