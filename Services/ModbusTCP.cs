using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NewAPP
{
    class ModbusTCP
    {

        public async Task<int> ReadWeightAsync ( string ip, int port, byte unitId, ushort registerAddress )
        {
            return await Task.Run(( ) =>
            {
                try
                {
                    using (TcpClient tcpClient = new TcpClient())
                    {
                        // Подключаемся с таймаутом 3 секунды
                        tcpClient.ReceiveTimeout = 3000;
                        tcpClient.SendTimeout = 3000;

                        // Пробуем подключиться
                        var result = tcpClient.BeginConnect(ip, port, null, null);
                        bool connected = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

                        if (!connected || !tcpClient.Connected)
                        {
                            throw new Exception($"Не удалось подключиться к {ip}:{port}");
                        }

                        tcpClient.EndConnect(result);

                        NetworkStream stream = tcpClient.GetStream();
                        stream.ReadTimeout = 3000;

                        // MODBUS запрос (функция 03 - чтение регистров)
                        byte[] request = new byte[12];

                        // Заголовок MODBUS TCP
                        request[0] = 0x00; // Transaction ID
                        request[1] = 0x01;
                        request[2] = 0x00; // Protocol ID
                        request[3] = 0x00;
                        request[4] = 0x00; // Length
                        request[5] = 0x06;

                        // Данные MODBUS
                        request[6] = unitId;        // Адрес устройства
                        request[7] = 0x03;          // Функция: чтение регистров
                        request[8] = (byte)(registerAddress >> 8);    // Адрес регистра (старший байт)
                        request[9] = (byte)(registerAddress & 0xFF);  // Адрес регистра (младший байт)
                        request[10] = 0x00;         // Количество регистров (старший байт)
                        request[11] = 0x02;         // Количество регистров (младший байт) = 2 регистра = 4 байта

                        // Отправляем запрос
                        stream.Write(request, 0, request.Length);

                        // Читаем ответ
                        byte[] header = new byte[7];
                        int bytesRead = stream.Read(header, 0, 7);

                        //if (bytesRead < 7) throw new Exception("Неполный ответ");

                        // Длина данных
                        int dataLength = header[4] << 8 | header[5];

                        // Читаем данные
                        byte[] data = new byte[dataLength];
                        bytesRead = stream.Read(data, 0, dataLength);

                        // Проверяем функцию
                        if (data[0] != 0x03) throw new Exception($"Ошибка функции: {data[0]}");

                        // Проверяем количество байт
                        if (data[1] != 0x04) throw new Exception($"Ошибка длины: {data[1]}");

                        // Парсим 32-битное значение (4 байта)
                        int rawValue = (int)((data[2] << 24) | (data[3] << 16) | (data[4] << 8) | data[5]);

                        // Преобразуем в килограммы (предполагаем что устройство отдает в граммах)
                        int weight = rawValue;

                        stream.Close();
                        return weight;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"MODBUS ошибка: {ex.Message}");
                }
            });
        }

        // Метод для проверки подключения
        public async Task<bool> TestConnectionAsync ( string ip, int port )
        {
            try
            {
                using (TcpClient tcpClient = new TcpClient())
                {
                    var connectTask = tcpClient.ConnectAsync(ip, port);
                    var timeoutTask = Task.Delay(3000);

                    var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                    return completedTask != timeoutTask && tcpClient.Connected;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}

