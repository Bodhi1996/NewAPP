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
        private ObservableCollection<NomenclatureUnit> _availableProducts;//это коллекция из левой части
        private ObservableCollection<NomenclatureUnit> _selectedProducts; //это коллекция товаров из правой части
        private List<NomenclatureUnit> _allProducts; //это коллекция для всех товаров

        // Временные списки для выделенных элементов
        private List<NomenclatureUnit> _selectedAvailable = new List<NomenclatureUnit>(); //временный список для левой части
        private List<NomenclatureUnit> _selectedSelected = new List<NomenclatureUnit>(); //временный список для правой части

        // Результат выбора
        public List<NomenclatureUnit> SelectedProductsResult { get; private set; } //выбранные элементы

        public event Action<List<NomenclatureUnit>> SelectedProductsChanged; //это событие, через которое будет передаваться данные
        //конструктор, в котором мы загружаем все необходимые элементы для функционирования окна
        public SelectWindow ( List<NomenclatureUnit> allProducts, List<NomenclatureUnit> preselected = null )
        {
            InitializeComponent();
            
            _allProducts = allProducts ?? new List<NomenclatureUnit>(); //создаем список всех продуктов, либо создаем новый список.
            _availableProducts = new ObservableCollection<NomenclatureUnit>(_allProducts); //создаем правый список, исходя из левого списка
            _selectedProducts = new ObservableCollection<NomenclatureUnit>(); //создаем пустой правый список

            // Если есть предвыбранные товары – переносим их в правую колонку
            if (preselected != null) //если переменная выбраных товаров не равна нулю, то через печесисление foreach передаем значения p из preselected  в выбранные продукты, но если выбранный продукт р находится в левой части.
            {
                foreach (var p in preselected)
                {
                    if (_availableProducts.Contains(p)) //проверка, есть ли в левой части продукт р
                    {
                        _availableProducts.Remove(p); //если да, то удаляем его из левой части
                        _selectedProducts.Add(p); //если да, то добавляем его в правую часть
                    }
                }
            }

            AvailableListBox.ItemsSource = _availableProducts; //обновляем окно xaml  левую часть продуктов
            SelectedListBox.ItemsSource = _selectedProducts; //обновляем окно xaml правую часть продуктов

            UpdateUI(); //обновление всех визуальных счетчиков
        }

        // Обновление счётчиков и кнопок
        private void UpdateUI ( )
        {
            AvailableCountText.Text = $"Доступно: {_availableProducts.Count}"; //обновление счетчика доступности
            SelectedCountText.Text = $"({_selectedProducts.Count})";//обновление выбранных товаров
            Title = $"Выбор товаров – выбрано {_selectedProducts.Count}";//обновление названия

            AddSelectedButton.IsEnabled = _selectedAvailable.Any(); //если есть выбранные товары то видимость включается
            AddAllButton.IsEnabled = _availableProducts.Any(); //если есть товары слева то кнопка становится видимой
            RemoveSelectedButton.IsEnabled = _selectedSelected.Any(); //если выбранные товары есть, то кнопка удаления выбранных товаров видимая
            RemoveAllButton.IsEnabled = _selectedProducts.Any(); //если выбранные товары есть, то кнопка удаления всех товаров видна
        }

        // Поиск
        private void SearchTextBox_TextChanged ( object sender, TextChangedEventArgs e ) //поиск товаров
        {
            string text = SearchTextBox.Text?.ToLower() ?? ""; //переменная для хранения запроса поиска, проверяет, равен ли текстбок нулю, если нет, то регистр изменяется, либо пустая строка
            if (string.IsNullOrWhiteSpace(text)) //проверка значения на нуль
            {
                AvailableListBox.ItemsSource = _availableProducts; //обновляем xaml 
            }
            else
            {
                var filtered = _availableProducts.Where(p => p.Name.ToLower().Contains(text)).ToList(); //переменная фильтра, где коллекция товаров с левой стороны, через linq проверяется, что некая р имя, в нижнем регистре, содержит поисковый текст, и переводится в коллекцию. 
                AvailableListBox.ItemsSource = filtered; //обновляем коллекцию в xaml странице
            }
        }

        // Обработчики выделения
        private void AvailableListBox_SelectionChanged ( object sender, SelectionChangedEventArgs e )//это событие выделения
        {
            _selectedAvailable = AvailableListBox.SelectedItems.Cast<NomenclatureUnit>().ToList(); //нажимаем на выбранный элемент и он перевидится в левую сторону
            UpdateUI();
        }

        private void SelectedListBox_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            _selectedSelected = SelectedListBox.SelectedItems.Cast<NomenclatureUnit>().ToList(); //переводится в правую сторону
            UpdateUI();
        }

        // Добавление выбранных
        private void AddSelectedButton_Click ( object sender, RoutedEventArgs e )
        {
            var toAdd = _selectedAvailable.ToList(); //создаем переменную которая хранит список правого элемента
            foreach (var p in toAdd) //перечисление foreach из списка 
            {
                _availableProducts.Remove(p); //удаляем из левого списка элемент p
                if (!_selectedProducts.Contains(p)) //если правая сторона не хранит выбранный элемент то добавляем его
                    _selectedProducts.Add(p);
            }
            _selectedAvailable.Clear(); //очищаем левый список
            UpdateUI();
        }

        // Добавить все
        private void AddAllButton_Click ( object sender, RoutedEventArgs e )
        {
            var all = _availableProducts.ToList(); //так же добавляем в новую переменную элементы
            foreach (var p in all) //перечисление из списка левого
            {
                _selectedProducts.Add(p); //все элементы добавляем в правую сторону
            }
            _availableProducts.Clear(); //очистка элементов
            UpdateUI();
        }

        // Удалить выбранные
        private void RemoveSelectedButton_Click ( object sender, RoutedEventArgs e )
        {
            var toRemove = _selectedSelected.ToList(); //добавляем список в новую переменную
            foreach (var p in toRemove) //перечисление
            {
                _selectedProducts.Remove(p); //удаляем из правого списка
                _availableProducts.Add(p); //добавляем в левый список
            }
            _selectedSelected.Clear(); //очищаем правый список
            SortAvailable();
            UpdateUI();
        }

        // Удалить все
        private void RemoveAllButton_Click ( object sender, RoutedEventArgs e )
        {
            var all = _selectedProducts.ToList(); //выбираем все товары из коллекции
            foreach (var p in all) //перечисление
            {
                _selectedProducts.Remove(p); //удаляем из правого списка
                _availableProducts.Add(p); //добавляем в левый список
            } 
            SortAvailable(); //сортировка
            UpdateUI(); //обновление
        }

        // Сортировка левого списка по имени
        private void SortAvailable ( )
        {
            var sorted = _availableProducts.OrderBy(p => p.Name).ToList(); //вводим новую переменную, которая группируем список по имени и переводит в список
            _availableProducts.Clear();
            foreach (var p in sorted) _availableProducts.Add(p);
        }

        // Подтверждение
        private void OkButton_Click ( object sender, RoutedEventArgs e )
        {
            
            SelectedProductsResult = _selectedProducts.ToList();

            SelectedProductsChanged.Invoke(SelectedProductsResult);

            //DialogResult = true;
            Close();
        }

        // Отмена
        private void CancelButton_Click ( object sender, RoutedEventArgs e )
        {
            //DialogResult = false;
            Close();
        }
    }
}