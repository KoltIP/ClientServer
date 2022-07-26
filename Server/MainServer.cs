﻿using System;
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
        
        //Основной метод сервера
        public void CreateServer()
        {
            Console.WriteLine("-----------*******Сервер запущен*******-----------");
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

        //Ввод данных вручную
        private void InputServerData()
        {
            Console.WriteLine("Введите : \n\t1)IP\n\t2)Номер порта прослушивания\n\t3)Каталог, в который сохранится принятый файл\nВнимание, данные вводятся через пробел в одну строку.");
            string input = Console.ReadLine();
            string[] mass = input.Split(" ");
            while (input == null || input == "" || mass.Length < 3)
            {
                Console.WriteLine("Неверный ввод или введено недостаточное количество параметров. Попробуйте повторить попытку.");
                input = Console.ReadLine();
            }
            try
            {
                adress = IPAddress.Parse(mass[0]);
                tcpPort = Int32.Parse(mass[1]);
                //Парсинг, в cлучае если путь к файлу содержит пробелы
                for (int i = 2; i <= mass.Length - 1; i++)
                    path += " " + mass[i];
                path = path.Substring(1, path.Length - 1);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }

        //Посылка пользователю сообщения об успешеном подключении
        private void SendServerTcp(TcpClient client, NetworkStream stream)
        {
            try
            {
                // сообщение для отправки клиенту
                string response = "Соединение установлено";
                // преобразуем сообщение в массив байтов
                byte[] data = Encoding.UTF8.GetBytes(response);
                // отправка сообщения
                stream.Write(data, 0, data.Length);
                Console.WriteLine("Отправлено сообщение: {0}", response);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }

        //Получении сообщения об имени файла и порте для Udp соединения
        private List<string> AcceptServerTcp(TcpClient client, NetworkStream stream)
        {
            // Переменные для чтения
            byte[] data = new byte[256];
            string response = string.Empty;
            StringBuilder readResponse = new StringBuilder();
            try
            {
                //Чтение данных
                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    readResponse.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable); // пока данные есть в потоке
            
                response = readResponse.ToString().Trim();
                Console.WriteLine("Получено сообщение:");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }


            List<string> responseList = new List<string>();
            try
            {
                fileName = response.Substring(0, response.LastIndexOf(" "));
                Console.WriteLine("\tНазвание файла: " + fileName);
                responseList.Add(fileName);
                udpPort = Int32.Parse(response.Substring(response.LastIndexOf(" ") + 1));
                Console.WriteLine("\tUDP порт: " + udpPort);
                responseList.Add(udpPort.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
            return responseList;
        }

        //Приём пакетов по Udp с отсылкой подтверждения по TCP
        private void AcceptServerUdp(UdpClient udpClient, NetworkStream stream, string path, string name)
        {

            IPEndPoint RemoteIpEndPoint = null;
            // Получаем файл
            byte[] bytes;
            List<byte[]> list = new List<byte[]>();
            try
            {
                udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 9999));
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
            while (true)
            {

                Console.WriteLine("Ожидается получение пакетов файла");
                try
                {
                    bytes = udpClient.Receive(ref RemoteIpEndPoint);
                    if (bytes.Length > 1)
                    {
                        int id = ParseByteInId(bytes);
                        Console.WriteLine($"Получен пакет с id  = {id}");
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
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", e.Message);
                }

                string flag = "111";
                try
                {
                    //получение сообщения о завершении
                    if (Encoding.ASCII.GetString(list[list.Count - 1]).ToString() == flag)
                    {
                        list.RemoveAt(list.Count - 1);
                        Console.WriteLine("Все пакеты получены. Приступаем к сборке.");
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", e.Message);
                }
            }
            //сохранение
            SaveDataInFile(list, path, name);
        }

        //Отделение ID пакета от содержащейся в нём информации
        private int ParseByteInId(byte[] bytes)
        {
            int id = -1;
            string str = String.Empty;
            try
            {
                str = Encoding.ASCII.GetString(bytes).ToString();
                if (str != "" && str != null && str.Length > 4)
                    id = Int32.Parse((str.Substring(str.Length - 4)).TrimEnd().TrimStart());
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
            return id;
        }

        //Сохранение файла с указанным именнем по указанному пути
        private void SaveDataInFile(List<byte[]> list, string path, string name)
        {
            try
            {
                string newPath = path + @"\" + name;
                using (FileStream fileStream = new FileStream(newPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    for (int i = 0; i < list.Count; i++)
                        fileStream.Write(list[i], 0, list[i].Length - 4);
                }
                Console.WriteLine($"Файл успешно собран. Вы можете ознакомиться с ним по пути:\n{newPath}");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }

    }
}
