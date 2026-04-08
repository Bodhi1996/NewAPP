using NewAPP.Models;
using NewAPP.Services;
using NewAPP.View;
using NewAPP.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;


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

        private int _selectedSensorsCount = 6;
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

        



        private int _selectedColumns = 2;
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
        private string _ipAddress = "192.168.0.56";
        public string IpAddress
        {
            get => _ipAddress;
            set { _ipAddress = value; OnPropertyChanged(); }
        }

        private string _port = "5000";
        public string Port
        {
            get => _port;
            set { _port = value; OnPropertyChanged(); }
        }

        private string _unitId = "1";
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
            set { _searchText = value; OnPropertyChanged(); }
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





        // ===== КОМАНДЫ =====
        public ICommand StartStopCommand { get; }
        public ICommand SwitchModeCommand { get; }
        public ICommand TestConnectionCommand { get; }
        public ICommand ShowSensorsCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand ShowNetworkCommand { get; }
        public ICommand ShowNomenclatureCommand { get; }
        public ICommand ShowHistoryCommand { get; }
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


                // Инициализация коллекций
                Terminals = new ObservableCollection<Terminal>();
                VisibleSensors = new ObservableCollection<Sensor>();

                // Инициализация таймера
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(0.1);
                _timer.Tick += Timer_Tick;
                

                IsWelcomeVisible = true;
                IsSensorsVisible = false;
                IsSettingsVisible = false;
                IsTcpSettingsVisible = false;
                IsAddUsersVisible = false;
                IsNomenclatureVisible = false;
                IsTopControlsVisible = false;
                IsRightCellsVisible = false;



                // Загрузка данных
                InitializeTerminals();
                UpdateVisibleSensors();
                TerminalVM = new TerminalViewModel(_dataBase);
                TerminalVM.PropertyChanged += TerminalVM_PropertyChanged;

                LoadlNum = new ObservableCollection<NomenclatureUnit>(_dataBase.AllNum());
                // Инициализация команд
                StartStopCommand = new RelayCommand(ExecuteStartStop);
                SwitchModeCommand = new RelayCommand(ExecuteSwitchMode);
                TestConnectionCommand = new RelayCommand(ExecuteTestConnection);
                ShowSensorsCommand = new RelayCommand(ExecuteShowSensors);
                ShowSettingsCommand = new RelayCommand(ExecuteShowSettings);
                ShowNetworkCommand = new RelayCommand(ExecuteShowNetwork);
                //ShowNomenclatureCommand = new RelayCommand(ExecuteShowNomenclature);
                ShowHistoryCommand = new RelayCommand(ExecuteShowHistory);
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
            }
            catch (Exception ex)
            {
                StatusText = $"Ошибка инициализации: {ex.Message}";
            }
        }


        // ===== МЕТОДЫ ИНИЦИАЛИЗАЦИИ =====
        private void InitializeTerminals ( )
        {
            var terminal = new Terminal
            {
                Id = 1,
                Name = "Терминал MODBUS",
                IpAddress = IpAddress,
                Port = int.Parse(Port),
                UnitId = byte.Parse(UnitId)
            };

            for (int i = 1; i <= 60; i++)
            {
                terminal.Sensors.Add(new Sensor
                {
                    Row = Row,
                    Shelf = Shelf,
                    Cell = Cell,
                    Name = $"Датчик {i:D2}",
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

            //VisibleAdminButton();
            UpdateVisibleSensors();
            StatusText = "Мониторинг датчиков";
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
                var results = _dataBase.SearchNomenclature(SearchText);

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
            

                if(reportWindow.ShowDialog()== true)
                {
                    
                }
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

                _dataBase.DeleteUnit(selectedNomenclature, weight);

                // Здесь будет логика обновления базы данных
            }

        }

        private void ExecuteSetSklad ( object param )  // настройки склада
        {
            var setSkladWindow = new SetSkladWindow();

            if(setSkladWindow.ShowDialog() == true)
            {
                Row = Int32.Parse(setSkladWindow.Row);
                Shelf = Int32.Parse(setSkladWindow.Shelf);
                Cell = Int32.Parse(setSkladWindow.Cell);

                InitializeTerminals(); // ← Переинициализация с новым Row
                UpdateVisibleSensors(); // ← Обновляем отображение

                // 3. Показываем датчики (если нужно)
                ExecuteShowSensors(null);
            }
        }

        private async void ExecuteAddNomenclature ( object param ) //кнопка добавления номенклатуры
        {
            var addNomenclatureWindow = new AddNomenclatureWindow();

            if(addNomenclatureWindow.ShowDialog() == true)
            {
                Name = addNomenclatureWindow.Name;
                InternalArticle = addNomenclatureWindow.InternalArticle;
                ExternalArticle = addNomenclatureWindow.ExternalArticle;
                Characteristic = addNomenclatureWindow.Characteristic;
                SerialNumber = addNomenclatureWindow.SerialNumber;
                Unit = addNomenclatureWindow.Unit;
                AdressCell = addNomenclatureWindow.AdressCell;
                OldQuantity = addNomenclatureWindow.OldQuantity;
                OperationTypeIn = addNomenclatureWindow.OperationTypeIn;
                OperationTypeOut = addNomenclatureWindow.OperationTypeOut;
                NewQuantity = addNomenclatureWindow.NewQuantity;

                _dataBase.AddNomenclature(Name, InternalArticle, ExternalArticle, Characteristic, SerialNumber, Unit, AdressCell, OldQuantity, OperationTypeIn, OperationTypeOut, NewQuantity);
            }
        }

        private async void ExecuteDeleteCommand(object param) 
        {
            var deleteNomenclatute = new DeleteNomenclatureWindow();
            if (deleteNomenclatute.ShowDialog() == true)
            {
                DeleteName = deleteNomenclatute.Name;
                _dataBase.DeleteNomenclature(DeleteName);
            }
        }

        private void ExecuteShowHistory ( object param )
        {
            StatusText = "История";
            //MessageBox.Show("История измерений и событий", "История");
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
                await _calibration.CalibZero("192.168.0.56", 5000, terminalAddress);
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
            LoadAllNomenclature();
        }

        private async void ExecuteEtalonPoint ( object param )
        {
            try
            {
                await _calibration.CalibWeight("192.168.0.56", 5000, 1, int.Parse(EtalonWeight));
                await _calibration.CalibWeight("192.168.0.56", 5000, 1, int.Parse(EtalonWeight));
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
            var terminal = Terminals[0];

            // Получаем список видимых датчиков
            var sensors = VisibleSensors.ToList();

            // Создаем задачи для параллельного опроса
            var tasks = sensors.Select(async sensor =>
            {
                try
                {
                    int weight = await _modbusService.ReadWeightAsync(
                        terminal.IpAddress,
                        terminal.Port,
                        terminal.UnitId,
                        sensor.RegisterAddress);
                    return (sensor, weight, connected: true);
                }
                catch
                {
                    return (sensor, weight: 0, connected: false);
                }
            });

            // Ждем завершения всех задач
            var results = await Task.WhenAll(tasks);

            // Обновляем результаты
            foreach (var (sensor, weight, connected) in results)
            {
                sensor.Weight = weight;
                sensor.IsConnected = connected;
            }
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
        }

    }
}

