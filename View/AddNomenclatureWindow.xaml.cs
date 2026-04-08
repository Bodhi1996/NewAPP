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

        public AddNomenclatureWindow ()
        {
            InitializeComponent();
            dataBase = new DatabaseService ();
            this.DataContext = this;

            AddNumButton1 = new RelayCommand(ExecuteAddNomenclature, CanExecuteAddNomenclature);
        }
        public ICommand AddNumButton1 { get; }

        private void ExecuteAddNomenclature ( object param )
        {
            // Сохраняем данные
            Name = NameNomenclatureBox.Text.Trim();
            InternalArticle = InterNomenclatureBox.Text.Trim();
            ExternalArticle = ExterNomenclatureBox.Text.Trim();
            Characteristic = CharNomenclatureBox.Text.Trim();
            SerialNumber = SNNomenclatureBox.Text.Trim();
            Unit = UnitNomenclatureBox.Text.Trim();
            AdressCell = AdrNomenclatureBox.Text.Trim();
            int.TryParse(OldNomenclatureBox.Text.Trim(), out int oldQyt);
            int.TryParse(InNomenclatureBox.Text.Trim(), out int OperIn);
            int.TryParse(OutNomenclatureBox.Text.Trim(), out int OperOut);
            int.TryParse(NewNomenclatureBox.Text.Trim(), out int NewQ);

            OldQuantity = oldQyt;
            OperationTypeIn = OperIn;
            OperationTypeOut = OperOut;
            NewQuantity = NewQ;
            // Закрываем окно с успешным результатом
            this.DialogResult = true;
            this.Close();
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
                    !string.IsNullOrWhiteSpace(NewNomenclatureBox?.Text);
        }
    }
}
