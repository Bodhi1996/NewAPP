using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NewAPP
{
    class RelayCommand: ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
        private readonly EventHandler _canExecuteChangedHandler;

        public RelayCommand ( Action<object> execute, Func<object, bool> canExecute = null )
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
            _canExecuteChangedHandler = ( s, e ) => CommandManager.InvalidateRequerySuggested();
        }

        public bool CanExecute ( object parameter )
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute ( object parameter )
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged ( )
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
