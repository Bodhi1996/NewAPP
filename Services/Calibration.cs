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
    }
}
