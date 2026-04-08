
using System.Windows;
using System.Windows.Controls;
using NewAPP.ViewModels;

namespace NewAPP.Views
{
    public partial class LoginWindow : Window
    {
        private LoginViewModel _viewModel;
        public LoginWindow ( )
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;
        }

        private void PasswordBox_PasswordChanged ( object sender, RoutedEventArgs e )
        {
            if (_viewModel != null)
            {
                _viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void LoginButton_Click ( object sender, RoutedEventArgs e )
        {
            // Временное решение - показываем, что команда работает
            if (_viewModel.LoginCommand.CanExecute(this))
            {
                _viewModel.LoginCommand.Execute(this);
            }
        }
    }
}