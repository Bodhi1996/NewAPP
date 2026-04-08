using System.Windows;
using NewAPP.Views;
using NewAPP.ViewModels;
using NewAPP.View;

namespace NewAPP
{
    public partial class App : Application
    {
        protected override void OnStartup ( StartupEventArgs e )
        {
            base.OnStartup(e);

            // Отключаем автоматическое закрытие
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Показываем окно авторизации
            var loginWindow = new LoginWindow();

            if (loginWindow.ShowDialog() == true)
            {
                // Получаем данные пользователя
                var loginVm = loginWindow.DataContext as LoginViewModel;

                // Создаем главное окно
                var mainWindow = new MainWindow();

                // Передаем пользователя в MainViewModel
                if (mainWindow.DataContext is MainViewModel mainVm)
                {
                    mainVm.CurrentUser = loginVm?.CurrentUser;
                }

                // Включаем автозакрытие обратно
                Current.ShutdownMode = ShutdownMode.OnLastWindowClose;

                // Показываем главное окно
                mainWindow.Show();
            }
            else
            {
                // Закрываем приложение
                Current.Shutdown();
            }
        }
    }
}