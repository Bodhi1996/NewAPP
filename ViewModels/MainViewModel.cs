using NewAPP.Models;
using NewAPP.Services;
using NewAPP.View;
using NewAPP.ViewModels;
using OfficeOpenXml.FormulaParsing.Ranges;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Velopack;
using Velopack.Sources;


namespace NewAPP
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // ===== ПОЛЯ =====
        private DispatcherTimer _timer;
        private Random _random;
        private ModbusTCP _modbusService;
        private bool _useRealData;
        private Calibration _calibration;
        private DatabaseService _dataBase;
        // ===== КОЛЛЕКЦИИ =====
        public ObservableCollection<Terminal> Terminals { get; set; }
        public ObservableCollection<Sensor> VisibleSensors { get; set; }
        private List<NomenclatureUnit> allItems;
        public TerminalViewModel TerminalVM { get; set; }

        // ===== СВОЙСТВА ДЛЯ КОМБОБОКСОВ =====
        public List<int> AvailableCounts { get; } = new List<int> { 6, 12, 30, 60 };
        public List<int> AvailableColumns { get; } = new List<int> { 1, 2, 3, 6 };
        public List<DispayConfig> SensorConfig { get; } = new List<DispayConfig>()
        {
            new DispayConfig{Name = "4x4", Sensors = 16, Columns = 4 },
            new DispayConfig{Name = "6x4", Sensors = 24, Columns = 6 },
            new DispayConfig{Name = "8x4", Sensors = 32, Columns = 8 }
        };

        private DispayConfig _display;
        public DispayConfig Display
        {
            get => _display;
            set
            {
                if (_display == value) return;
                _display = value;
                OnPropertyChanged();
                if (value != null)
                {
                    SelectedColumns = value.Columns;
                    SelectedSensorsCount = value.Sensors;
                }
            }
        }

        private int _selectedSensorsCount = 4;
        public int SelectedSensorsCount
        {
            get => _selectedSensorsCount;
            set
            {
                _selectedSensorsCount = value;
                OnPropertyChanged();
                UpdateVisibleSensors();
            }
        }

        



        private int _selectedColumns = 4;
        public int SelectedColumns
        {
            get => _selectedColumns;
            set { _selectedColumns = value; OnPropertyChanged(); }
        }

        // ===== СВОЙСТВА СОСТОЯНИЯ =====
        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartButtonText));
                OnPropertyChanged(nameof(StartButtonColor));
            }
        }

        private string _statusText = "Готов к работе";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        private string _sensorsCountText = "0/60";
        public string SensorsCountText
        {
            get => _sensorsCountText;
            set { _sensorsCountText = value; OnPropertyChanged(); }
        }

        // ===== ВЫЧИСЛЯЕМЫЕ СВОЙСТВА ДЛЯ UI =====
        public string StartButtonText => IsRunning ? "⏸ ПАУЗА" : "▶ СТАРТ";
        public Brush StartButtonColor => IsRunning ? Brushes.Orange : Brushes.Green;

        public string ModeButtonText => _useRealData ? "📡 MODBUS" : "🎮 ТЕСТ";
        public Brush ModeButtonColor => _useRealData ? Brushes.DodgerBlue : Brushes.Orange;

        public string ModeText => _useRealData ? "MODBUS" : "ТЕСТОВЫЙ";
        public Brush ModeTextColor => _useRealData ? Brushes.DodgerBlue : Brushes.Orange;

        public string UserText => IsAdmin ? "Администратор" : "Пользователь";

        // ===== СВОЙСТВА ДЛЯ НАСТРОЕК =====
        private string _ipAddress;
        public string IpAddress
        {
            get => _ipAddress;
            set { _ipAddress = value; OnPropertyChanged(); }
        }

        private string _port ;
        public string Port
        {
            get => _port;
            set { _port = value; OnPropertyChanged(); }
        }

        private string _unitId;
        public string UnitId
        {
            get => _unitId;
            set { _unitId = value; OnPropertyChanged(); }
        }

        // ===== СВОЙСТВА ДЛЯ ВИДИМОСТИ ОКОН =====
        private bool _isWelcomeVisible = true;
        public bool IsWelcomeVisible
        {
            get => _isWelcomeVisible;
            set { _isWelcomeVisible = value; OnPropertyChanged(); }
        }

        private bool _isSensorsVisible;
        public bool IsSensorsVisible
        {
            get => _isSensorsVisible;
            set { _isSensorsVisible = value; OnPropertyChanged(); }
        }

        private bool _isSettingsVisible;
        public bool IsSettingsVisible
        {
            get => _isSettingsVisible;
            set { _isSettingsVisible = value; OnPropertyChanged(); }
        }

        private string _searchText; //свойство поиска
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value.Trim();
                OnPropertyChanged(); 

                
            }
        }

        private bool _isHotkeysVisible;
        public bool IsHotkeysVisible 
        {
            get => _isHotkeysVisible;
            set
            {
                _isHotkeysVisible = value; OnPropertyChanged();
            }
        }

        private bool _isTcpSettingsVisible;
        public bool IsTcpSettingsVisible
        {
            get => _isTcpSettingsVisible;
            set { _isTcpSettingsVisible = value; OnPropertyChanged(); }
        }

        private bool _isTopControlsVisible;
        public bool IsTopControlsVisible
        {
            get => _isTopControlsVisible;
            set { _isTopControlsVisible = value; OnPropertyChanged(); }
        }

        private bool _isRightCellsVisible;
        public bool IsRightCellsVisible
        {
            get => _isRightCellsVisible;
            set { _isRightCellsVisible = value; OnPropertyChanged(); }
        }
        private bool _isAddUsersVisible; // надо сделать отдельную видимость для кнопки, при этом должна исчезать видимость области за которую отвечает кнопка
        public bool IsAddUsersVisible
        {
            get => _isAddUsersVisible;
            set { _isAddUsersVisible = value; OnPropertyChanged(); }
        }
        private bool _executeShowCorrectVisible;
        public bool ExecuteShowCorrectVisible 
        {
            get => _executeShowCorrectVisible;
            set
            {
                _executeShowCorrectVisible = value;
                OnPropertyChanged();
            }
        }

        private string _isAddUsersLogin; //новый логин
        public string IsAddUsersLogin
        {
            get => _isAddUsersLogin;
            set { _isAddUsersLogin = value; OnPropertyChanged(); }
        }

        private string _isAddUsersPass; //новый пароль
        public string IsAddUsersPass
        {
            get => _isAddUsersPass;
            set { _isAddUsersPass = value; OnPropertyChanged(); }
        }

        private string _userRole = "Пользователь"; // новая роль
        public string UserRole
        {
            get => _userRole;
            set { _userRole = value; OnPropertyChanged(); }
        }
        private bool _isAddButtonUsersVisible; //видимость для кнопки
        public bool IsAddButtonUsersVisible
        {
            get => _isAddButtonUsersVisible;
            set { _isAddButtonUsersVisible = value; OnPropertyChanged(); }
        }

        private bool _isVisibleSklad; //видимость для кнопки склада
        public bool IsVisibleSklad
        {
            get => _isVisibleSklad;
            set { _isVisibleSklad = value; OnPropertyChanged(); }
        }

        private int _row;
        public int Row
        {
            get => _row;
            set
            {
                _row = value;
                OnPropertyChanged(); // ← Уведомляем UI об изменении
            }
        }

        private int _cell;
        public int Cell
        {
            get => _cell;
            set
            {
                _cell = value;
                OnPropertyChanged();
            }
        }

        private int _shelf;
        public int Shelf
        {
            get => _shelf;
            set
            {
                _shelf = value;
                OnPropertyChanged();
            }
        }
        private bool _isNomenclatureVisible; //свойство видимости номенклатуры
        public bool IsNomenclatureVisible 
        {
            get => _isNomenclatureVisible;
            set { _isNomenclatureVisible = value; OnPropertyChanged(); }
        }

        private bool _isDataBaseButtonVisible; //свойство видимости кнопки нмоенклатуры
        public bool IsDataBaseButtonVisible
        {
            get => _isDataBaseButtonVisible;
            set { _isDataBaseButtonVisible = value; OnPropertyChanged(); }
        }

        public List<string> AvailableRoles { get; } = new List<string> //список ролей
        { 
            "Пользователь",
            "Администратор"
        };

       private ObservableCollection<NomenclatureUnit> _nomenclatureItems; //свойство для NumGride.ItemsSource
        public ObservableCollection<NomenclatureUnit> NomenclatureItems
        {
            get => _nomenclatureItems;
            set { _nomenclatureItems = value; OnPropertyChanged(); }
        }

        private int _staticCount; //свойство для статики
        public int StaticCount
        {
            get => _staticCount;
            set
            {
                _staticCount = value;
                OnPropertyChanged();
            }
        }

        private string _name;
        public string Name //имя для добавления датчика
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(); // ← Уведомляем UI об изменении
            }
        }

        private string _unit;//ед.изм
        public string Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                OnPropertyChanged();
            }
        }

        private string _internalArticle; //внешний номер
        public string InternalArticle
        {
            get => _internalArticle;
            set
            {
                _internalArticle = value;
                OnPropertyChanged();
            }
        }

        private string _externalArticle; //внутренний номер
        public string ExternalArticle
        {
            get => _externalArticle;
            set
            {
                _externalArticle = value;
                OnPropertyChanged();
            }
        }

        private string _characteristic; //характеристика
        public string Characteristic
        {
            get => _characteristic;
            set
            {
                _characteristic = value;
                OnPropertyChanged();
            }
        }

        private string _serialNumber; //серийный номер
        public string SerialNumber
        {
            get => _serialNumber;
            set
            {
                _serialNumber = value;
                OnPropertyChanged();
            }
        }

        private string _adressCell; //адресс ячейки
        public string AdressCell
        {
            get => _adressCell;
            set
            {
                _adressCell = value;
                OnPropertyChanged();
            }
        }

        private int _oldQuantity; //начальное кол-во
        public int OldQuantity
        {
            get => _oldQuantity;
            set
            {
                _oldQuantity = value;
                OnPropertyChanged();
            }
        }

        private int _operationTypeIn; //добавили
        public int OperationTypeIn
        {
            get => _operationTypeIn;
            set
            {
                _operationTypeIn = value;
                OnPropertyChanged();
            }
        }

        private int _operationTypeOut; //убрали
        public int OperationTypeOut
        {
            get => _operationTypeOut;
            set
            {
                _operationTypeOut = value;
                OnPropertyChanged();
            }
        }

        private int _newQuantity; //остаток
        public int NewQuantity
        {
            get => _newQuantity;
            set
            {
                _newQuantity = value;
                OnPropertyChanged();
            }
        }

        private string _deleteName; //удалить имя
        public string DeleteName
        {
            get => _deleteName;
            set
            {
                _deleteName = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<NomenclatureUnit> _loadlNum;
        public ObservableCollection<NomenclatureUnit> LoadlNum
        {
            get => _loadlNum;
            set
            {
                _loadlNum = value;
                OnPropertyChanged();
            }
        }

        private string _terminalNumber; //добавлено свойство для ввода терминала
        public string TerminalNumber
        {
            get => _terminalNumber;
            set
            {
                _terminalNumber = value;
                OnPropertyChanged();
            }
        }

        private string _etalonWeight = "150"; //еталонный вес
        public string EtalonWeight
        {
            get => _etalonWeight;
            set
            {
                _etalonWeight = value; OnPropertyChanged();
            }
        }

        private int _unitPrice;
        public int UnitPrice  //свойство для цены за единицу
        {
            get => _unitPrice;
            set
            {
                _unitPrice = value;
                OnPropertyChanged();
            }
        }

        private int _weightUnit;
        public int WeightUnit //свойство веса за единицу
        {
            get => _weightUnit;
            set
            {
                _weightUnit = value; OnPropertyChanged();
            }
        }
        //свойства для поиска наименования для датчика
        private string _searchTextCell; //переменная для хранения текста
        private List<NomenclatureUnit> _allUnit;
        private ObservableCollection<NomenclatureUnit> _filterUnit; //коллекция для хранения результата поиска
        private NomenclatureUnit _selectedUnit; //свойство для выбранного элемента

        public string SearchTextCell 
        {
            get => _searchTextCell;
            set
            {
                _searchTextCell = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSuggestion));

                FilterMEthod();
            }
        }
        public NomenclatureUnit SelectedUnit 
        {
            get => _selectedUnit;
            set
            {
                _selectedUnit = value; OnPropertyChanged();
                OnPropertyChanged(nameof(HasSuggestion));

                if (value != null)
                {
                    SearchTextCell = value.Name;

                }
            }
        }

        public ObservableCollection<NomenclatureUnit> FilterUnit
        {
            get => _filterUnit;
            set
            {
                _filterUnit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSuggestion));
            }
        }

        public bool HasSuggestion => FilterUnit != null &&
            FilterUnit.Any() && !string.IsNullOrWhiteSpace(SearchTextCell);

        public static List<NomenclatureUnit> AllNomenclatureUnits { get; private set; }

        // ===== СВОЙСТВА ВИДИМОСТИ КОЛОНОК =====
        private Visibility _showId = Visibility.Visible;
        private Visibility _showName = Visibility.Visible;
        private Visibility _showInNumber = Visibility.Visible;
        private Visibility _showOutNumber = Visibility.Visible;
        private Visibility _showCharacteristick = Visibility.Visible;
        private Visibility _showUnit = Visibility.Visible;
        private Visibility _showAdr = Visibility.Visible;
        private Visibility _showQuantity = Visibility.Visible;
        private Visibility _showUnitPrice = Visibility.Visible;
        private Visibility _showWeightUnit { get; set; }

        public Visibility ShowWeightUnit 
        {
            get => _showWeightUnit;
            set
            {
                _showWeightUnit = value;
                OnPropertyChanged();
            }
        }

        public Visibility ShowId 
        {
            get => _showId;
            set
            {
                _showId = value;
                OnPropertyChanged();
            }
        }
        public Visibility ShowName
        {
            get => _showName;
            set
            {
                _showName = value;
                OnPropertyChanged();
            }
        }
        public Visibility ShowInNumber
        {
            get => _showInNumber;
            set
            {
                _showInNumber = value;
                OnPropertyChanged();
            }
        }
        public Visibility ShowOutNumber
        {
            get => _showOutNumber;
            set
            {
                _showOutNumber = value;
                OnPropertyChanged();
            }
        }
        public Visibility ShowCharacteristick
        {
            get => _showCharacteristick;
            set
            {
                _showCharacteristick = value;
                OnPropertyChanged();
            }
        }
        public Visibility ShowUnit
        {
            get => _showUnit;
            set
            {
                _showUnit = value;
                OnPropertyChanged();
            }
        }
        public Visibility ShowAdr
        {
            get => _showAdr;
            set
            {
                _showAdr = value;
                OnPropertyChanged();
            }
        }
        public Visibility ShowQuantity
        {
            get => _showQuantity;
            set
            {
                _showQuantity = value;
                OnPropertyChanged();
            }
        }
        public Visibility ShowUnitPrice
        {
            get => _showUnitPrice;
            set
            {
                _showUnitPrice = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ActionItem> _availableAction;
        public ObservableCollection<ActionItem> AvailableActions
        {
            get => _availableAction;
            set
            {
                _availableAction = value;
                OnPropertyChanged();
            }
        }

        private ActionItem _selectedAction;
        public ActionItem SelectedAction 
        {
            get => _selectedAction;
            set
            {
                _selectedAction = value; OnPropertyChanged();
            }
        }

        private ActionItem _hot1key;
        public ActionItem Hotkey1Action
        {
            get => _hot1key;
            set
            {
                _hot1key = value;
                OnPropertyChanged();

                if (_hot1key != null)
                {
                    ButtonContent2 = value.Name;
                    OpenComand2 = value.Command;
                }
            }
        }
        private ActionItem _hotkey2Action;
        public ActionItem Hotkey2Action 
        {
            get => _hotkey2Action;
            set
            {
                _hotkey2Action = value;
                OnPropertyChanged();
                if (_hotkey2Action != null)
                {
                    ButtonContent1 = value.Name;
                    OpenComand1 = value.Command;
                }
            }
        }

        private string _buttonContent1;
        public string ButtonContent1
        {
            get => _buttonContent1;
            set
            {
                _buttonContent1 = value; OnPropertyChanged();
            }
        }
        private ICommand _openComand1;
        public ICommand OpenComand1
        {
            get => _openComand1;
            set
            {
                _openComand1 = value; OnPropertyChanged();
            }
        }

        private string _buttonContent2;
        public string ButtonContent2
        {
            get => _buttonContent2;
            set
            {
                _buttonContent2 = value; OnPropertyChanged();
            }
        }
        private ICommand _openComand2;
        public ICommand OpenComand2 
        {
            get => _openComand2;
            set
            {
                _openComand2 = value; OnPropertyChanged();
            }
        }

        // ===== КОМАНДЫ =====
        public ICommand StartStopCommand { get; }
        public ICommand SwitchModeCommand { get; }
        public ICommand TestConnectionCommand { get; }
        public ICommand ShowSensorsCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand ShowNetworkCommand { get; }
        public ICommand ShowNomenclatureCommand { get; }
        public ICommand ShowBindCommand { get; }
        public ICommand ZeroPointCommand { get; }
        public ICommand EtalonPointCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand SetingSklad { get; } //кнопка открытия настроек склада
        public ICommand AddButtonUsers { get; private set; } // кнопка добавления пользователя
        public ICommand ButtonDataBase { get; } //кнопка открытия базы данных
        public ICommand AddNomenclatureButton { get; } // кнопка добавления номенклатуры
        public ICommand DeleteCommand { get; }
        public ICommand UpdateButtonNum { get; }
        public ICommand ExceleOtchet {  get; } //кнопка открытия отчета
        public ICommand DeleteNomCommand { get; } //команда убирания из номенклатуры чего то
        public ICommand SearchCommand { get; } //поиск по кнопку
        public ICommand OpenColumnSettingsCommand { get; } //команда для открытия окна настроек
        public ICommand CorrectQuantityCommand { get; } //команда для открытия окна корректировок остатков
        public ICommand IsCorrectButtonVisible { get; } //видимость кнопки коррекции остатков
        public ICommand ApplyHotkeysCommand { get; }
        public ICommand UpdateProgrammCommand { get; }
        public ICommand LoggerCommand {  get; }

        private Dictionary<Sensor, int> _lastValidWeight = new Dictionary<Sensor, int>();
        private Dictionary<Sensor, int> _consecutiveErrors = new Dictionary<Sensor, int>();
        private Dictionary<Sensor, int> _zeroCount = new Dictionary<Sensor, int>();

        // ===== КОНСТРУКТОР =====
        public MainViewModel ( )
        {
            try
            {
                // Инициализация сервисов
                _random = new Random();
                _modbusService = new ModbusTCP();
                _calibration = new Calibration();
                _dataBase = new DatabaseService(); // создание экземпляра базы данных

                Row = 1;
                Cell = 32;
                Shelf = 1;

                // Инициализация коллекций
                Terminals = new ObservableCollection<Terminal>();
                VisibleSensors = new ObservableCollection<Sensor>();

                // Инициализация таймера
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Tick += Timer_Tick;
                

                IsWelcomeVisible = true;
                IsSensorsVisible = false;
                IsSettingsVisible = false;
                IsTcpSettingsVisible = false;
                IsAddUsersVisible = false;
                IsNomenclatureVisible = false;
                IsTopControlsVisible = false;
                IsRightCellsVisible = false;

                TerminalVM = new TerminalViewModel(_dataBase); 
         TerminalVM.PropertyChanged += TerminalVM_PropertyChanged;

                TerminalVM.IpAddress = "192.168.0.56";
                TerminalVM.Port = "5000";
                TerminalVM.UnitId = "1";
                TerminalVM.TerminalName = "Терминал 1";

                // Загрузка данных
                InitializeTerminals();
                UpdateVisibleSensors();


                LoadlNum = new ObservableCollection<NomenclatureUnit>(_dataBase.AllNum());
                AllNomenclatureUnits = _dataBase.AllNum();
                AutoCheckConnectionOnStartup();

                // Инициализация команд
                StartStopCommand = new RelayCommand(ExecuteStartStop);
                SwitchModeCommand = new RelayCommand(ExecuteSwitchMode);
                TestConnectionCommand = new RelayCommand(ExecuteTestConnection);
                ShowSensorsCommand = new RelayCommand(ExecuteShowSensors);
                ShowSettingsCommand = new RelayCommand(ExecuteShowSettings);
                ShowNetworkCommand = new RelayCommand(ExecuteShowNetwork);
                //ShowNomenclatureCommand = new RelayCommand(ExecuteShowNomenclature);
                ShowBindCommand = new RelayCommand(ExecuteShowBind);
                ZeroPointCommand = new RelayCommand(ExecuteZeroPoint);
                EtalonPointCommand = new RelayCommand(ExecuteEtalonPoint);
                AddUserCommand = new RelayCommand(ExecuteAddUsers); //видимость
                AddButtonUsers = new RelayCommand(ExecuteAddUser); //добавление при помощи кнопки
                SetingSklad = new RelayCommand(ExecuteSetSklad); // кнопка открытия настроек склада
                ButtonDataBase = new RelayCommand(ExecuteDataBase); // кнопка открытия базы даных
                AddNomenclatureButton = new RelayCommand(ExecuteAddNomenclature); //
                DeleteCommand = new RelayCommand(ExecuteDeleteCommand);
                UpdateButtonNum = new RelayCommand(ExecuteUpdateButtonNum);
                ExceleOtchet = new RelayCommand(ExecuteExceleOtchet); //кнопка открытия отчета
                DeleteNomCommand = new RelayCommand(ExecuteDeleteUnitCommand);
                SearchCommand = new RelayCommand(ExecuteSearchCommand);
                OpenColumnSettingsCommand = new RelayCommand(OpenColumnSettings); //кнопка открытия
                CorrectQuantityCommand = new RelayCommand(ExecuteCorrectQuantity); //открытия окна корректировки остатков
                //IsCorrectButtonVisible = new RelayCommand(ExecuteShowCorrectVisible);
                ApplyHotkeysCommand = new RelayCommand(ExecuteApplyHotkeys);
                UpdateProgrammCommand = new RelayCommand(ExecuteProgrammCommand);
                LoggerCommand = new RelayCommand(ExecuteLoggerCommand);

                AvailableActions = new ObservableCollection<ActionItem>
                {
                    new ActionItem {Name = "добавить номенклатуру", Command = AddNomenclatureButton},
                    new ActionItem {Name = "убрать номенклатуру", Command = DeleteCommand},
                    new ActionItem {Name = "отчет", Command = ExceleOtchet},
                    new ActionItem {Name = "настройки склада", Command = SetingSklad}
                };
            }
            catch (Exception ex)
            {
                StatusText = $"Ошибка инициализации: {ex.Message}";
            }
        }

        

        // ===== МЕТОДЫ ИНИЦИАЛИЗАЦИИ =====
        private void InitializeTerminals ( )
        {
            string terminalIp = TerminalVM.IpAddress;
            string terminalPort = TerminalVM.Port;
            string terminalUnitId = TerminalVM.UnitId;
            string terminalName = TerminalVM.TerminalName;


            var terminal = new Terminal
            {
                Id = 1,
                Name = terminalName,
                IpAddress = terminalIp,
                Port = int.Parse(terminalPort),
                UnitId = byte.Parse(terminalUnitId)
            };

            for (int i = 1; i <= Cell; i++)
            {
                terminal.Sensors.Add(new Sensor
                {
                    Row = Row,
                    Shelf = Shelf,
                    Cell = i,
                    Name = "Адрес",
                    Weight = 20 + i,
                    RegisterAddress = (ushort)(30 + (i - 1) * 2),
                    IsConnected = false
                });
            }

            Terminals.Clear();
            Terminals.Add(terminal);
        }

        private void UpdateVisibleSensors ( )
        {
            if (Terminals.Count == 0) return;

            var allSensors = Terminals
                .SelectMany(t => t.Sensors)
                .Take(SelectedSensorsCount)
                .ToList();

            VisibleSensors.Clear();
            foreach (var sensor in allSensors)
                VisibleSensors.Add(sensor);

            SensorsCountText = $"{VisibleSensors.Count}/60";
        }

        // ===== КОМАНДЫ =====
        private void ExecuteStartStop ( object param )
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                StatusText = "Остановлено";
            }
            else
            {
                _timer.Start();
                StatusText = "Сбор данных...";
            }
            IsRunning = _timer.IsEnabled;
        }

        private void ExecuteSwitchMode ( object param )
        {
            _useRealData = !_useRealData;
            OnPropertyChanged(nameof(ModeButtonText));
            OnPropertyChanged(nameof(ModeButtonColor));
            OnPropertyChanged(nameof(ModeText));
            OnPropertyChanged(nameof(ModeTextColor));

            StatusText = _useRealData ? "Режим MODBUS (реальные данные)" : "Тестовый режим";
        }

        private async void ExecuteTestConnection ( object param )
        {
            if (Terminals.Count == 0) return;

            var terminal = Terminals[0];
            StatusText = "Тестирование подключения...";

            try
            {
                bool connected = await _modbusService.TestConnectionAsync(terminal.IpAddress, terminal.Port);
                StatusText = connected
                    ? $"✅ Подключено к {terminal.IpAddress}:{terminal.Port}"
                    : $"❌ Не удалось подключиться к {terminal.IpAddress}:{terminal.Port}";
            }
            catch (Exception ex)
            {
                StatusText = $"❌ Ошибка: {ex.Message}";
            }
        }

        private void ExecuteShowSensors ( object param )
        {
            // Обновляем видимость
            IsTopControlsVisible = true;//true
            IsSensorsVisible = true; //true
            IsWelcomeVisible = false;
            IsSettingsVisible = false;
            IsTcpSettingsVisible = false;
            IsRightCellsVisible = true;
            IsAddUsersVisible = false;
            IsNomenclatureVisible = false;
            IsHotkeysVisible = false;


            //VisibleAdminButton();
            UpdateVisibleSensors();
            StatusText = "Мониторинг датчиков";
        }
        public void ExecuteLoggerCommand(object param)
        {
            MessageBox.Show("Тут возможен журнал ошибок");
        }
        private void LoadAllNomenclature() //подгружает номенклатуру 
        {
            try
            {
                var all = _dataBase.AllNum();
                NomenclatureItems = new ObservableCollection<NomenclatureUnit>(all);
                StaticCount = NomenclatureItems?.Count ?? 0;
            }
            catch (Exception ex)
            {

                StatusText = $"Ошибка загрузки: {ex.Message}";
            }
            
        }

        private void ExecuteUpdateButtonNum( object param )
        {
            var all = _dataBase.AllNum();
            NomenclatureItems = new ObservableCollection<NomenclatureUnit>(all);
        }

        private void ExecuteSearchCommand (object param)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Если поиск пустой - показываем всю номенклатуру
                LoadAllNomenclature();
                StatusText = $"Всего записей: {NomenclatureItems?.Count ?? 0}";
                return;
            }

            try
            {
                var results = _dataBase.SearchUnit(SearchText);

                // ПРЕОБРАЗУЕМ NomenclatureItems в NomenclatureUnit для DataGrid
                var convertedResults = new ObservableCollection<NomenclatureUnit>();
                foreach (var item in results)
                {
                    convertedResults.Add(new NomenclatureUnit
                    {
                        Id = item.Id,
                        Name = item.Name,
                        InternalArticle = item.InternalArticle,
                        ExternalArticle = item.ExternalArticle,
                        Characteristic = item.Characteristic,
                        SerialNamber = item.SerialNamber,
                        Unit = item.Unit,
                        AddressCell = item.AddressCell,
                        OldQuantity = item.OldQuantity,
                        OperationTypeIn = item.OperationTypeIn,
                        OperationTypeOut = item.OperationTypeOut,
                        NewQuantity = item.NewQuantity,
                        UnitPrice = item.UnitPrice,
                        OperationDate = null
                    });
                }

                // Обновляем DataGrid результатами поиска
                NomenclatureItems = convertedResults;
                StatusText = $"Найдено записей: {results.Count}";

                if (results.Count == 0)
                {
                    MessageBox.Show("Ничего не найдено!", "Поиск",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Ошибка поиска: {ex.Message}";
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteExceleOtchet ( object param ) 
        {
            var all = _dataBase.AllNum();
            var reportWindow = new ReportWindow(all);
            reportWindow.Show();
        }

        private void ExecuteAddUsers (object param ) //область добавления пользователя
        {
            IsAddUsersVisible = true;//уберу пока
            IsWelcomeVisible = false;
            IsSensorsVisible = false;
            IsSettingsVisible = false; 
            IsTcpSettingsVisible = false;
            IsTopControlsVisible = false;
            IsNomenclatureVisible = false;
            IsRightCellsVisible = false;
            IsHotkeysVisible = false;

        }

        private async void ExecuteProgrammCommand ( object param )
        {
            try
            {
                // Создаём менеджер обновлений, указывая на GitHub репозиторий
                var updateManager = new UpdateManager(
                    new GithubSource(
                        repoUrl: "https://github.com/Bodhi1996/NewAPP",
                        accessToken: null,        // для публичного репозитория токен не нужен
                        prerelease: false
                    )
                );

                // Проверяем наличие новой версии
                var newVersion = await updateManager.CheckForUpdatesAsync();

                if (newVersion == null)
                {
                    MessageBox.Show("У Вас последняя версия", "Обновление",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Спрашиваем пользователя
                var result = MessageBox.Show(
                    $"Доступна новая версия {newVersion.TargetFullRelease.Version}!\n\n" +
                    $"Текущая версия: {updateManager.CurrentVersion?.ToString() ?? "Неизвестно"}\n\n" +
                    "Обновить сейчас?",
                    "Доступно обновление",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Показываем прогресс (опционально)
                    //StatusText = "Загрузка обновления...";

                    // Скачиваем обновление
                    await updateManager.DownloadUpdatesAsync(newVersion);

                    // Применяем и перезапускаем
                    updateManager.ApplyUpdatesAndRestart(newVersion);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке обновлений: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        
        }

        private void ExecuteShowSettings ( object param )
        {
            IsSettingsVisible = true;
            IsWelcomeVisible = false;
            IsSensorsVisible = false; //true
            IsTcpSettingsVisible = false;
            IsTopControlsVisible = false;
            IsRightCellsVisible = false;
            IsNomenclatureVisible = false;
            IsAddUsersVisible = false;
            IsHotkeysVisible = false;
            // VisibleAdminButton();

            StatusText = "Настройки калибровки";
        }

        private void ExecuteShowNetwork ( object param )
        {
   
            IsTcpSettingsVisible = true; //false
            IsTopControlsVisible = false;
            IsRightCellsVisible = false;
            IsNomenclatureVisible = false;
            IsAddUsersVisible = false;
            IsWelcomeVisible = false;
            IsSensorsVisible = false;
            IsSettingsVisible = false;
            IsHotkeysVisible = false;
            // VisibleAdminButton();

            StatusText = "Настройки сети";
        }

        private void ExecuteDeleteUnitCommand ( object param )
        {
            if (param is Sensor sensor)
            {
                // Теперь у нас есть доступ к конкретному датчику
                string selectedNomenclature = sensor.SelectedNomenclature;
                int weight = sensor.Weight;

                if (selectedNomenclature == null)
                {
                    MessageBox.Show("ничего не выбрано");
                }
                else
                {
                    var product = _dataBase.AllNum().FirstOrDefault(x => x.Name == selectedNomenclature);

                    _dataBase.DeleteUnit(selectedNomenclature, weight);

                    if (product != null)
                    {
                        _dataBase.SaveOperation(product.Id, "Списание", weight, CurrentUser?.Login);
                    }

                }


                // Здесь будет логика обновления базы данных
            }

        }

        private void ExecuteApplyHotkeys ( object param )
        {
            if (Hotkey1Action != null && Hotkey1Action.Command != null)
            {
                OpenComand1 = Hotkey1Action.Command;
                ButtonContent1 = Hotkey1Action.Name;
                //OpenComand2 = Hotkey2Action.Command;
                //ButtonContent2 = Hotkey2Action.Name;
            }
            else
            {
                // Если ничего не выбрано или команда отсутствует
                MessageBox.Show("Пожалуйста, выберите действие из списка");
            }

        }

        private void ExecuteSetSklad ( object param )  // настройки склада
        {
            var setSkladWindow = new SetSkladWindow();

            setSkladWindow.dataCon += OnDataReceived;
           
            ExecuteShowSensors(null);

            setSkladWindow.Show();
        }

        private void OnDataReceived ( (int val1, int val2, int val3) data ) //метод для передачи данных из окна склада
        {
                Row = data.val1;
                Cell = data.val2; 
                Shelf = data.val3;
            InitializeTerminals();
            UpdateVisibleSensors();
        }

        private async void ExecuteAddNomenclature ( object param ) //кнопка добавления номенклатуры
        {
            var addNomenclatureWindow = new AddNomenclatureWindow();
            addNomenclatureWindow.peredacha += OnDataRecivedAddNomenclature;
           
            addNomenclatureWindow.Show();

        }

        private void OnDataRecivedAddNomenclature((string val1, string val2, string val3, string val4, string val5, string val6, string val7, int val8, int val9, int val10, int val11, int val12, int val13)data)
        {
            Name = data.val1;
            InternalArticle = data.val2;
            ExternalArticle = data.val3;
            Characteristic = data.val4;
            SerialNumber = data.val5;
            Unit = data.val6;
            AdressCell = data.val7;
            OldQuantity = data.val8;
            OperationTypeIn = data.val9;
            OperationTypeOut = data.val10;
            NewQuantity = data.val11;
            UnitPrice = data.val12;
            WeightUnit = data.val13;
            _dataBase.AddNomenclature(Name, InternalArticle, ExternalArticle, Characteristic, SerialNumber, Unit, AdressCell, OldQuantity, OperationTypeIn, OperationTypeOut, NewQuantity, UnitPrice, WeightUnit);

            var all = _dataBase.AllNum();
            var addedProduct = all.FirstOrDefault(x => x.Name == Name);

            if(addedProduct != null)
            {
                _dataBase.SaveOperation(addedProduct.Id, "Добавление", NewQuantity, CurrentUser?.Login);
            }
            MessageBox.Show("Позиция добавлена");
        }

        private async void ExecuteDeleteCommand(object param) 
        {
            var deleteNomenclatute = new DeleteNomenclatureWindow();
            deleteNomenclatute.deleteName += DeleteNomenclatute_Action111;

            deleteNomenclatute.Show();
            
        }



        private void OpenColumnSettings ( object parameter ) //кнопка открытия визуальных настроек
        {
            var settingsWindow = new ColumnSettingsWindow();

            // Устанавливаем текущие значения чекбоксов
            settingsWindow.chkId.IsChecked = ShowId == Visibility.Visible;
            settingsWindow.chkName.IsChecked = ShowName == Visibility.Visible;
            settingsWindow.chkInNumber.IsChecked = ShowInNumber == Visibility.Visible;
            settingsWindow.chkOutNumber.IsChecked = ShowOutNumber == Visibility.Visible;
            settingsWindow.chkHaracteristick.IsChecked = ShowCharacteristick == Visibility.Visible;
            settingsWindow.chkUnit.IsChecked = ShowUnit == Visibility.Visible;
            settingsWindow.chkAdr.IsChecked = ShowAdr == Visibility.Visible;
            settingsWindow.chkQuntity.IsChecked = ShowQuantity == Visibility.Visible;
            settingsWindow.chkPrice.IsChecked = ShowUnitPrice == Visibility.Visible;
            settingsWindow.chkWeightUnit.IsChecked = ShowWeightUnit == Visibility.Visible;

            if (settingsWindow.ShowDialog() == true)
            {
                // Обновляем видимость колонок
                ShowId = settingsWindow.ShowId ? Visibility.Visible : Visibility.Collapsed;
                ShowName = settingsWindow.ShowName ? Visibility.Visible : Visibility.Collapsed;
                ShowInNumber = settingsWindow.ShowInNumber ? Visibility.Visible : Visibility.Collapsed;
                ShowOutNumber = settingsWindow.ShowOutNumber ? Visibility.Visible : Visibility.Collapsed;
                ShowCharacteristick = settingsWindow.ShowCharacteristick ? Visibility.Visible : Visibility.Collapsed;
                ShowUnit = settingsWindow.ShowUnit ? Visibility.Visible : Visibility.Collapsed;
                ShowAdr = settingsWindow.ShowAdr ? Visibility.Visible : Visibility.Collapsed;
                ShowQuantity = settingsWindow.ShowQuntity ? Visibility.Visible : Visibility.Collapsed;
                ShowUnitPrice = settingsWindow.ShowUnitPrice ? Visibility.Visible : Visibility.Collapsed;
                ShowWeightUnit = settingsWindow.ShowWeightUnit ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void DeleteNomenclatute_Action111 ( string name )
        {
            var product = _dataBase.AllNum().FirstOrDefault(x => x.Name == name);
            if (product != null)
            {
                _dataBase.SaveOperation(product.Id, "Удаление", int.Parse(product.NewQuantity), CurrentUser?.Login);
            }

            DeleteName = name;
            _dataBase.DeleteNomenclature(DeleteName);
            MessageBox.Show("Позиция удалена, обновите список");
        }

        private void ExecuteShowBind ( object param )
        {
            IsHotkeysVisible = true;
            IsTcpSettingsVisible = false; //false
            IsTopControlsVisible = false;
            IsRightCellsVisible = false;
            IsNomenclatureVisible = false;
            IsAddUsersVisible = false;
            IsWelcomeVisible = false;
            IsSensorsVisible = false;
            IsSettingsVisible = false;

        }


        private async void ExecuteZeroPoint ( object param )
        {
            try
            {
                // Получаем номер терминала из свойства
                if (!byte.TryParse(TerminalNumber, out byte terminalAddress))
                {
                    MessageBox.Show("Введите корректный номер терминала (1-255)", "Ошибка");
                    return;
                }
                //await _modbusService.TareAfterExternal(TerminalVM.IpAddress,int.Parse(TerminalVM.Port), byte.Parse(TerminalVM.UnitId));
                await _calibration.CalibZero(TerminalVM.IpAddress, int.Parse(TerminalVM.Port), terminalAddress);
                MessageBox.Show("Калибровка нуля выполнена успешно", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }


        private async void ExecuteAddUser ( object param ) //добавление пользователя
        {
            string role = UserRole;
            string dbRole = role switch
            {
                "Администратор" => "Admin",
                "Пользователь" => "User",
                _ => "User"
            }; 

            bool result = _dataBase.AddUser(IsAddUsersLogin, IsAddUsersPass, dbRole);

            if( result )
            {
                MessageBox.Show("Пользователь добавлен успешно");
                IsAddUsersLogin = "";
                IsAddUsersPass = "";
                
            }
            else
            {
                MessageBox.Show("Пользователь не добавлен");
            }
        }

        private async void ExecuteDataBase( object param ) //нажатие на кнопку базы данных
        {
            IsNomenclatureVisible = true;
            IsWelcomeVisible = false;
            IsSensorsVisible = false;
            IsSettingsVisible = false;
            IsTcpSettingsVisible = false; //false
            IsTopControlsVisible = false;
            IsRightCellsVisible = false;
            IsAddUsersVisible = false;
            IsHotkeysVisible = false;
            LoadAllNomenclature();
        }

        private async void ExecuteEtalonPoint ( object param )
        {
            var terminal = TerminalVM.SelectedTerminal ?? TerminalVM.Terminals.FirstOrDefault();
            try
            {
                //await _calibration.CalibWeight("192.168.0.56", 5000, 1, int.Parse(EtalonWeight));
                //await _calibration.CalibWeight("192.168.0.56", 5000, 1, int.Parse(EtalonWeight));
                await _calibration.CalibrationPoint(terminal.IpAddress, terminal.Port, terminal.UnitId, int.Parse(EtalonWeight));
                await _calibration.CalibrationPoint(terminal.IpAddress, terminal.Port, terminal.UnitId, int.Parse(EtalonWeight));

                MessageBox.Show("Вес успешно выставлен");
            }
            catch (Exception ex)
            {

                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // ===== ТАЙМЕР =====
        private async void Timer_Tick ( object sender, EventArgs e )
        {
            if (_useRealData)
            {
                await ReadRealDataAsync();
            }
            else
            {
                UpdateTestData();
            }

            UpdateConnectedCount();
        }

        private void UpdateTestData ( ) //расчет тестовых данных на датчик
        {
            foreach (var terminal in Terminals)
            {
                foreach (var sensor in terminal.Sensors)
                {
                    int change = _random.Next(-6, 6);
                    sensor.Weight = Math.Max(0, sensor.Weight + change);
                    sensor.IsConnected = true;
                }
            }
        }

        private async Task ReadRealDataAsync ( )
        {
            if (Terminals.Count == 0) return;
            var terminal = TerminalVM.SelectedTerminal ?? TerminalVM.Terminals.FirstOrDefault();

            // Проверяем, что терминал существует
            if (terminal == null)
            {
                System.Diagnostics.Debug.WriteLine("Нет доступных терминалов");
                return;
            }
            var sensors = VisibleSensors.ToList();

            // Для 32 датчиков лучше опрашивать НЕ все сразу, а с задержкой
            var results = new List<(Sensor sensor, int weight, bool connected)>();

            foreach (var sensor in sensors)
            {
                // Небольшая задержка между датчиками (5-10 мс)
                await Task.Delay(5);

                try
                {
                    int weightInGrams = await ReadWeightWithTimeout(sensor, terminal);

                    int weightInPieces = weightInGrams;
                    if (!string.IsNullOrEmpty(sensor.SelectedNomenclature))
                    {
                        var product = _dataBase.AllNum().FirstOrDefault(x => x.Name == sensor.SelectedNomenclature);
                        if (product != null && product.WeightUnit > 0)
                        {
                            weightInPieces = weightInGrams / product.WeightUnit;
                            sensor.WeightUnit = product.WeightUnit;
                        }
                    }

                    // Сохраняем последний корректный вес
                    _lastValidWeight[sensor] = weightInPieces;
                    _consecutiveErrors[sensor] = 0;

                    results.Add((sensor, weightInPieces, true));
                }
                catch (Exception ex)
                {
                    // При ошибке - используем последний известный вес
                    _consecutiveErrors[sensor] = _consecutiveErrors.GetValueOrDefault(sensor) + 1;

                    int fallbackWeight = _lastValidWeight.GetValueOrDefault(sensor, 0);

                    // Если ошибок больше 5 подряд - считаем датчик отключенным
                    bool isConnected = _consecutiveErrors[sensor] < 5;

                    results.Add((sensor, fallbackWeight, isConnected));

                    System.Diagnostics.Debug.WriteLine($"Датчик {sensor.Name}: ошибка #{_consecutiveErrors[sensor]}, вес={fallbackWeight}");
                }
            }

            // Обрабатываем результаты
            foreach (var (sensor, weight, connected) in results)
            {
                int previousWeight = sensor.Weight;

                // Игнорируем резкие скачки до 0
                if (weight == 0 && previousWeight > 0 && _zeroCount.GetValueOrDefault(sensor) < 3)
                {
                    _zeroCount[sensor] = _zeroCount.GetValueOrDefault(sensor) + 1;
                    continue;  // Пропускаем обновление, оставляем старый вес
                }
                else
                {
                    _zeroCount[sensor] = 0;
                }




                sensor.Weight = weight;
                sensor.IsConnected = connected;

                bool wasFirstRead = sensor.IsFirstRead;
                if (wasFirstRead)
                {
                    sensor.IsFirstRead = false;
                    continue;
                }

                // Проверяем изменение для обновления БД (только если датчик стабилен)
                if (sensor.IsConnected &&
                    !string.IsNullOrEmpty(sensor.SelectedNomenclature) &&
                    sensor.WeightUnit > 0 &&
                    _consecutiveErrors[sensor] == 0)  // ← Обновляем БД только при стабильном соединении
                {
                    int difference = sensor.Weight - previousWeight;

                    if (difference != 0)
                    {
                        int unitsChanged = Math.Abs(difference);

                        if (difference < 0)
                        {
                            _dataBase.DeleteUnit(sensor.SelectedNomenclature, unitsChanged);
                            var product = _dataBase.AllNum().FirstOrDefault(x => x.Name == sensor.SelectedNomenclature);
                            if (product != null)
                            {
                                _dataBase.SaveOperation(product.Id, "Списание", unitsChanged, CurrentUser?.Login ?? "System");
                            }
                        }
                        else if (difference > 0)
                        {
                            _dataBase.AddUnit(sensor.SelectedNomenclature, unitsChanged);
                            var product = _dataBase.AllNum().FirstOrDefault(x => x.Name == sensor.SelectedNomenclature);
                            if (product != null)
                            {
                                _dataBase.SaveOperation(product.Id, "Добавление", unitsChanged, CurrentUser?.Login ?? "System");
                            }
                        }
                    }
                }
            }
        }

        private async Task AutoCheckConnectionOnStartup ( )
        {
            try
            {
                // Получаем терминал из TerminalVM
                var terminal = TerminalVM?.SelectedTerminal ?? TerminalVM?.Terminals.FirstOrDefault();

                if (terminal == null)
                {
                    System.Diagnostics.Debug.WriteLine("Нет доступных терминалов для проверки");
                    StatusText = "Нет настроенных терминалов. Добавьте терминал в разделе 'Сеть'";
                    return;
                }

                // Проверяем корректность данных
                if (string.IsNullOrWhiteSpace(terminal.IpAddress))
                {
                    System.Diagnostics.Debug.WriteLine("IP адрес терминала не задан");
                    StatusText = "IP адрес терминала не задан. Настройте терминал в разделе 'Сеть'";
                    return;
                }



                if (terminal.Port <= 0 || terminal.Port > 65535)
                {
                    System.Diagnostics.Debug.WriteLine($"Некорректный порт: {terminal.Port}");
                    StatusText = $"Некорректный порт: {terminal.Port}. Проверьте настройки терминала";
                    return;
                }

                StatusText = $"Проверка подключения к терминалу {terminal.Name} ({terminal.IpAddress}:{terminal.Port})...";

                // Выполняем проверку подключения
                bool connected = await _modbusService.TestConnectionAsync(terminal.IpAddress, terminal.Port);

                // Обновляем статус терминала
                terminal.IsConnected = connected;
                terminal.StatusText = connected ? "Online" : "Offline";
                terminal.StatusColor = connected ? "Green" : "Red";

                // Обновляем статистику в TerminalVM
                TerminalVM.OnPropertyChanged(nameof(TerminalVM.ActivaTerminal));
                TerminalVM.OnPropertyChanged(nameof(TerminalVM.TotalTerminal));

                // Обновляем общий статус
                if (connected)
                {
                    StatusText = $"✅ Терминал {terminal.Name} подключен. Готов к работе.";
                    System.Diagnostics.Debug.WriteLine($"Автопроверка: терминал {terminal.IpAddress}:{terminal.Port} - ДОСТУПЕН");
                }
                else
                {
                    StatusText = $"❌ Терминал {terminal.Name} недоступен. Проверьте подключение.";
                    System.Diagnostics.Debug.WriteLine($"Автопроверка: терминал {terminal.IpAddress}:{terminal.Port} - НЕДОСТУПЕН");
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Ошибка при проверке подключения: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Ошибка автопроверки: {ex.Message}");
            }
        }


        // Вспомогательный метод с таймаутом
        private async Task<int> ReadWeightWithTimeout ( Sensor sensor, Terminal terminal, int timeoutMs = 200 )
        {
            var task = _modbusService.ReadWeightAsync(
                terminal.IpAddress,
                terminal.Port,
                terminal.UnitId,
                sensor.RegisterAddress);

            var timeoutTask = Task.Delay(timeoutMs);

            var completedTask = await Task.WhenAny(task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                throw new TimeoutException($"Датчик {sensor.Name} не ответил за {timeoutMs} мс");
            }

            return await task;
        }
        //тут ошибка появляется
        //private async Task ReadRealDataAsync ( )
        //{
        //    if (Terminals.Count == 0) return;

        //    var terminal = Terminals[0];

        //    foreach (var sensor in VisibleSensors) 
        //    {
        //        try
        //        {
        //            int weight = await _modbusService.ReadWeightAsync(
        //                terminal.IpAddress,
        //                terminal.Port,
        //                terminal.UnitId,
        //                sensor.RegisterAddress);

        //            sensor.Weight = weight;
        //            sensor.IsConnected = true;
        //        }
        //        catch
        //        {
        //            sensor.IsConnected = false;
        //        }
        //        await Task.Delay(10);
        //    }
        //}

        private void ExecuteCorrectQuantity (object param) 
        {
            var currectWindow = new CurrectQuantityWindow();
            currectWindow.ShowDialog();
            if(currectWindow.DialogResult == true)
            {
                string name = currectWindow.Name;
                int amount = currectWindow.Amount;
                _dataBase.CorrectUnit(name, amount);
            }
        }

        private void UpdateConnectedCount ( )
        {
            if (Terminals.Count == 0) return;

            int connectedCount = Terminals.Sum(t => t.Sensors.Count(s => s.IsConnected));
            StatusText = _timer.IsEnabled
                ? $"Сбор данных... ({connectedCount}/{VisibleSensors.Count} датчиков)"
                : StatusText;
        }

        // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =====


        // Очистка ресурсов
        public void Cleanup ( )
        {
            _timer?.Stop();
        }

        private void TerminalVM_PropertyChanged ( object sender, PropertyChangedEventArgs e )
        {
            // Когда коллекция терминалов изменилась
            if (e.PropertyName == nameof(TerminalVM.Terminals))
            {
                // Синхронизируем коллекции
                SynchronizeTerminals();
            }
            // Когда выбран другой терминал
            else if (e.PropertyName == nameof(TerminalVM.SelectedTerminal))
            {
                // Обновляем видимые датчики
                UpdateVisibleSensors();
            }
        }

        private void SynchronizeTerminals ( )
        {
            // Очищаем старую коллекцию
            Terminals.Clear();

            // Добавляем все терминалы из TerminalVM
            foreach (var terminal in TerminalVM.Terminals)
            {
                Terminals.Add(terminal);
            }

            // Обновляем отображение датчиков
            UpdateVisibleSensors();
        }

        // ===== INotifyPropertyChanged =====
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged ( [CallerMemberName] string name = null )
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        // Добавь в класс MainViewModel (в любое место, например после других полей)

        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAdmin));
                OnPropertyChanged(nameof(IsUser));
                OnPropertyChanged(nameof(UserInfo));
                OnPropertyChanged(nameof(WelcomeMessage));

                VisibleAdmin();
                VisibleAdminButton();
            }
        }

        private void FilterMEthod()
        {
            if (!string.IsNullOrWhiteSpace(SearchTextCell)) return;

            var result = _allUnit
                .Where(p => p.Name.ToLower().Contains(SearchText.ToLower()))
                .Take(10)
                .ToList();
            FilterUnit = new ObservableCollection<NomenclatureUnit>(result);
            OnPropertyChanged(nameof(HasSuggestion));
        }

        public bool IsAdmin => CurrentUser?.Role == "Admin";
        public bool IsUser => CurrentUser?.Role == "User";
        public string UserInfo => CurrentUser != null ? $"{CurrentUser.Login} ({CurrentUser.Role})" : "Не авторизован";
        public string WelcomeMessage => CurrentUser != null ? $"Добро пожаловать, {CurrentUser.Login}!" : "Добро пожаловать!";

        private void VisibleAdmin ( )
        {
            IsAddUsersVisible = IsAdmin;
            
            
        }
        private void VisibleAdminButton() // метод для видимости кнопки добавления и кнопки склада
        {
            IsAddButtonUsersVisible = IsAdmin;
            IsVisibleSklad = IsAdmin;
            IsDataBaseButtonVisible = IsAdmin;
            ExecuteShowCorrectVisible = IsAdmin;
        }

    }
}

