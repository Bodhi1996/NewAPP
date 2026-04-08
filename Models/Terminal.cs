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
   public class Terminal:INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // НОВЫЕ СВОЙСТВА ДЛЯ ПОДКЛЮЧЕНИЯ:
        public string IpAddress { get; set; }
        public int Port { get; set; } = 5000;
        public byte UnitId { get; set; } = 1;
        public bool IsConnected { get; set;}
        //свойства для подклчюения терминала в основном окне
        private string _statusColor; //изменение цвета статуса
        public string StatusColor {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged("StatusColor"); }
        }

        private string _statusText; //изменение текста статуса
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged("StatusText"); }
        }
        private int _sensorCount; //кол-во датчиков.
        public int SensorsCount 
        {
            get => _sensorCount;
            set { _sensorCount = value; OnPropertyChanged("SensorCount"); }
        }

        public ObservableCollection<Sensor> Sensors { get; set; } = new ObservableCollection<Sensor>();

        public void AddSensor ( Sensor sensor )
        {// метод добавления
            if (Sensors.Count < 60)
            {
                Sensors.Add(sensor);
                OnPropertyChanged(nameof(Sensors));
            }
        }

        public void DeleteSensor(Sensor sensor)//метод удаления
        {
            Sensors.Remove(sensor);
            OnPropertyChanged(nameof(Sensors));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged ( [CallerMemberName] string prop = "" )
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

}
