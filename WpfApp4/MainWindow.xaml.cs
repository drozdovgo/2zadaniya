using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp4.Models;
using WpfApp4.ViewModels;

namespace WpfApp4
{

    public partial class MainWindow : Window
    {
        private MainWindowViewModel _context;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _context = new();
        }
    }
}