using NewAPP.Models;
using NewAPP.Services;
using System;
using System.Collections.Generic;
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
using static NewAPP.Services.Excele;

namespace NewAPP.View
{
    /// <summary>
    /// Логика взаимодействия для ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {

        List<NomenclatureUnit> allData; //все данные подкгружаем
        List<NomenclatureUnit> filteredData; //отфильтрованные данные подгружаем
        private Excele Excele; //создаем ексель
        private bool isLoaded = false;
        private List<NomenclatureUnit> _selectedProductsForReport = new List<NomenclatureUnit>();


        public ReportWindow ( List<NomenclatureUnit> data )
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine($"=== ReportWindow Constructor ===");
            System.Diagnostics.Debug.WriteLine($"Получено данных: {data?.Count ?? 0}");

            allData = data;
            Excele = new Excele();

            // Проверка: есть ли данные
            if (allData == null || allData.Count == 0)
            {
                MessageBox.Show("Нет данных для отображения!", "Предупреждение",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Подписываемся на событие загрузки окна
            this.Loaded += ReportWindow_Loaded;
        }

        private void ReportWindow_Loaded ( object sender, RoutedEventArgs e )
        {
            try
            {
                // Заполняем список товаров
                LoadProducts();

                // Устанавливаем даты по умолчанию
                StartDatePicker.SelectedDate = DateTime.Now.AddDays(-30);
                EndDatePicker.SelectedDate = DateTime.Now;

                // Устанавливаем периодичность по умолчанию (Месяц)
                if (PeriodTypeComboBox != null && PeriodTypeComboBox.Items.Count > 0)
                {
                    PeriodTypeComboBox.SelectedIndex = 2; // Месяц (индекс 2, если порядок: День(0), Неделя(1), Месяц(2), Квартал(3))
                }

                // Устанавливаем тип отчета по умолчанию
                if (AllProductsRadio != null)
                {
                    AllProductsRadio.IsChecked = true;
                }

                // Подписываемся на события
                StartDatePicker.SelectedDateChanged += ( s, arg ) => UpdateData();
                EndDatePicker.SelectedDateChanged += ( s, arg ) => UpdateData();
                ProductComboBox.SelectionChanged += ( s, arg ) => UpdateData();
                ManualProductTextBox.TextChanged += ( s, arg ) => UpdateData();
                AllProductsRadio.Checked += ( s, arg ) => UpdateData();
                SingleProductRadio.Checked += ( s, arg ) => UpdateData();
                ManualEntryCheckBox.Checked += ( s, arg ) => UpdateData();
                ManualEntryCheckBox.Unchecked += ( s, arg ) => UpdateData();
                PeriodTypeComboBox.SelectionChanged += ( s, arg ) => UpdateData();

                isLoaded = true;

                // Первоначальная загрузка данных
                UpdateData();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки окна: {ex.Message}");
                PreviewText.Text = $"Ошибка загрузки: {ex.Message}";
            }
        }

        private void LoadProducts ( )
        {
            try
            {
                var productNames = Excele.GetAllProductNames(allData);

                System.Diagnostics.Debug.WriteLine($"Загружено товаров в ComboBox: {productNames.Count}");

                ProductComboBox.ItemsSource = productNames;

                if (productNames.Any())
                {
                    ProductComboBox.SelectedIndex = 0;
                    System.Diagnostics.Debug.WriteLine($"Выбран первый товар: {productNames[0]}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Список товаров пуст!");
                    ProductComboBox.ItemsSource = new List<string> { "Нет данных" };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки товаров: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка");
            }
        }

        private void UpdateData ( )
        {
            if (!isLoaded) return;

            try
            {
                var startDate = StartDatePicker.SelectedDate ?? DateTime.Now;
                var endDate = EndDatePicker.SelectedDate ?? DateTime.Now;
                var allProducts = AllProductsRadio.IsChecked == true;

                // Фильтр по дате (всегда)
                var filtered = allData.Where(x =>
                    x.OperationDate.HasValue &&
                    x.OperationDate.Value.Date >= startDate.Date &&
                    x.OperationDate.Value.Date <= endDate.Date).ToList();

                // 👇 Применяем фильтр по товарам
                if (_selectedProductsForReport.Any())
                {
                    var selectedNames = _selectedProductsForReport.Select(p => p.Name).ToHashSet();
                    filtered = filtered.Where(x => selectedNames.Contains(x.Name)).ToList();
                }
                else if (!allProducts)
                {
                    string productName = "";
                    if (ManualEntryCheckBox.IsChecked == true)
                        productName = ManualProductTextBox?.Text ?? "";
                    else
                        productName = ProductComboBox?.SelectedItem?.ToString() ?? "";

                    if (!string.IsNullOrEmpty(productName) && productName != "Нет данных")
                        filtered = filtered.Where(x => x.Name.Equals(productName, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                filteredData = filtered;
                ReportDataGrid.ItemsSource = filteredData;

                // Статистика (как у тебя было)
                int totalAdded = filtered.Sum(x => int.TryParse(x.OperationTypeIn, out int val) ? val : 0);
                int totalRemoved = filtered.Sum(x => int.TryParse(x.OperationTypeOut, out int val) ? val : 0);
                int totalRemaining = filtered.Sum(x => int.TryParse(x.NewQuantity, out int val) ? val : 0);

                string periodName = "Месяц";
                if (PeriodTypeComboBox?.SelectedItem is ComboBoxItem selectedItem)
                {
                    switch (selectedItem.Tag?.ToString())
                    {
                        case "Day": periodName = "День"; break;
                        case "Week": periodName = "Неделя"; break;
                        case "Month": periodName = "Месяц"; break;
                        case "Quarter": periodName = "Квартал"; break;
                    }
                }

                string productInfo = allProducts ? "Все" :
                    (_selectedProductsForReport.Any() ?
                        $"{_selectedProductsForReport.Count} товаров" :
                        (ManualEntryCheckBox.IsChecked == true ? ManualProductTextBox.Text : ProductComboBox?.SelectedItem?.ToString() ?? "Не выбран"));

                PreviewText.Text = $"📊 Найдено записей: {filtered.Count}\n" +
                                  $"📅 Период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}\n" +
                                  $"🏷️ Товар: {productInfo}\n" +
                                  $"📈 Периодичность: {periodName}\n\n" +
                                  $"➕ Всего добавлено: {totalAdded}\n" +
                                  $"➖ Всего убрано: {totalRemoved}\n" +
                                  $"✅ Остаток: {totalRemaining}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка UpdateData: {ex.Message}");
                PreviewText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void ManualEntryCheckBox_Checked ( object sender, RoutedEventArgs e )
        {
            if (ProductComboBox != null) ProductComboBox.Visibility = Visibility.Collapsed;
            if (ManualProductTextBox != null) ManualProductTextBox.Visibility = Visibility.Visible;
            if (SingleProductRadio != null) SingleProductRadio.IsChecked = true;
            UpdateData();
        }

        private void ManualEntryCheckBox_Unchecked ( object sender, RoutedEventArgs e )
        {
            if (ProductComboBox != null) ProductComboBox.Visibility = Visibility.Visible;
            if (ManualProductTextBox != null) ManualProductTextBox.Visibility = Visibility.Collapsed;
            UpdateData();
        }

        private void ProductComboBox_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            if (ManualEntryCheckBox != null && ManualEntryCheckBox.IsChecked == false)
                UpdateData();
        }

        private void PeriodTypeComboBox_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            UpdateData();
        }

        private void GenerateReportButton_Click ( object sender, RoutedEventArgs e )
        {
            try
            {
                if (StartDatePicker == null || EndDatePicker == null || AllProductsRadio == null)
                {
                    MessageBox.Show("Ошибка инициализации окна!", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var startDate = StartDatePicker.SelectedDate ?? DateTime.Now;
                var endDate = EndDatePicker.SelectedDate ?? DateTime.Now;
                var allProducts = AllProductsRadio.IsChecked == true;

                ReportPeriodType periodType = ReportPeriodType.Month;
                if (PeriodTypeComboBox?.SelectedItem is ComboBoxItem selectedItem)
                {
                    switch (selectedItem.Tag?.ToString())
                    {
                        case "Day": periodType = ReportPeriodType.Day; break;
                        case "Week": periodType = ReportPeriodType.Week; break;
                        case "Month": periodType = ReportPeriodType.Month; break;
                        case "Quarter": periodType = ReportPeriodType.Quarter; break;
                    }
                }

                // ========== ОСНОВНАЯ ЛОГИКА ==========
                List<NomenclatureUnit> dataForReport = null;
                string reportTitle = "";

                // Приоритет: если есть выбранные товары через диалог
                if (_selectedProductsForReport.Any())
                {
                    dataForReport = filteredData; // filteredData уже содержит только выбранные товары (по датам)
                    reportTitle = $"Выбрано товаров: {_selectedProductsForReport.Count}";

                    // Прямой вызов без повторной фильтрации
                    Excele.ReportToExceleDirect(dataForReport, startDate, endDate, reportTitle, allProducts, periodType);
                    return;
                }

                // Старая логика для одного товара или всех
                if (!allProducts)
                {
                    string productName = "";
                    if (ManualEntryCheckBox.IsChecked == true)
                        productName = ManualProductTextBox?.Text ?? "";
                    else
                        productName = ProductComboBox?.SelectedItem?.ToString() ?? "";

                    if (string.IsNullOrEmpty(productName) || productName == "Нет данных")
                    {
                        MessageBox.Show("Выберите или введите название товара!", "Предупреждение",
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    reportTitle = productName;
                }
                else
                {
                    reportTitle = "Все товары";
                }

                if (filteredData == null || filteredData.Count == 0)
                {
                    MessageBox.Show("Нет данных для формирования отчета!", "Предупреждение",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Вызов стандартного метода (с фильтрацией по товару)
                Excele.ReportToExcele(filteredData, startDate, endDate, reportTitle, allProducts, periodType);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}", "Ошибка");
            }
        
        }

        private void RefreshButton_Click ( object sender, RoutedEventArgs e )
        {
            UpdateData();
            MessageBox.Show("Данные обновлены!", "Обновление", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SelectMultipleButton_Click ( object sender, RoutedEventArgs e )
        {
            // Получаем уникальные товары из всех данных (без дублей)
            var uniqueProducts = allData
                .GroupBy(x => x.Name)
                .Select(g => g.First())
                .ToList();

            var selectionWindow = new SelectWindow(uniqueProducts, _selectedProductsForReport);
            if (selectionWindow.ShowDialog() == true)
            {
                _selectedProductsForReport = selectionWindow.SelectedProductsResult;
                UpdateData(); // обновляем таблицу и статистику
                MessageBox.Show($"Выбрано товаров: {_selectedProductsForReport.Count}", "Выбор",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CancelButton_Click ( object sender, RoutedEventArgs e )
        {
            
            Close();
        }
    }
}

