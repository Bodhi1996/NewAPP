using NewAPP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public bool IsFirstRead = true;

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

        // ===== ПОИСК НОМЕНКЛАТУРЫ ДЛЯ ДАТЧИКА =====
        private string _searchText;
        private NomenclatureUnit _selectedUnitObj;
        private ObservableCollection<NomenclatureUnit> _filterUnit;

        public bool _isUpdateSurch;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_isUpdateSurch) return;
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSuggestions));
                FilterMethod();
            }
        }

        public NomenclatureUnit SelectedUnitObj
        {
            get => _selectedUnitObj;
            set
            {
                _selectedUnitObj = value;
                OnPropertyChanged();
                if (value != null)
                {
                    _isUpdateSurch = true;
                    SelectedNomenclature = value.Name;
                    SearchText = value.Name;
                    FilterUnit = new ObservableCollection<NomenclatureUnit>();
                    _isUpdateSurch = false;
                }
            }
        }

        public ObservableCollection<NomenclatureUnit> FilterUnit
        {
            get => _filterUnit ?? (_filterUnit = new ObservableCollection<NomenclatureUnit>());
            set
            {
                if (_filterUnit == value) return;
                _filterUnit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSuggestions));
            }
        }

        public bool HasSuggestions => SearchText != null && FilterUnit.Any() && !string.IsNullOrWhiteSpace(SearchText);

        public void FilterMethod ( )
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilterUnit = new ObservableCollection<NomenclatureUnit>();
                return;
            }
            var all = MainViewModel.AllNomenclatureUnits;
            if (all == null) return;
            var result = all
               .Where(p => p.Name != null && p.Name.ToLower().Contains(SearchText.ToLower()))
               .Take(10)
               .ToList();
            FilterUnit = new ObservableCollection<NomenclatureUnit>(result);

        }

        private int _weightUnit;
        public int WeightUnit 
        {
            get => _weightUnit;
            set
            {
                _weightUnit = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged ( [CallerMemberName] string name = null )
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
