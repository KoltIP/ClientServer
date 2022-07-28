using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class MainServer
    {       
        IPAddress adress;
        int tcpPort; // Tcp порт, получаемый из ввода
        int udpPort; // Udp порт, получаемый из ввода
        string fileName = string.Empty; //имя файла, берётся из ввода
        string path = string.Empty; //Расположение файла , берётся из ввода 

        private void InputServerData()
        {
            Console.WriteLine("Введите : \n\t1)IP\n\t2)Номер порта прослушивания\n\t3)Каталог, в который сохранится принятый файл\nВнимание, данные вводятся через пробел в одну строку.");
            string input = Console.ReadLine();
            string[] mass = input.Split(" ");
            while (input == null || input == "" || mass.Length < 3 || mass.Length > 3)
            {
                Console.WriteLine("Неверный ввод. Попробуйте повторить попытку.");
                input = Console.ReadLine();
            }
            try
            {
                adress = IPAddress.Parse(mass[0]);
                tcpPort = Int32.Parse(mass[1]);
                path = mass[2];
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }

        //Отсылаем 
        private void SendServerTcp(TcpClient client, NetworkStream stream)
        {
            // сообщение для отправки клиенту
            string response = "Соединение установлено";
            // преобразуем сообщение в массив байтов
            byte[] data = Encoding.UTF8.GetBytes(response);
            // отправка сообщения
            stream.Write(data, 0, data.Length);
            Console.WriteLine("Отправлено сообщение: {0}", response);
            // закрываем поток
        }


        //Принимаем сообщение об имени файла
        private List<string> AcceptServerTcp(TcpClient client, NetworkStream stream)
        {
            // Переменные для чтения
            byte[] data = new byte[256];
            StringBuilder readResponse = new StringBuilder();
            //Чтение данных
            do
            {
                int bytes = stream.Read(data, 0, data.Length);
                readResponse.Append(Encoding.UTF8.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable); // пока данные есть в потоке

            string response = readResponse.ToString().Trim();
            Console.WriteLine("Получено сообщение:");

            List<string> responseList = new List<string>();
            fileName = response.Substring(0, response.LastIndexOf(" "));
            Console.WriteLine("\tНазвание файла: " + fileName);
            responseList.Add(fileName);
            udpPort = Int32.Parse(response.Substring(response.LastIndexOf(" ") + 1));
            Console.WriteLine("\tUDP порт: " + udpPort);
            responseList.Add(udpPort.ToString());
            return responseList;
        }

        //Отделение ID пакета от содержащейся в нём информации
        private int ParseByteInId(byte[] bytes)
        {
            int id = -1;
            string str = String.Empty;
            str = Encoding.ASCII.GetString(bytes).ToString();
            if (str != "" && str != null && str.Length > 4)
                id = Int32.Parse((str.Substring(str.Length - 4)).TrimEnd().TrimStart());
            return id;
        }

        //Приём пакетов по Udp с отсылкой подтверждения по TCP
        private void AcceptServerUdp(UdpClient udpClient, NetworkStream stream, string path, string name)
        {

            IPEndPoint RemoteIpEndPoint = null;
            // Получаем файл
            byte[] bytes;
            List<byte[]> list = new List<byte[]>();
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 9999));
            while (true)
            {

                Console.WriteLine("Ожидается получение файла");
                bytes = udpClient.Receive(ref RemoteIpEndPoint);
                if (bytes.Length > 1)
                {
                    int id = ParseByteInId(bytes);
                    Console.WriteLine($"Получен блок файла с id  = {id}");
                    list.Add(bytes);
                }

                //отправление подтверждения СТАРАЯ ВЕРСИЯ
                if (bytes.Length > 0)
                {
                    string AcceptResponse = "Пакет получен";
                    // преобразуем сообщение в массив байтов
                    byte[] acceptBytes = Encoding.UTF8.GetBytes(AcceptResponse);
                    // отправка сообщения
                    //stream.Write(acceptBytes, 0, acceptBytes.Length);
                    stream.Socket.Send(acceptBytes);
                    Console.WriteLine("Отправлено сообщение: {0}", AcceptResponse);
                    // закрываем поток
                }

                string flag = "111";
                //получение сообщения о завершении
                if (Encoding.ASCII.GetString(list[list.Count - 1]).ToString() == flag)
                {
                    list.RemoveAt(list.Count - 1);
                    Console.WriteLine("Все блоки получены. Приступаем к сборке.");
                    break;
                }
            }
            //сохранение
            SaveDataInFile(list, path, name);
        }

        private void SaveDataInFile(List<byte[]> list, string path, string name)
        {
            string newPath = path + @"\" + name;
            using (FileStream fileStream = new FileStream(newPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                for (int i = 0; i < list.Count; i++)
                    fileStream.Write(list[i], 0, list[i].Length - 4);
            }
            Console.WriteLine($"Файл успешно собран. Вы можете ознакомиться с ним по пути:\n{newPath}");
        }

        public void CreateServer()
        {
            //ввод данных вручную
            InputServerData();

            TcpListener server = new TcpListener(adress, tcpPort);
            try
            {
                server.Start();

                Console.WriteLine("Ожидание подключений... ");
                while (true)
                {
                    // получаем входящее подключение
                    TcpClient client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    Console.WriteLine("Подключен клиент. Выполнение запроса...");

                    //отсылаем оповещение о подключении
                    SendServerTcp(client, stream);

                    List<string> list = AcceptServerTcp(client, stream);
                    fileName = list[0];
                    udpPort = Int32.Parse(list[1]);
                    UdpClient udpClient = new UdpClient();
                    AcceptServerUdp(udpClient, stream, path, fileName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (server != null)
                    server.Stop();
            }
        }
    }
}
