using NewAPP.Models;
using NewAPP.Services;
using System.ComponentModel;
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
        private DatabaseService _dataBase;


        public MainWindow ( )
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            _dataBase = new DatabaseService();
            this.DataContext = _viewModel;
            this.Width = 1400;
            this.Height = 800;

            // Подписка на событие Closed (не OnClosing!)
            this.Closed += ( s, e ) => _viewModel.Cleanup();
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged ( object sender, PropertyChangedEventArgs e )
        {
            // При изменении любого свойства видимости обновляем колонки
            if (e.PropertyName == nameof(MainViewModel.ShowUnit) ||
                e.PropertyName == nameof(MainViewModel.ShowId) ||
                e.PropertyName == nameof(MainViewModel.ShowName) ||
                e.PropertyName == nameof(MainViewModel.ShowInNumber) ||
                e.PropertyName == nameof(MainViewModel.ShowOutNumber) ||
                e.PropertyName == nameof(MainViewModel.ShowCharacteristick) ||
                e.PropertyName == nameof(MainViewModel.ShowAdr) ||
                e.PropertyName == nameof(MainViewModel.ShowQuantity) ||
                e.PropertyName == nameof(MainViewModel.ShowUnitPrice))
            {
                UpdateColumnsVisibility();
            }
        }
        private void UpdateColumnsVisibility ( )
        {
            foreach (var column in NumGride.Columns)
            {
                switch (column.Header.ToString())
                {
                    case "Номер":
                        column.Visibility = _viewModel.ShowId;
                        break;
                    case "Наименование":
                        column.Visibility = _viewModel.ShowName;
                        break;
                    case "Вн.номер":
                        column.Visibility = _viewModel.ShowInNumber;
                        break;
                    case "Внут.номер":
                        column.Visibility = _viewModel.ShowOutNumber;
                        break;
                    case "Характеристика":
                        column.Visibility = _viewModel.ShowCharacteristick;
                        break;
                    case "Ед.изм.":
                        column.Visibility = _viewModel.ShowUnit;
                        break;
                    case "Адрес":
                        column.Visibility = _viewModel.ShowAdr;
                        break;
                    case "Остаток":
                        column.Visibility = _viewModel.ShowQuantity;
                        break;
                    case "Цена":  // ← ИСПРАВЛЕНО: теперь "Цена", а не "Цена за единицу"
                        column.Visibility = _viewModel.ShowUnitPrice;
                        break;
                }
            }
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
        private void DataGrid_MouseDoubleClick ( object sender, MouseButtonEventArgs e )
        {
            var grid = sender as DataGrid;
            var selectedProduct = grid.SelectedItem as NomenclatureUnit;
            if (selectedProduct != null) {
                {
                    var History = _dataBase.GetOperationType(selectedProduct.Id);
                    var historyWindow = new HistoryWindow();
                    historyWindow.SetData(selectedProduct.Name, History);
                    historyWindow.Show();
                }
            }
        }
    }
}
