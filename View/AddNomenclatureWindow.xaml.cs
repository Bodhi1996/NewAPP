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
using NewAPP.Services;
using NewAPP.ViewModels;

namespace NewAPP.View
{
    /// <summary>
    /// Логика взаимодействия для AddNomenclatureWindow.xaml
    /// </summary>
    public partial class AddNomenclatureWindow : Window
    {
        private DatabaseService dataBase;

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

            AddNumButton1 = new RelayCommand(ExecuteAddNomenclature, CanExecuteAddNomenclature);
        }
        public event Action<(string val1, string val2, string val3, string val4, string val5, string val6, string val7, int val8, int val9, int val10, int val11, int val12, int val13)> peredacha;
        public ICommand AddNumButton1 { get; }

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

        // Метод проверки - возвращает bool, доступна ли команда
        private bool CanExecuteAddNomenclature ( object param )
        {
            // Проверяем, заполнены ли все поля
            return !string.IsNullOrWhiteSpace(NameNomenclatureBox?.Text) &&
                    !string.IsNullOrWhiteSpace(InterNomenclatureBox?.Text) &&
                    !string.IsNullOrWhiteSpace(ExterNomenclatureBox?.Text) &&
                    !string.IsNullOrWhiteSpace(CharNomenclatureBox?.Text) &&
                    !string.IsNullOrWhiteSpace(SNNomenclatureBox?.Text) &&
                    !string.IsNullOrWhiteSpace(UnitNomenclatureBox?.Text) &&
                    !string.IsNullOrWhiteSpace(AdrNomenclatureBox?.Text) &&
                    !string.IsNullOrWhiteSpace(OldNomenclatureBox?.Text) &&
                    !string.IsNullOrWhiteSpace(InNomenclatureBox?.Text) &&
                    !string.IsNullOrWhiteSpace(OutNomenclatureBox?.Text) &&
                    !string.IsNullOrWhiteSpace(UnitPriceBox?.Text) &&
                    !string.IsNullOrWhiteSpace(NewNomenclatureBox?.Text) &&
                    !string.IsNullOrWhiteSpace(WeightUnitBox?.Text);
        }
    }
}
