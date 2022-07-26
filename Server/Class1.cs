using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Class1
    {
        public void waste()
        {/*
            private void AcceptServerUdp(string adress, int udpServerPort, TcpClient tcpClient, NetworkStream stream)
            {
                //Создание удалеённой точки
                IPEndPoint recieveEndPoint = null;
                //Сорздание клиента udp для получения файла от клиента
                UdpClient udpClient = new UdpClient(udpServerPort);

                //байты ответа
                byte[] packetSend = new byte[1];
                //байты получение
                byte[] packetRecieve;
                //ответом возвращается массив из одного значения(1)
                packetSend[0] = 1;
                //первое получение данных от клиента
                packetRecieve = udpClient.Receive(ref recieveEndPoint);



                IPEndPoint endPoint = new IPEndPoint(recieveEndPoint.Address, udpServerPort);
                //конвертация
                int parts = BitConverter.ToInt32(packetRecieve, 0);
                // отправка сообщения о получении количества блоков
                stream.Write(packetSend, 0, packetSend.Length);

                Console.WriteLine("Получено количество блоков");

                for (int i = 0; i < parts; i++)
                {
                    packetRecieve = udp.Receive(ref recieveEndPoint);
                    list.Add(packetRecieve);
                    stream.Write(packetSend, 0, packetSend.Length);
                }
                Console.WriteLine("Все блоки получены.");
                udpClient.Close();
                stream.Close();
                using (FileStream ws = new FileStream(Path.GetFileName(pathOfFile), FileMode.Create, FileAccess.Write))
                {
                    for (int i = 0; i < list.Count; i++)
                        ws.Write(packetRecieve, 0, packetRecieve.Length);
                }
                tcpClient.Close();
                Console.WriteLine("Соединение закрыто");
            }
            */
        }
    }
}
