using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{/*
    internal class Class1
    {
        
        public void waste()
        {
            private void SendUdpFile(string path, string serverAddress, TcpClient tcpClient, NetworkStream stream)
            {
                UdpClient udpClient = new UdpClient();
                IPAddress ipAddr;
                try
                {
                    ipAddress = IPAddress.Parse(serverAddress);
                }
                catch (Exception e)
                { Console.WriteLine(e.ToString()); return; }
                try
                {
                    sendUdpEndPoint = new IPEndPoint(ipAddress, udpServerPort);
                    IPEndPoint recieveEndPoint = null;
                }
                catch (Exception e)
                { Console.WriteLine(e.ToString()); return; }

                try
                {
                    udpClient = new UdpClient(udpServerPort);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString()); return;
                }

                //работа с файлом
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    //Сколько нужно прочитать
                    int numBytesRead = (int)fs.Length;
                    //Сколько данных уже вычитано из файла
                    int numBytesReaded = 0;
                    string name = Path.GetFileName(path);
                    byte[] packetSend;
                    byte[] data = new byte[256];
                    int bytes;
                    StringBuilder readResponse = new StringBuilder();

                    //Вычисления количества частей
                    int parts = (int)fs.Length / packetSize;
                    if ((int)fs.Length % packetSize != 0)
                        parts++;
                    packetSend = BitConverter.GetBytes(parts);

                    //Отправили количество частей
                    try
                    {
                        udpClient.Send(packetSend, packetSend.Length, sendUdpEndPoint);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    //Подтверждение
                    //StringBuilder new_readResponse = new StringBuilder();
                    //data = new byte[256];                
                    //bytes = stream.Read(data, 0, data.Length);
                    //readResponse.Append(Encoding.UTF8.GetString(data, 0, bytes));

                    //цикл отправки
                    int n = 0;
                    packetSend = new byte[packetSize];
                    for (int i = 0; i < parts - 1; i++)
                    {
                        n = fs.Read(packetSend, 0, packetSize);
                        if (n == 0)
                            break;
                        numBytesReaded += n;
                        numBytesRead -= n;
                        udpClient.Send(packetSend, packetSend.Length, sendUdpEndPoint);
                        //Подтверждение
                        //packetRecieve = udpClient.Receive(ref recieveEndPoint);
                        //StringBuilder partResponse = new StringBuilder();
                        //data = new byte[256];
                        //bytes = stream.Read(data, 0, data.Length);
                        //partResponse.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    packetSend = new byte[numBytesRead];
                    n = fs.Read(packetSend, 0, numBytesRead);
                    udpClient.Send(packetSend, packetSend.Length, sendUdpEndPoint);
                    //Подтверждение
                    //packetRecieve = udpClient.Receive(ref recieveEndPoint);
                    //StringBuilder part_readResponse = new StringBuilder();
                    //data = new byte[256];
                    //int part_bytes = stream.Read(data, 0, data.Length);
                    //readResponse.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    //Console.WriteLine("Файл успешно отправлен");
                }
            }
        }
    }*/
}
