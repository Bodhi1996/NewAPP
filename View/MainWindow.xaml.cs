using NewAPP.Models;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NewAPP.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       private MainViewModel _viewModel;


        public MainWindow ( )
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;
            this.Width = 1400;
            this.Height = 800;

            // Подписка на событие Closed (не OnClosing!)
            this.Closed += ( s, e ) => _viewModel.Cleanup();
        }

        // Только специфичные для окна обработчики
        private void Window_PreviewKeyDown ( object sender, KeyEventArgs e )
        {
            if (e.Key == Key.F5 && _viewModel != null)
            {
                // Можно добавить команду обновления
                e.Handled = true;
            }
        }
    }
}
