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

namespace NewAPP.View
{
    /// <summary>
    /// Логика взаимодействия для CurrectQuantityWindow.xaml
    /// </summary>
    public partial class CurrectQuantityWindow : Window
    {
        DatabaseService _db;
        public string Name { get; set; }
        public int Amount { get; set; }

        public CurrectQuantityWindow ( )
        {
            InitializeComponent();
            _db = new DatabaseService();
            this.DataContext = this;
            CurrentQuantityCommand = new RelayCommand(ExecuteCorrectQuantity, CanExecuteCorrectQuantity);
        }

        public ICommand CurrentQuantityCommand { get; }

        public void ExecuteCorrectQuantity ( object param )
        {
            Name = NameNomenclatureBox.Text.Trim();
            Amount = int.Parse(AmountTextBox.Text.Trim());
            this.DialogResult = true;
            this.Close();
        }

        private bool CanExecuteCorrectQuantity (object param)
        {
            return !string.IsNullOrWhiteSpace(NameNomenclatureBox?.Text);
        }
        //public void Current_Click ( object sender, RoutedEventArgs e )
        //{
        //    Name = NameNomenclatureBox.Text.Trim();
        //    Amount = int.Parse(AmountTextBox.Text.Trim());
        //    _db.CorrectUnit(Name, Amount);
        //}
    }
}
