using System;
using System.Windows.Input;

namespace WpfApp4.Utils
{
    public class MyCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public MyCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            bool result = _canExecute == null || _canExecute(parameter);
            System.Diagnostics.Debug.WriteLine($"MyCommand CanExecute вызван: {result}");
            return result;
        }

        public void Execute(object parameter)
        {
            System.Diagnostics.Debug.WriteLine($"MyCommand Execute вызван");
            _execute(parameter);
        }
    }
}