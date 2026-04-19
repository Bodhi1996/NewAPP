using NewAPP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NewAPP.View
{
    /// <summary>
    /// Логика взаимодействия для SelectWindow.xaml
    /// </summary>
    public partial class SelectWindow : Window
    {
        private ObservableCollection<NomenclatureUnit> _availableProducts;
        private ObservableCollection<NomenclatureUnit> _selectedProducts;
        private List<NomenclatureUnit> _allProducts;

        // Временные списки для выделенных элементов
        private List<NomenclatureUnit> _selectedAvailable = new List<NomenclatureUnit>();
        private List<NomenclatureUnit> _selectedSelected = new List<NomenclatureUnit>();

        // Результат выбора
        public List<NomenclatureUnit> SelectedProductsResult { get; private set; }

        public SelectWindow ( List<NomenclatureUnit> allProducts, List<NomenclatureUnit> preselected = null )
        {
            InitializeComponent();

            _allProducts = allProducts ?? new List<NomenclatureUnit>();
            _availableProducts = new ObservableCollection<NomenclatureUnit>(_allProducts);
            _selectedProducts = new ObservableCollection<NomenclatureUnit>();

            // Если есть предвыбранные товары – переносим их в правую колонку
            if (preselected != null)
            {
                foreach (var p in preselected)
                {
                    if (_availableProducts.Contains(p))
                    {
                        _availableProducts.Remove(p);
                        _selectedProducts.Add(p);
                    }
                }
            }

            AvailableListBox.ItemsSource = _availableProducts;
            SelectedListBox.ItemsSource = _selectedProducts;

            UpdateUI();
        }

        // Обновление счётчиков и кнопок
        private void UpdateUI ( )
        {
            AvailableCountText.Text = $"Доступно: {_availableProducts.Count}";
            SelectedCountText.Text = $"({_selectedProducts.Count})";
            Title = $"Выбор товаров – выбрано {_selectedProducts.Count}";

            AddSelectedButton.IsEnabled = _selectedAvailable.Any();
            AddAllButton.IsEnabled = _availableProducts.Any();
            RemoveSelectedButton.IsEnabled = _selectedSelected.Any();
            RemoveAllButton.IsEnabled = _selectedProducts.Any();
        }

        // Поиск
        private void SearchTextBox_TextChanged ( object sender, TextChangedEventArgs e )
        {
            string text = SearchTextBox.Text?.ToLower() ?? "";
            if (string.IsNullOrWhiteSpace(text))
            {
                AvailableListBox.ItemsSource = _availableProducts;
            }
            else
            {
                var filtered = _availableProducts.Where(p => p.Name.ToLower().Contains(text)).ToList();
                AvailableListBox.ItemsSource = filtered;
            }
        }

        // Обработчики выделения
        private void AvailableListBox_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            _selectedAvailable = AvailableListBox.SelectedItems.Cast<NomenclatureUnit>().ToList();
            UpdateUI();
        }

        private void SelectedListBox_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            _selectedSelected = SelectedListBox.SelectedItems.Cast<NomenclatureUnit>().ToList();
            UpdateUI();
        }

        // Добавление выбранных
        private void AddSelectedButton_Click ( object sender, RoutedEventArgs e )
        {
            var toAdd = _selectedAvailable.ToList();
            foreach (var p in toAdd)
            {
                _availableProducts.Remove(p);
                if (!_selectedProducts.Contains(p))
                    _selectedProducts.Add(p);
            }
            _selectedAvailable.Clear();
            UpdateUI();
        }

        // Добавить все
        private void AddAllButton_Click ( object sender, RoutedEventArgs e )
        {
            var all = _availableProducts.ToList();
            foreach (var p in all)
            {
                _selectedProducts.Add(p);
            }
            _availableProducts.Clear();
            UpdateUI();
        }

        // Удалить выбранные
        private void RemoveSelectedButton_Click ( object sender, RoutedEventArgs e )
        {
            var toRemove = _selectedSelected.ToList();
            foreach (var p in toRemove)
            {
                _selectedProducts.Remove(p);
                _availableProducts.Add(p);
            }
            _selectedSelected.Clear();
            SortAvailable();
            UpdateUI();
        }

        // Удалить все
        private void RemoveAllButton_Click ( object sender, RoutedEventArgs e )
        {
            var all = _selectedProducts.ToList();
            foreach (var p in all)
            {
                _selectedProducts.Remove(p);
                _availableProducts.Add(p);
            }
            SortAvailable();
            UpdateUI();
        }

        // Сортировка левого списка по имени
        private void SortAvailable ( )
        {
            var sorted = _availableProducts.OrderBy(p => p.Name).ToList();
            _availableProducts.Clear();
            foreach (var p in sorted) _availableProducts.Add(p);
        }

        // Подтверждение
        private void OkButton_Click ( object sender, RoutedEventArgs e )
        {
            SelectedProductsResult = _selectedProducts.ToList();
            DialogResult = true;
            Close();
        }

        // Отмена
        private void CancelButton_Click ( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
            Close();
        }
    }
}