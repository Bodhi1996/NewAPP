using NewAPP.Models;
using NewAPP.Services;
using NewAPP.Services;
using NewAPP.ViewModels;
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
using System.Xml;

namespace NewAPP.View
{
    /// <summary>
    /// Логика взаимодействия для AddNomenclatureWindow.xaml
    /// </summary>
    public partial class AddNomenclatureWindow : Window
    {
        private DatabaseService dataBase;
        private static List<bool> visibilitySettings = new List<bool>();
        private static bool isInitialized = false;

        public string Name { get; set; } //имя
        public string InternalArticle { get; set; } //внешний номер 
        public string ExternalArticle { get; set; } //внутренний номер
        public string Characteristic { get; set; } //характеристика 
        public string SerialNumber { get; set; } //серийный номер
        public string Unit { get; set; } //Единицы измерения
        public string AdressCell { get; set; } //адрес ячейки
        public int OldQuantity { get; set; } //старое кол-во
        public int OperationTypeIn { get; set; } //добавили
        public int OperationTypeOut { get; set; } //убавили
        public int NewQuantity { get; set; } //остаток
        public int UnitPrice { get; set; } //цена за единицу товара
        public int WeightUnit { get; set; } //вес за единицу

        public AddNomenclatureWindow ()
        {
            InitializeComponent();
            dataBase = new DatabaseService ();
            this.DataContext = this;

            if (!isInitialized)
            {
                InitDefaultSettings();
                isInitialized = true;
            }

            // Загружаем сохраненные настройки
            LoadSettingsFromList();

            AddNumButton1 = new RelayCommand(ExecuteAddNomenclature, CanExecuteAddNomenclature);
        }
        public event Action<(string val1, string val2, string val3, string val4, string val5, string val6, string val7, int val8, int val9, int val10, int val11, int val12, int val13)> peredacha;
        public ICommand AddNumButton1 { get; }


        private void InitDefaultSettings ( )
        {
            visibilitySettings.Clear();
            visibilitySettings.Add(true);  // Name
            visibilitySettings.Add(true);  // InNumber
            visibilitySettings.Add(true);  // OutNumber
            visibilitySettings.Add(true);  // Characteristick
            visibilitySettings.Add(true);  // SerialNumber
            visibilitySettings.Add(true);  // Price
            visibilitySettings.Add(true);  // Weight
            visibilitySettings.Add(true);  // Unit
            visibilitySettings.Add(true);  // Adr
            visibilitySettings.Add(true);  // OldQuantity
            visibilitySettings.Add(true);  // OperationIn
            visibilitySettings.Add(true);  // OperationOut
            visibilitySettings.Add(true);  // Quantity
        }

        private void LoadSettingsFromList ( )
        {
            if (visibilitySettings.Count < 13) return;

            NameLable.Visibility = visibilitySettings[0] ? Visibility.Visible : Visibility.Collapsed;
            NameNomenclatureBox.Visibility = visibilitySettings[0] ? Visibility.Visible : Visibility.Collapsed;

            InterLable.Visibility = visibilitySettings[1] ? Visibility.Visible : Visibility.Collapsed;
            InterNomenclatureBox.Visibility = visibilitySettings[1] ? Visibility.Visible : Visibility.Collapsed;

            ExterLable.Visibility = visibilitySettings[2] ? Visibility.Visible : Visibility.Collapsed;
            ExterNomenclatureBox.Visibility = visibilitySettings[2] ? Visibility.Visible : Visibility.Collapsed;

            CharLable.Visibility = visibilitySettings[3] ? Visibility.Visible : Visibility.Collapsed;
            CharNomenclatureBox.Visibility = visibilitySettings[3] ? Visibility.Visible : Visibility.Collapsed;

            SNLable.Visibility = visibilitySettings[4] ? Visibility.Visible : Visibility.Collapsed;
            SNNomenclatureBox.Visibility = visibilitySettings[4] ? Visibility.Visible : Visibility.Collapsed;

            UnitPriceLable.Visibility = visibilitySettings[5] ? Visibility.Visible : Visibility.Collapsed;
            UnitPriceBox.Visibility = visibilitySettings[5] ? Visibility.Visible : Visibility.Collapsed;

            WeightLable.Visibility = visibilitySettings[6] ? Visibility.Visible : Visibility.Collapsed;
            WeightUnitBox.Visibility = visibilitySettings[6] ? Visibility.Visible : Visibility.Collapsed;

            UnitNomLable.Visibility = visibilitySettings[7] ? Visibility.Visible : Visibility.Collapsed;
            UnitNomenclatureBox.Visibility = visibilitySettings[7] ? Visibility.Visible : Visibility.Collapsed;

            AdrLable.Visibility = visibilitySettings[8] ? Visibility.Visible : Visibility.Collapsed;
            AdrNomenclatureBox.Visibility = visibilitySettings[8] ? Visibility.Visible : Visibility.Collapsed;

            OldLable.Visibility = visibilitySettings[9] ? Visibility.Visible : Visibility.Collapsed;
            OldNomenclatureBox.Visibility = visibilitySettings[9] ? Visibility.Visible : Visibility.Collapsed;

            InNomLable.Visibility = visibilitySettings[10] ? Visibility.Visible : Visibility.Collapsed;
            InNomenclatureBox.Visibility = visibilitySettings[10] ? Visibility.Visible : Visibility.Collapsed;

            OutNomLAble.Visibility = visibilitySettings[11] ? Visibility.Visible : Visibility.Collapsed;
            OutNomenclatureBox.Visibility = visibilitySettings[11] ? Visibility.Visible : Visibility.Collapsed;

            NewNomLable.Visibility = visibilitySettings[12] ? Visibility.Visible : Visibility.Collapsed;
            NewNomenclatureBox.Visibility = visibilitySettings[12] ? Visibility.Visible : Visibility.Collapsed;

        }

        private void ExecuteAddNomenclature ( object param )
        {
            // Сохраняем данные
            Name = NameNomenclatureBox.Text.Trim(); //val1
            InternalArticle = InterNomenclatureBox.Text.Trim(); //val2
            ExternalArticle = ExterNomenclatureBox.Text.Trim(); //val3
            Characteristic = CharNomenclatureBox.Text.Trim(); //val4
            SerialNumber = SNNomenclatureBox.Text.Trim(); //val5
            Unit = UnitNomenclatureBox.Text.Trim(); //val6
            AdressCell = AdrNomenclatureBox.Text.Trim(); //val7
            int.TryParse(OldNomenclatureBox.Text.Trim(), out int oldQyt); //val8
            int.TryParse(InNomenclatureBox.Text.Trim(), out int OperIn);//val9
            int.TryParse(OutNomenclatureBox.Text.Trim(), out int OperOut); //val10
            int.TryParse(NewNomenclatureBox.Text.Trim(), out int NewQ); //val11
            int.TryParse(UnitPriceBox.Text.Trim(), out int UnitPri); //val12
            int.TryParse(WeightUnitBox.Text.Trim(), out int WeUnit); //val13

            OldQuantity = oldQyt;
            OperationTypeIn = OperIn;
            OperationTypeOut = OperOut;
            NewQuantity = OperIn;
            UnitPrice = UnitPri;
            WeightUnit = WeUnit;
            // Закрываем окно с успешным результатом
            peredacha?.Invoke((Name, InternalArticle, ExternalArticle, Characteristic, SerialNumber, Unit, AdressCell, OldQuantity, OperationTypeIn, OperationTypeOut, NewQuantity, UnitPrice, WeightUnit));
        }

        private void InputField_Click(object sender, EventArgs e)
        {
            var inputVisibleField = new InputVisibleField();

            inputVisibleField.chkName.IsChecked = visibilitySettings[0];
            inputVisibleField.chkInNumber.IsChecked = visibilitySettings[1];
            inputVisibleField.chkOutNumber.IsChecked = visibilitySettings[2];
            inputVisibleField.chkHaracteristick.IsChecked = visibilitySettings[3];
            inputVisibleField.chkSerialNumber.IsChecked = visibilitySettings[4];
            inputVisibleField.chkPrice.IsChecked = visibilitySettings[5];
            inputVisibleField.chkWeight.IsChecked = visibilitySettings[6];
            inputVisibleField.chkUnit.IsChecked = visibilitySettings[7];
            inputVisibleField.chkAdr.IsChecked = visibilitySettings[8];
            inputVisibleField.chkOldQuantity.IsChecked = visibilitySettings[9];
            inputVisibleField.chkOperationIn.IsChecked = visibilitySettings[10];
            inputVisibleField.chkOperationOut.IsChecked = visibilitySettings[11];
            inputVisibleField.chkQuntity.IsChecked = visibilitySettings[12];

            inputVisibleField.visibleAction += ( showName, showInNumber, showOutNumber, showHaracteristick,
                showSerialNumber, showPrice, showWeight, showUnit, showAdr, showOldQuantity, showOperationIn, showOperationOut, showQuantity ) =>
            {
                NameLable.Visibility = showName ? Visibility.Visible : Visibility.Collapsed;
                NameNomenclatureBox.Visibility = showName ? Visibility.Visible : Visibility.Collapsed;

                InterLable.Visibility = showInNumber ? Visibility.Visible : Visibility.Collapsed;
                InterNomenclatureBox.Visibility = showInNumber ? Visibility.Visible : Visibility.Collapsed;

                ExterLable.Visibility = showOutNumber ? Visibility.Visible : Visibility.Collapsed;
                ExterNomenclatureBox.Visibility = showOutNumber ? Visibility.Visible : Visibility.Collapsed;

                CharLable.Visibility = showHaracteristick ? Visibility.Visible : Visibility.Collapsed;
                CharNomenclatureBox.Visibility = showHaracteristick ? Visibility.Visible : Visibility.Collapsed;

                SNLable.Visibility = showSerialNumber ? Visibility.Visible : Visibility.Collapsed;
                SNNomenclatureBox.Visibility = showSerialNumber ? Visibility.Visible : Visibility.Collapsed;

                UnitPriceLable.Visibility = showPrice ? Visibility.Visible : Visibility.Collapsed;
                UnitPriceBox.Visibility = showPrice ? Visibility.Visible : Visibility.Collapsed;

                WeightLable.Visibility = showWeight ? Visibility.Visible : Visibility.Collapsed;
                WeightUnitBox.Visibility = showWeight ? Visibility.Visible : Visibility.Collapsed;

                UnitNomLable.Visibility = showUnit ? Visibility.Visible : Visibility.Collapsed;
                UnitNomenclatureBox.Visibility = showUnit ? Visibility.Visible : Visibility.Collapsed;

                AdrLable.Visibility = showAdr ? Visibility.Visible : Visibility.Collapsed;
                AdrNomenclatureBox.Visibility = showAdr ? Visibility.Visible : Visibility.Collapsed;

                OldLable.Visibility = showOldQuantity ? Visibility.Visible : Visibility.Collapsed;
                OldNomenclatureBox.Visibility = showOldQuantity ? Visibility.Visible : Visibility.Collapsed;

                InNomLable.Visibility = showOperationIn ? Visibility.Visible : Visibility.Collapsed;
                InNomenclatureBox.Visibility = showOperationIn ? Visibility.Visible : Visibility.Collapsed;

                OutNomLAble.Visibility = showOperationOut ? Visibility.Visible : Visibility.Collapsed;
                OutNomenclatureBox.Visibility = showOperationOut ? Visibility.Visible : Visibility.Collapsed;

                NewNomLable.Visibility = showQuantity ? Visibility.Visible : Visibility.Collapsed;
                NewNomenclatureBox.Visibility = showQuantity ? Visibility.Visible : Visibility.Collapsed;

                SaveSettingsToList(showName, showInNumber, showOutNumber,
                                 showHaracteristick, showSerialNumber,
                                 showPrice, showWeight, showUnit,
                                 showAdr, showOldQuantity, showOperationIn,
                                 showOperationOut, showQuantity);

                (AddNumButton1 as RelayCommand)?.RaiseCanExecuteChanged();

                MessageBox.Show("Настройки видимости применены!", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);


            };
            inputVisibleField.ShowDialog();
        }

        private void SaveSettingsToList ( bool showName, bool showInNumber, bool showOutNumber,
                                       bool showCharacteristick, bool showSerialNumber,
                                       bool showPrice, bool showWeight, bool showUnit,
                                       bool showAdr, bool showOldQuantity, bool showOperationIn,
                                       bool showOperationOut, bool showQuantity )
        {
            visibilitySettings[0] = showName;
            visibilitySettings[1] = showInNumber;
            visibilitySettings[2] = showOutNumber;
            visibilitySettings[3] = showCharacteristick;
            visibilitySettings[4] = showSerialNumber;
            visibilitySettings[5] = showPrice;
            visibilitySettings[6] = showWeight;
            visibilitySettings[7] = showUnit;
            visibilitySettings[8] = showAdr;
            visibilitySettings[9] = showOldQuantity;
            visibilitySettings[10] = showOperationIn;
            visibilitySettings[11] = showOperationOut;
            visibilitySettings[12] = showQuantity;

            System.Diagnostics.Debug.WriteLine($"Сохранено: Name={showName}, Unit={showUnit}");
        }

        // Метод проверки - возвращает bool, доступна ли команда
        private bool CanExecuteAddNomenclature ( object param )
        {
            // Проверяем, заполнены ли все поля
            if (NameLable.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(NameNomenclatureBox?.Text))
                return false;

            if (InterLable.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(InterNomenclatureBox?.Text))
                return false;

            if (ExterLable.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(ExterNomenclatureBox?.Text))
                return false;

            if (CharLable.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(CharNomenclatureBox?.Text))
                return false;

            if (SNLable.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(SNNomenclatureBox?.Text))
                return false;

            if (UnitPriceLable.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(UnitPriceBox?.Text))
                return false;

            if (WeightLable.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(WeightUnitBox?.Text))
                return false;

            // Проверяем только видимые поля (правая колонка)
            if (UnitNomLable.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(UnitNomenclatureBox?.Text))
                return false;

            if (AdrLable.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(AdrNomenclatureBox?.Text))
                return false;

            if (OldLable.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(OldNomenclatureBox?.Text))
                return false;

            if (InNomLable.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(InNomenclatureBox?.Text))
                return false;

            if (OutNomLAble.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(OutNomenclatureBox?.Text))
                return false;

            if (NewNomLable.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(NewNomenclatureBox?.Text))
                return false;

            return true;

        }
    }
}
