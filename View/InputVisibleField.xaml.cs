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

namespace NewAPP.View
{
    /// <summary>
    /// Логика взаимодействия для InputVisibleField.xaml
    /// </summary>
    public partial class InputVisibleField : Window
    {
        public bool ShowName {  get; set; } //1
        public bool ShowInNumber {  get; set; } //2
        public bool ShowOutNumber {  get; set; } //2
        public bool ShowHaracteristick {  get; set; } //3
        public bool ShowSerialNumber {  get; set; } //4
        public bool ShowPrice {  get; set; } //5
        public bool ShowWeight {  get; set; } //6
        public bool ShowUnit {  get; set; } //7
        public bool ShowAdr {  get; set; } //8
        public bool ShowOldQuantity {  get; set; } //9
        public bool ShowOperationIn {  get; set; }//10
        public bool ShowOperationOut {  get; set; } //11
        public bool ShowQuantity {  get; set; } //12

        public event Action<bool, bool, bool, bool, bool, bool, bool, bool, bool, bool, bool, bool, bool> visibleAction;

        public InputVisibleField ( )
        {
            InitializeComponent();
        }

        private void OkButton_Click( object sender, RoutedEventArgs e )
        {
            ShowName = chkName?.IsChecked == true;
            ShowInNumber = chkInNumber?.IsChecked == true;
            ShowOutNumber = chkOutNumber?.IsChecked == true;
            ShowHaracteristick = chkHaracteristick?.IsChecked == true;
            ShowSerialNumber = chkSerialNumber?.IsChecked == true;
            ShowPrice = chkPrice?.IsChecked == true;
            ShowWeight = chkWeight?.IsChecked == true;
            ShowUnit = chkUnit?.IsChecked == true;
            ShowAdr = chkAdr?.IsChecked == true;
            ShowOldQuantity = chkOldQuantity?.IsChecked == true;
            ShowOperationIn = chkOperationIn?.IsChecked == true;
            ShowOperationOut = chkOperationOut?.IsChecked == true;
            ShowQuantity = chkQuntity?.IsChecked == true;

            visibleAction?.Invoke(ShowName, ShowInNumber, ShowOutNumber, ShowHaracteristick, ShowSerialNumber, ShowPrice, ShowWeight, ShowUnit, ShowAdr, ShowOldQuantity, ShowOperationIn, ShowOperationOut, ShowQuantity);

            this.DialogResult = true;
            this.Close();
        }

        private void CheckBox_Changed ( object sender, RoutedEventArgs e )
        {
            // Можно оставить пустым, если не нужна дополнительная логика
            // Или добавить логику, например:
            // var checkBox = sender as CheckBox;
            // if (checkBox.IsChecked == true)
            // {
            //     // Что-то делаем при установке галочки
            // }
        }


    }
}
