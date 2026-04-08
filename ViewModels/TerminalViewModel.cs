using NewAPP.Services;
using OfficeOpenXml.Table.PivotTable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static OfficeOpenXml.ExcelErrorValue;

namespace NewAPP.ViewModels
{
    public class TerminalViewModel: INotifyPropertyChanged
    {
        private DatabaseService _dataBase;
        private ModbusTCP _modbusTCP;

        private ObservableCollection<Terminal> _terminals; //коллекция где будут храниться терминалы.
        public ObservableCollection<Terminal> Terminals
        {
            get => _terminals;
            set
            {
                _terminals = value;
                OnPropertyChanged(nameof(Terminals));
                OnPropertyChanged(nameof(TotalTerminal));
            }
        }
        private Terminal _selectedTerminal; //выбранный терминал
        public Terminal SelectedTerminal
        {
            get => _selectedTerminal;
            set
            {
                _selectedTerminal = value; OnPropertyChanged(nameof(SelectedTerminal));
                if(value != null)
                {
                    TerminalName = value.Name;
                    IpAddress = value.IpAddress;
                    Port = value.Port.ToString();
                    UnitId = value.UnitId.ToString();
                    SensorCount = value.SensorsCount;
                }
            }
        }
        //private ObservableCollection<Sensor> _sensor;
        //public ObservableCollection<Sensor> Sensors
        //{
        //    get => _sensor;
        //    set
        //    {
        //        _sensor = value;
        //        OnPropertyChanged(nameof(Sensor));

        //    }
        //}
        private string _terminalName;
        public string TerminalName 
        {
            get => _terminalName;
            set
            {
                _terminalName = value;
                OnPropertyChanged(nameof(TerminalName));
            }
        }
        public string _ipAddress;
        public string IpAddress
        {
            get => _ipAddress;
            set
            {
                _ipAddress = value;
                OnPropertyChanged(nameof(IpAddress));
            }
        }
        private string _port;
        public string Port
        {
            get => _port;
            set
            {
                _port = value;
                OnPropertyChanged(nameof(Port));
            }
        }
        private string _unitId;
        public string UnitId
        {
            get => _unitId;
            set
            {
                _unitId = value;
                OnPropertyChanged(nameof(UnitId));
            }
        }
        private int _sensorCount;
        public int SensorCount
        {
            get => _sensorCount;
                set
            {
                _sensorCount = value;
                OnPropertyChanged(nameof(SensorCount));
            }
        }
        //статистика
        public int TotalTerminal => Terminals?.Count ?? 0;
        public int ActivaTerminal => Terminals?.Count(t => t.IsConnected) ?? 0;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged ( [CallerMemberName] string prop = "" )
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        //команды
        public ICommand AddTerminalCommand { get; }//добавление терминала
        public ICommand EditTerminalCommand { get; }//редактирование терминала
        public ICommand DeleteTerminalCommand { get; } //удаление терминала
        public ICommand TestConnectionCommand { get; }// тестирование подключения
        public ICommand SelectTerminalCommand { get; }

        public TerminalViewModel( DatabaseService dataBase )
        {
            _dataBase = dataBase;
            _modbusTCP = new ModbusTCP();
            Terminals = new ObservableCollection<Terminal>(_dataBase.GetTerminals());

            //Terminals = new ObservableCollection<Terminal>();
            ////Sensors = new ObservableCollection<Sensor>();

            AddTerminalCommand = new RelayCommand(ExecuteAddTerminal);
            EditTerminalCommand = new RelayCommand(ExecuteEditTerminal);
            DeleteTerminalCommand = new RelayCommand(ExecuteDeleteTerminal);
            TestConnectionCommand = new RelayCommand(ExecuteTestConnection);
            SelectTerminalCommand = new RelayCommand(ExecuteSelectTerminal);
        }

        public void ExecuteAddTerminal(object param) //метод для добавления терминала
        {
            var terminal = new Terminal
            {
                Id = 1,
                Name = TerminalName,
                IpAddress = IpAddress,
                Port = int.Parse(Port),
                UnitId = byte.Parse(UnitId),
                Sensors = new ObservableCollection<Sensor>()
            };
            for (int i = 1; i <= 60; i++)
            {
                var sens = new Sensor
                {
                    SensorNumber = i,
                    Name = $"Датчик {i:D2}",
                    RegisterAddress = (ushort)(30 + (i - 1) * 2),
                    IsConnected = false,
                    TerminalId = terminal.Id
                };
                terminal.AddSensor(sens);
            }
            _dataBase.SaveTerminal(terminal);
            Terminals.Add(terminal);
            ClearForm();

            OnPropertyChanged(nameof(Terminals));
            OnPropertyChanged(nameof(TotalTerminal));
            OnPropertyChanged(nameof(ActivaTerminal));
        }
        private void ExecuteSelectTerminal ( object param )
        {
            if (param is Terminal terminal)
            {
                SelectedTerminal = terminal;
            }
        }

        public void ExecuteEditTerminal(object param) //редактировать терминал
        {
           if(SelectedTerminal != null)
            {
                SelectedTerminal.Name = TerminalName;
                SelectedTerminal.IpAddress = IpAddress;
                SelectedTerminal.Port = int.Parse(Port);
                SelectedTerminal.UnitId = byte.Parse(UnitId);

                _dataBase.UpdateTerminal(SelectedTerminal);
                RefreshTerminalsList();
                OnPropertyChanged(nameof(Terminals));
                SelectedTerminal = null;
                ClearForm();

            }
           
        }
        public void ExecuteDeleteTerminal(object param) //удалить терминал
        {
            if (SelectedTerminal != null)
            {
                // Запрашиваем подтверждение
                var result = MessageBox.Show($"Удалить терминал '{SelectedTerminal.Name}'?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Удаляем из базы данных
                    bool deleted = _dataBase.DeleteTerminal(SelectedTerminal.Id);

                    if (deleted)
                    {
                        // Удаляем из коллекции
                        Terminals.Remove(SelectedTerminal);

                        // Очищаем форму
                        ClearForm();

                        // Сбрасываем выбранный терминал
                        SelectedTerminal = null;

                        // Обновляем статистику
                        OnPropertyChanged(nameof(Terminals));
                        OnPropertyChanged(nameof(TotalTerminal));
                        OnPropertyChanged(nameof(ActivaTerminal));

                        MessageBox.Show("Терминал успешно удален!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при удалении терминала!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите терминал для удаления!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public async void ExecuteTestConnection(object param) //тестирование терминала
        {
           if(SelectedTerminal == null)
            {
                MessageBox.Show("Ничего не выбрано");
                return;
            }
            bool result = await _modbusTCP.TestConnectionAsync(SelectedTerminal.IpAddress, SelectedTerminal.Port);

            SelectedTerminal.IsConnected = result;

            if(result)
            {
                MessageBox.Show("Все путем чувак");
                return;
            }
            else
            {
                MessageBox.Show("Не все путем чувак");
            }
        }

        private void RefreshTerminalsList ( )
        {
            var temp = Terminals.ToList();
            Terminals.Clear();
            foreach (var t in temp)
            {
                Terminals.Add(t);
            }
        }
        private void ClearForm ( )
        {
            TerminalName = string.Empty;
            IpAddress = string.Empty;
            Port = "502";
            UnitId = "1";
            SensorCount = 10;
        }

    }
}
