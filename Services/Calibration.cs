using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NewAPP.Services
{
    internal class Calibration
    {
        public async Task CalibZero(string ip, int port, byte terminalAddress)
        {
            await Task.Run(( ) =>
            {
                try
                {
                    using(TcpClient client = new TcpClient())
                    {
                        client.Connect(ip, port);
                        using (NetworkStream stream = client.GetStream())
                        {
                            byte[] remote = new byte[]
                            {
                                        0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x10,
                                        0x27, 0x23, 0x00, 0x01, 0x02, 0x00, 0x01
                            };
                            stream.Write(remote, 0, remote.Length);
                            stream.Read(new byte[64], 0, 64);

                            byte[] select = new byte[]
                                {
                                0x00, 0x02, 0x00, 0x00, 0x00, 0x06, 0x01, 0x10,
                                0x27, 0x12, 0x00, 0x01, 0x02, 0x00, terminalAddress
                                };
                            stream.Write(select, 0, select.Length);
                            stream.Read(new byte[64], 0, 64);

                            byte[] zero = new byte[]
                                {
                                0x00, 0x03, 0x00, 0x00, 0x00, 0x06, 0x01, 0x10,
                                0x27, 0x21, 0x00, 0x01, 0x02, 0x00, 0x01
                                };
                            stream.Write(zero, 0, zero.Length);
                            stream.Read(new byte[64], 0, 64);
                            
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    // Логируем детальную информацию
                    string errorMessage = $"Ошибка калибровки нуля: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $" Inner: {ex.InnerException.Message}";
                    }

                    // Пробрасываем с понятным сообщением
                    throw new Exception(errorMessage, ex);
                }
            });
        }
        public async Task CalibWeight ( string ip, int port, byte terminalAddress, int Weight)
        {
            await Task.Run(( ) =>
            {
                try
                {
                    using (TcpClient client = new TcpClient())
                    {
                        client.Connect(ip, port);
                        using (NetworkStream stream = client.GetStream())
                        {
                            byte[] remote = new byte[]
                            {
                                        0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x10,
                                        0x27, 0x23, 0x00, 0x01, 0x02, 0x00, 0x01
                            };
                            stream.Write(remote, 0, remote.Length);
                            stream.Read(new byte[64], 0, 64);

                            byte[] select = new byte[]
                                {
                                0x00, 0x02, 0x00, 0x00, 0x00, 0x06, 0x01, 0x10,
                                0x27, 0x12, 0x00, 0x01, 0x02, 0x00, terminalAddress
                                };
                            stream.Write(select, 0, select.Length);
                            stream.Read(new byte[64], 0, 64);

                            byte[] zero = new byte[]
                                {
                                0x00, 0x03, 0x00, 0x00, 0x00, 0x06, 0x01, 0x10,
                                0x27, 0x21, 0x00, 0x01, 0x02, 0x00, 0x01
                                };
                            stream.Write(zero, 0, zero.Length);
                            stream.Read(new byte[64], 0, 64);

                            byte[] weightBytes = BitConverter.GetBytes(Weight);
                            if (BitConverter.IsLittleEndian) Array.Reverse(weightBytes);

                            // ========== 5. Запись веса эталона в 10003 ==========
                            byte[] setWeight = new byte[]
                            {
                                0x00, 0x04, 0x00, 0x00, 0x00, 0x08, 0x01, 0x10,
                                0x27, 0x13, 0x00, 0x02, 0x04,
                                weightBytes[0], weightBytes[1], weightBytes[2], weightBytes[3]
                            };
                            stream.Write(setWeight, 0, setWeight.Length);
                            stream.Read(new byte[64], 0, 64);


                            // ========== 6. Команда калибровки количества ==========
                            byte[] calibrate = new byte[]
                            {
        0x00, 0x05, 0x00, 0x00, 0x00, 0x06, 0x01, 0x10,
        0x27, 0x21, 0x00, 0x01, 0x02, 0x00, 0x02
                            };
                            stream.Write(calibrate, 0, calibrate.Length);
                            stream.Read(new byte[64], 0, 64);


                        }

                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
            });
        }

        public async Task CalibrationPoint ( string ip, int port, byte terminalAddress, int Weight )
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                tcpClient.Connect(ip, port);
                using (NetworkStream stream = tcpClient.GetStream())
                {
                    // ========== 1. Включение внешнего управления ==========
                    byte[] remote = new byte[]
                    {
                0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x10,
                0x27, 0x23, 0x00, 0x01, 0x02, 0x00, 0x01
                    };
                    stream.Write(remote, 0, remote.Length);
                    stream.Read(new byte[64], 0, 64);


                    // ========== 2. Выбор терминала ==========
                    byte[] select = new byte[]
                    {
                0x00, 0x02, 0x00, 0x00, 0x00, 0x06, 0x01, 0x10,
                0x27, 0x12, 0x00, 0x01, 0x02, 0x00, terminalAddress
                    };
                    stream.Write(select, 0, select.Length);
                    stream.Read(new byte[64], 0, 64);


                    // ========== 3. Калибровка нуля (пустой лоток) ==========
                    

                    // ========== 4. Подготовка эталонного груза =========

                    // Подготовка веса в big-endian
                    byte[] weightBytes = BitConverter.GetBytes(Weight);
                    if (BitConverter.IsLittleEndian) Array.Reverse(weightBytes);
                    

                    // ========== 5. Запись веса эталона в 10003 ==========
                    byte[] setWeight = new byte[]
                    {
                0x00, 0x04, 0x00, 0x00, 0x00, 0x08, 0x01, 0x10,
                0x27, 0x13, 0x00, 0x02, 0x04,
                weightBytes[0], weightBytes[1], weightBytes[2], weightBytes[3]
                    };
                    stream.Write(setWeight, 0, setWeight.Length);
                    stream.Read(new byte[64], 0, 64);


                    // ========== 6. Команда калибровки количества ==========
                    byte[] calibrate = new byte[]
                    {
                0x00, 0x05, 0x00, 0x00, 0x00, 0x06, 0x01, 0x10,
                0x27, 0x21, 0x00, 0x01, 0x02, 0x00, 0x02
                    };
                    stream.Write(calibrate, 0, calibrate.Length);
                    stream.Read(new byte[64], 0, 64);


                    // ========== 7. Проверка веса ==========
                    byte[] readWeight = new byte[]
                    {
                0x00, 0x06, 0x00, 0x00, 0x00, 0x06, 0x01, 0x03,
                0x00, 0x1E, 0x00, 0x02
                    };
                    stream.Write(readWeight, 0, readWeight.Length);

                    byte[] resp = new byte[64];
                    int read = stream.Read(resp, 0, resp.Length);

                    if (read >= 13)
                    {
                        int weight = (resp[9] << 24) | (resp[10] << 16) | (resp[11] << 8) | resp[12];
                        
                    }
                }
            }
        }
    }
}
