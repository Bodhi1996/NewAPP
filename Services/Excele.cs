using Microsoft.Win32;
using NewAPP.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;

namespace NewAPP.Services
{
    class Excele
    {
        public enum ReportPeriodType
        {
            Day,      // по дням
            Week,     // по неделям
            Month,    // по месяцам
            Quarter   // по кварталам
        }

        public Excele() 
        {
            ExcelPackage.License.SetNonCommercialPersonal("<LolKek123>");
        }

        public bool ReportToExcele ( List<NomenclatureUnit> data, DateTime startDate, DateTime endDate,
                           string searchName, bool allProducts, ReportPeriodType periodType )
        {
            try
            {
                var filter = data.Where(x =>
                {
                    if (x.OperationDate.HasValue)
                    {
                        return x.OperationDate.Value.Date >= startDate.Date &&
                               x.OperationDate.Value.Date <= endDate.Date;
                    }
                    return false;
                }).ToList();

                List<NomenclatureUnit> name;
                if (!allProducts && !string.IsNullOrEmpty(searchName))
                {
                    name = filter.Where(x => x.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (name.Count == 0)
                    {
                        MessageBox.Show($"Товар '{searchName}' не найден за выбранный период!",
                              "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }
                else
                {
                    name = filter;
                }

                if (name.Count == 0)
                {
                    MessageBox.Show("Нет данных за выбранный период!", "Предупреждение",
                          MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                return ExportToExele(name, startDate, endDate, searchName, allProducts, periodType);
            }
            catch (Exception ex)
            {
                throw;
            }
        } //фильтрация
        private bool ExportToExele ( List<NomenclatureUnit> data, DateTime startDate, DateTime endDate,
                                   string searchName, bool allProducts, ReportPeriodType periodType )
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel файлы (*.xlsx)|*.xlsx",
                    DefaultExt = "xlsx",
                    FileName = $"Отчет_по_номенклатуре_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var package = new ExcelPackage())
                    {
                        // ========== ЛИСТ 1: Детальный отчет ==========
                        var worksheet1 = package.Workbook.Worksheets.Add("Детальный отчет");

                        // Заголовок
                        worksheet1.Cells["A1:M1"].Merge = true;
                        worksheet1.Cells["A1"].Value = "ОТЧЕТ ПО НОМЕНКЛАТУРЕ (Детальный)";
                        worksheet1.Cells["A1"].Style.Font.Bold = true;
                        worksheet1.Cells["A1"].Style.Font.Size = 16;
                        worksheet1.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        // Информация о периоде и фильтрах
                        worksheet1.Cells["A3"].Value = $"Период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
                        worksheet1.Cells["A4"].Value = $"Товар: {(allProducts ? "Все" : searchName)}";
                        worksheet1.Cells["A5"].Value = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm:ss}";
                        worksheet1.Cells["A6"].Value = $"Периодичность: {GetPeriodTypeName(periodType)}";

                        string[] headers = {"ID", "Наименование", "Внешний номер", "Номер клиента",
                    "Характеристика", "Серийный номер", "Ед. изм.", "Адрес",
                    "Начальное кол-во", "Добавили", "Убрали", "Остаток","Цена", "Дата операции"};

                        int headerRow = 8; // Сдвинули из-за добавленной строки
                        for (int i = 0; i < headers.Length; i++)
                        {
                            worksheet1.Cells[headerRow, i + 1].Value = headers[i];
                            worksheet1.Cells[headerRow, i + 1].Style.Font.Bold = true;
                            worksheet1.Cells[headerRow, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet1.Cells[headerRow, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                            worksheet1.Cells[headerRow, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }

                        int row = headerRow + 1;
                        foreach (var item in data)
                        {
                            worksheet1.Cells[row, 1].Value = item.Id;
                            worksheet1.Cells[row, 2].Value = item.Name;
                            worksheet1.Cells[row, 3].Value = item.InternalArticle;
                            worksheet1.Cells[row, 4].Value = item.ExternalArticle;
                            worksheet1.Cells[row, 5].Value = item.Characteristic;
                            worksheet1.Cells[row, 6].Value = item.SerialNamber;
                            worksheet1.Cells[row, 7].Value = item.Unit;
                            worksheet1.Cells[row, 8].Value = item.AddressCell;
                            worksheet1.Cells[row, 9].Value = item.OldQuantity;
                            worksheet1.Cells[row, 10].Value = item.OperationTypeIn;
                            worksheet1.Cells[row, 11].Value = item.OperationTypeOut;
                            worksheet1.Cells[row, 12].Value = item.NewQuantity;
                            worksheet1.Cells[row, 13].Value = item.UnitPrice;
                            worksheet1.Cells[row, 14].Value = item.OperationDate?.ToString("dd.MM.yyyy HH:mm:ss") ?? "Не указана";
                            row++;
                        }

                        // Итоговая строка для листа 1
                        worksheet1.Cells[row, 1].Value = "ИТОГО:";
                        worksheet1.Cells[row, 1].Style.Font.Bold = true;

                        int totalAdded = data.Sum(x => int.TryParse(x.OperationTypeIn, out int val) ? val : 0);
                        int totalRemoved = data.Sum(x => int.TryParse(x.OperationTypeOut, out int val) ? val : 0);
                        int totalRemaining = data.Sum(x => int.TryParse(x.NewQuantity, out int val) ? val : 0);
                        int totalPrice = data.Sum(x => (int.TryParse(x.UnitPrice, out int val) ? val : 0) *
                            (int.TryParse(x.NewQuantity, out int p) ? p : 0)) ;

                        worksheet1.Cells[row, 10].Value = totalAdded;
                        worksheet1.Cells[row, 11].Value = totalRemoved;
                        worksheet1.Cells[row, 12].Value = totalRemaining;
                        worksheet1.Cells[row, 13].Value = totalPrice;
                        worksheet1.Cells[row, 10, row, 12].Style.Font.Bold = true;

                        worksheet1.Cells.AutoFitColumns();

                        // ========== ЛИСТ 2: Сводный отчет по периодам ==========
                        var worksheet2 = package.Workbook.Worksheets.Add("Сводный отчет");

                        // Строка 1: Период
                        worksheet2.Cells["A1"].Value = "Период:";
                        worksheet2.Cells["B1"].Value = startDate.ToString("yyyy-MM-dd HH:mm:ss");
                        worksheet2.Cells["C1"].Value = endDate.ToString("yyyy-MM-dd HH:mm:ss");

                        // Строка 2: Товар
                        worksheet2.Cells["A2"].Value = "Товар";
                        worksheet2.Cells["B2"].Value = allProducts ? "Все" : searchName;

                        // Строка 3: Дата формирования
                        worksheet2.Cells["A3"].Value = "Дата формирования:";
                        worksheet2.Cells["B3"].Value = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

                        // Строка 4: Тип периодичности
                        worksheet2.Cells["A4"].Value = "Периодичность:";
                        worksheet2.Cells["B4"].Value = GetPeriodTypeName(periodType);

                        // Получаем периоды в зависимости от выбранного типа
                        var periods = GetPeriods(startDate, endDate, periodType);

                        // Строка 6: Заголовки с наименованиями
                        int headerRow2 = 6;
                        int colIndex = 1;

                        // Базовые колонки (A-G)
                        string[] baseColumns = { "Наименование", "Внутренний артикул (присвоенный клиентом самостоятельно)",
                         "Внешний артикул (присвоенный поставщиком)", "Дополнительная характеристика",
                         "Серийный номер", "Единицы измерения", "Адрес ячейки" };

                        for (int i = 0; i < baseColumns.Length; i++)
                        {
                            worksheet2.Cells[headerRow2, colIndex].Value = baseColumns[i];
                            worksheet2.Cells[headerRow2, colIndex].Style.Font.Bold = true;
                            worksheet2.Cells[headerRow2, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet2.Cells[headerRow2, colIndex].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                            worksheet2.Cells[headerRow2, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet2.Cells[headerRow2, colIndex].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            colIndex++;
                        }

                        // Запоминаем позицию первой колонки с датами
                        int firstDateColumn = colIndex;

                        // Заполняем даты периодов (строка headerRow2)
                        foreach (var period in periods)
                        {
                            string periodLabel = GetPeriodLabel(period, periodType);
                            worksheet2.Cells[headerRow2, colIndex].Value = periodLabel;
                            worksheet2.Cells[headerRow2, colIndex].Style.Font.Bold = true;
                            worksheet2.Cells[headerRow2, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet2.Cells[headerRow2, colIndex].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                            worksheet2.Cells[headerRow2, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            // Объединяем 4 ячейки для каждого периода
                            worksheet2.Cells[headerRow2, colIndex, headerRow2, colIndex + 3].Merge = true;

                            colIndex += 4;
                        }

                        // Строка headerRow2 + 1: Подзаголовки
                        colIndex = firstDateColumn;
                        foreach (var period in periods)
                        {
                            worksheet2.Cells[headerRow2 + 1, colIndex].Value = "Начальный остаток";
                            worksheet2.Cells[headerRow2 + 1, colIndex + 1].Value = "Приход";
                            worksheet2.Cells[headerRow2 + 1, colIndex + 2].Value = "Расход";
                            worksheet2.Cells[headerRow2 + 1, colIndex + 3].Value = "Конечный остаток";

                            for (int i = 0; i < 4; i++)
                            {
                                worksheet2.Cells[headerRow2 + 1, colIndex + i].Style.Font.Bold = true;
                                worksheet2.Cells[headerRow2 + 1, colIndex + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet2.Cells[headerRow2 + 1, colIndex + i].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                                worksheet2.Cells[headerRow2 + 1, colIndex + i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                worksheet2.Cells[headerRow2 + 1, colIndex + i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            }

                            colIndex += 4;
                        }

                        // Группируем данные по номенклатуре
                        var nomenclatureGroups = data.GroupBy(x => new {
                            x.Name,
                            x.InternalArticle,
                            x.ExternalArticle,
                            x.Characteristic,
                            x.SerialNamber,
                            x.Unit,
                            x.AddressCell
                        });

                        int dataRow = headerRow2 + 2;

                        foreach (var nomGroup in nomenclatureGroups)
                        {
                            var firstItem = nomGroup.First();

                            // Заполняем базовые колонки (A-G)
                            worksheet2.Cells[dataRow, 1].Value = firstItem.Name;
                            worksheet2.Cells[dataRow, 2].Value = firstItem.InternalArticle;
                            worksheet2.Cells[dataRow, 3].Value = firstItem.ExternalArticle;
                            worksheet2.Cells[dataRow, 4].Value = firstItem.Characteristic;
                            worksheet2.Cells[dataRow, 5].Value = firstItem.SerialNamber;
                            worksheet2.Cells[dataRow, 6].Value = firstItem.Unit;
                            worksheet2.Cells[dataRow, 7].Value = firstItem.AddressCell;

                            // Заполняем данные по периодам
                            colIndex = firstDateColumn;
                            decimal? previousEndingBalance = null;

                            for (int p = 0; p < periods.Count; p++)
                            {
                                var period = periods[p];

                                // Получаем операции за текущий период
                                var periodOperations = GetOperationsForPeriod(nomGroup, period, periodType)
                                    .OrderBy(x => x.OperationDate)
                                    .ToList();

                                // Получаем остаток на начало периода
                                decimal? startBalance;

                                if (previousEndingBalance.HasValue)
                                {
                                    startBalance = previousEndingBalance.Value;
                                }
                                else
                                {
                                    // Для первого периода - берем начальный остаток из самой ранней операции
                                    var firstOperation = nomGroup.OrderBy(x => x.OperationDate).FirstOrDefault();
                                    if (firstOperation != null && !string.IsNullOrEmpty(firstOperation.OldQuantity))
                                    {
                                        if (decimal.TryParse(firstOperation.OldQuantity, out decimal parsedValue))
                                        {
                                            startBalance = parsedValue;
                                        }
                                        else
                                        {
                                            startBalance = 0;
                                        }
                                    }
                                    else
                                    {
                                        startBalance = 0;
                                    }
                                }

                                // Считаем приход и расход за период
                                decimal income = periodOperations.Sum(x =>
                                {
                                    if (decimal.TryParse(x.OperationTypeIn, out decimal val))
                                        return val;
                                    return 0;
                                });

                                decimal expense = periodOperations.Sum(x =>
                                {
                                    if (decimal.TryParse(x.OperationTypeOut, out decimal val))
                                        return val;
                                    return 0;
                                });

                                // Конечный остаток
                                decimal endingBalance = (startBalance ?? 0) + income - expense;

                                // Если нет операций за период, используем остаток с предыдущего периода
                                if (!periodOperations.Any() && previousEndingBalance.HasValue)
                                {
                                    startBalance = previousEndingBalance;
                                    endingBalance = previousEndingBalance.Value;
                                }

                                // Заполняем ячейки
                                worksheet2.Cells[dataRow, colIndex].Value = startBalance;
                                worksheet2.Cells[dataRow, colIndex + 1].Value = income > 0 ? income : (object)null;
                                worksheet2.Cells[dataRow, colIndex + 2].Value = expense > 0 ? expense : (object)null;
                                worksheet2.Cells[dataRow, colIndex + 3].Value = endingBalance;

                                previousEndingBalance = endingBalance;
                                colIndex += 4;
                            }

                            dataRow++;
                        }

                        // Применяем границы и форматирование
                        worksheet2.Cells[worksheet2.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        worksheet2.Cells[worksheet2.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        worksheet2.Cells[worksheet2.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        worksheet2.Cells[worksheet2.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                        worksheet2.Cells.AutoFitColumns();

                        // Сохраняем файл
                        FileInfo fileInfo = new FileInfo(saveFileDialog.FileName);
                        package.SaveAs(fileInfo);

                        MessageBox.Show($"Отчет успешно сохранен!\n\nФайл: {saveFileDialog.FileName}\n\n" +
                                      $"Сформировано 2 листа:\n" +
                                      $"1. Детальный отчет\n" +
                                      $"2. Сводный отчет (по {GetPeriodTypeName(periodType).ToLower()})",
                                      "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        public List<string> GetAllProductNames ( List<NomenclatureUnit> allData )
        {
            try
            {
                if (allData == null || allData.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("AllData пустой или null");
                    return new List<string>();
                }

                var names = allData.Select(x => x.Name).Distinct().OrderBy(x => x).ToList();
                System.Diagnostics.Debug.WriteLine($"Найдено уникальных товаров: {names.Count}");

                return names;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetAllProductNames: {ex.Message}");
                return new List<string>();
            }
        }

        private List<DateTime> GetPeriods ( DateTime startDate, DateTime endDate, ReportPeriodType periodType )
        {
            var periods = new List<DateTime>();

            switch (periodType)
            {
                case ReportPeriodType.Day:
                    for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                    {
                        periods.Add(date);
                    }
                    break;

                case ReportPeriodType.Week:
                    var startOfFirstWeek = startDate.Date;
                    while (startOfFirstWeek.DayOfWeek != DayOfWeek.Monday)
                    {
                        startOfFirstWeek = startOfFirstWeek.AddDays(-1);
                    }
                    for (var date = startOfFirstWeek; date <= endDate.Date; date = date.AddDays(7))
                    {
                        periods.Add(date);
                    }
                    break;

                case ReportPeriodType.Month:
                    for (var date = new DateTime(startDate.Year, startDate.Month, 1);
                         date <= new DateTime(endDate.Year, endDate.Month, 1);
                         date = date.AddMonths(1))
                    {
                        periods.Add(date);
                    }
                    break;

                case ReportPeriodType.Quarter:
                    for (var date = new DateTime(startDate.Year, ((startDate.Month - 1) / 3 * 3) + 1, 1);
                         date <= new DateTime(endDate.Year, ((endDate.Month - 1) / 3 * 3) + 1, 1);
                         date = date.AddMonths(3))
                    {
                        periods.Add(date);
                    }
                    break;
            }

            return periods;
        }

        // Получение операций за конкретный период
        private IEnumerable<NomenclatureUnit> GetOperationsForPeriod (
            IGrouping<object, NomenclatureUnit> nomGroup,
            DateTime period,
            ReportPeriodType periodType )
        {
            switch (periodType)
            {
                case ReportPeriodType.Day:
                    return nomGroup.Where(x => x.OperationDate.HasValue &&
                                               x.OperationDate.Value.Date == period.Date);

                case ReportPeriodType.Week:
                    return nomGroup.Where(x => x.OperationDate.HasValue &&
                                               x.OperationDate.Value.Date >= period &&
                                               x.OperationDate.Value.Date < period.AddDays(7));

                case ReportPeriodType.Month:
                    return nomGroup.Where(x => x.OperationDate.HasValue &&
                                               x.OperationDate.Value.Year == period.Year &&
                                               x.OperationDate.Value.Month == period.Month);

                case ReportPeriodType.Quarter:
                    return nomGroup.Where(x => x.OperationDate.HasValue &&
                                               x.OperationDate.Value.Year == period.Year &&
                                               (x.OperationDate.Value.Month - 1) / 3 == (period.Month - 1) / 3);

                default:
                    return Enumerable.Empty<NomenclatureUnit>();
            }
        }

        // Получение названия типа периодичности
        private string GetPeriodTypeName ( ReportPeriodType periodType )
        {
            switch (periodType)
            {
                case ReportPeriodType.Day: return "День";
                case ReportPeriodType.Week: return "Неделя";
                case ReportPeriodType.Month: return "Месяц";
                case ReportPeriodType.Quarter: return "Квартал";
                default: return "Неизвестно";
            }
        }

        // Получение метки для отображения периода
        private string GetPeriodLabel ( DateTime period, ReportPeriodType periodType )
        {
            switch (periodType)
            {
                case ReportPeriodType.Day:
                    return period.ToString("yyyy-MM-dd");

                case ReportPeriodType.Week:
                    var endOfWeek = period.AddDays(6);
                    return $"{period:yyyy-MM-dd} - {endOfWeek:yyyy-MM-dd}";

                case ReportPeriodType.Month:
                    return period.ToString("yyyy-MM-dd HH:mm:ss");

                case ReportPeriodType.Quarter:
                    int quarterNumber = ((period.Month - 1) / 3) + 1;
                    return $"{period.Year}-Q{quarterNumber}";

                default:
                    return period.ToString("yyyy-MM-dd");
            }
        }
        public bool ReportToExceleDirect ( List<NomenclatureUnit> data, DateTime startDate, DateTime endDate,
                                 string reportTitle, bool allProducts, ReportPeriodType periodType )
        {
            return ExportToExele(data, startDate, endDate, reportTitle, allProducts, periodType);
        }


    }
}
