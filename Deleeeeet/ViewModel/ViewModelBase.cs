using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Deleeeeet.ViewModel
{
    internal class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName]string propertyName = "") => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal class DelegateCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        private readonly Predicate<object?>? _canExecutePredicate;
        private readonly Action<object?>? _executeAction;

        public bool CanExecute(object? parameter)
        {
            if (_canExecutePredicate == null)
            {
                return true;
            }
            return _canExecutePredicate(parameter);
        }

        public void Execute(object? parameter)
        {
            _executeAction?.Invoke(parameter);
        }

        public DelegateCommand(Action<object?> action)
        {
            this._executeAction  = action;
            this._canExecutePredicate= null;
        }

        public DelegateCommand(Action<object?> action, Predicate<object?> predicate)
        {
            this._executeAction = action;
            this._canExecutePredicate = predicate;
        }
    }
}
