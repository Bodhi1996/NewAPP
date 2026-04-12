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
    /// Логика взаимодействия для DeleteNomenclatureWindow.xaml
    /// </summary>
    public partial class DeleteNomenclatureWindow : Window
    {
        private DatabaseService dataBase;
        public string Name { get; set; }

        public DeleteNomenclatureWindow()
        {
            InitializeComponent();
            dataBase = new DatabaseService();
            this.DataContext = this;
            DeleteButtonNumCommand = new RelayCommand(ExecuteAddNomenclature, CanExecuteAddNomenclature);
        }

        public event Action<string> deleteName;

        public ICommand DeleteButtonNumCommand { get; }

        private void ExecuteAddNomenclature ( object param )
        {
            // Сохраняем данные
            Name = DeleteTextBox.Text.Trim();
            deleteName?.Invoke(Name);
            

            // Закрываем окно с успешным результатом
            //this.DialogResult = true;
            //this.Close();
        }

        // Метод проверки - возвращает bool, доступна ли команда
        private bool CanExecuteAddNomenclature ( object param )
        {
            // Проверяем, заполнены ли все поля
            return !string.IsNullOrWhiteSpace(DeleteTextBox?.Text);
        }
    }
}
