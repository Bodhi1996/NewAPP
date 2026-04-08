using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using NewAPP.Services;
using NewAPP.Models;

namespace NewAPP.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _db;
        private string _login;
        private string _password;
        private string _errorMessage;
        private bool _isAuthenticating;
        private User _currentUser;

        public LoginViewModel ( )
        {
            _db = new DatabaseService();
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
        }

        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool IsAuthenticating
        {
            get => _isAuthenticating;
            set
            {
                _isAuthenticating = value;
                OnPropertyChanged();
            }
        }

        public User CurrentUser => _currentUser;

        public ICommand LoginCommand { get; }

        private bool CanExecuteLogin ( object param )
        {
            return !string.IsNullOrWhiteSpace(Login) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsAuthenticating;
        }

        private void ExecuteLogin ( object param )
        {
            IsAuthenticating = true;
            ErrorMessage = "";

            try
            {
                var user = _db.Authenticate(Login, Password);

                if (user != null)
                {
                    _currentUser = user;

                    // Пытаемся получить окно разными способами
                    Window window = null;

                    if (param is Window w)
                        window = w;
                    else if (Application.Current.Windows.Count > 0)
                        window = Application.Current.Windows[0]; // Берем первое окно

                    if (window != null)
                    {
                        window.DialogResult = true;
                        window.Close();
                    }
                }
                else
                {
                    ErrorMessage = "Неверный логин или пароль!";
                    Password = "";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
            }
            finally
            {
                IsAuthenticating = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged ( [CallerMemberName] string name = null )
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}