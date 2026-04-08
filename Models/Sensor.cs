using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NewAPP
{
    public class Sensor : INotifyPropertyChanged
    {
        private int _cell;//ячейка
        private int _shelf; //стелаж
        private int _row; //ряд
        private string _name;
        private int _weight;
        private bool _isConnected;
        private string _lastError;

        private int _id;
        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        private int _sensorNumber;
        public int SensorNumber
        {
            get => _sensorNumber;
            set { _sensorNumber = value; OnPropertyChanged(); }
        }

        public int Cell
        {
            get => _cell;
            set { _cell = value; OnPropertyChanged(); }
        }

        public int Shelf
        {
            get => _shelf;
            set { _shelf = value; OnPropertyChanged(); }
        }

        public int Row  
        {
            get => _row;
            set { _row = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public int Weight
        {
            get => _weight;
            set { _weight = value; OnPropertyChanged(); }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        public string Location => $"{Row}-{Shelf}-{Cell}";

        private string _selectedNomenclature;
        public string SelectedNomenclature
        {
            get => _selectedNomenclature;
            set { _selectedNomenclature = value; 
                OnPropertyChanged(); UpdateUnitFromNomenclature();
            }
        }

        private string _selectedUnit;
        public string SelectedUnit
        {
            get => _selectedUnit;
            set { _selectedUnit = value; OnPropertyChanged(); }
        }

        public string LastError
        {
            get => _lastError;
            set { _lastError = value; OnPropertyChanged(); }
        }

        private void UpdateUnitFromNomenclature()
    {
        if (string.IsNullOrEmpty(SelectedNomenclature)) return;
        
        // Получаем доступ к данным MainViewModel
        var mainVm = App.Current.MainWindow?.DataContext as MainViewModel;
        var item = mainVm?.LoadlNum?.FirstOrDefault(x => x.Name == SelectedNomenclature);
        
        if (item != null)
        {
            SelectedUnit = item.Unit;  // "шт", "кг", "м" и т.д.
        }
    }

        public ushort RegisterAddress { get; set; }

        public int TerminalId { get; set; } //номер терминала к которому будет подключен датчик

        // Вычисляемые свойства для UI
        public string StatusText => IsConnected ? "Online" : "Offline";
        public string StatusColor => IsConnected ? "Green" : "Red";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged ( [CallerMemberName] string name = null )
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
