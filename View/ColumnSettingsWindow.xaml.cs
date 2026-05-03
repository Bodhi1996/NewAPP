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
    /// Логика взаимодействия для ColumnSettingsWindow.xaml
    /// </summary>
    public partial class ColumnSettingsWindow : Window
    {
        public bool ShowId { get; private set; }
        public bool ShowName { get; private set; }
        public bool ShowInNumber { get; private set; }
        public bool ShowOutNumber { get; private set; }
        public bool ShowCharacteristick { get; private set; }
        public bool ShowUnit { get; private set; }
        public bool ShowAdr { get; private set; }
        public bool ShowQuntity { get; private set; }
        public bool ShowUnitPrice { get; private set; }
        public bool ShowWeightUnit { get; set; }    

        public ColumnSettingsWindow ( bool currentShowId = true,
                                    bool currentShowName = true,
                                    bool currentShowInNumber = true,
                                    bool currentShowOutNumber = true,
                                    bool currentShowCharacteristick = true,
                                    bool currentShowUnit = true,
                                    bool currentShowAdr = true,
                                    bool currentShowQuantity = true,
                                    bool currentShowUnitPrice = true,
                                    bool showWeightUnit = true )
        {
            InitializeComponent();

            chkId.IsChecked = currentShowId;
            chkName.IsChecked = currentShowName;
            chkInNumber.IsChecked = currentShowInNumber;
            chkOutNumber.IsChecked = currentShowOutNumber;
            chkHaracteristick.IsChecked = currentShowCharacteristick;
            chkUnit.IsChecked = currentShowUnit;
            chkAdr.IsChecked = currentShowAdr;
            chkQuntity.IsChecked = currentShowQuantity;
            chkPrice.IsChecked = currentShowUnitPrice;
            ShowWeightUnit = showWeightUnit;

            // Обновляем свойства
            UpdatePropertiesFromCheckboxes();
            
         }
        private void CheckBox_Changed ( object sender, RoutedEventArgs e )
        {
            UpdatePropertiesFromCheckboxes();
        }

        private void UpdatePropertiesFromCheckboxes ( )
        {
            // Добавляем проверку на null для безопасности
            ShowId = chkId?.IsChecked ?? true;
            ShowName = chkName?.IsChecked ?? true;
            ShowInNumber = chkInNumber?.IsChecked ?? true;
            ShowOutNumber = chkOutNumber?.IsChecked ?? true;
            ShowCharacteristick = chkHaracteristick?.IsChecked ?? true;
            ShowUnit = chkUnit?.IsChecked ?? true;
            ShowAdr = chkAdr?.IsChecked ?? true;
            ShowQuntity = chkQuntity?.IsChecked ?? true;
            ShowUnitPrice = chkPrice?.IsChecked ?? true;
            ShowWeightUnit = chkWeightUnit?.IsChecked ?? true;
        }

        private void OkButton_Click ( object sender, RoutedEventArgs e )
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click ( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
            Close();
        }
    }
}
