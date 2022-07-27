﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class MainClient
    {
        private const string serverAddress = "127.0.0.1"; //IP server
        IPAddress ipAddress = IPAddress.Parse(serverAddress);
        private const int tcpServerPort = 8888; //TCP port
        private const int udpServerPort = 9999;//UDP port
        TcpClient tcpClient;
        TcpListener tcpListener;
        NetworkStream stream;
        UdpClient udpClient;
        private string path = @"C:\Users\user\Desktop\Тестовые задания\Ратекс\Тестовое задание два языка.txt"; //Путь к файлу        
        private const int timeOut = 5000; //ТаймАут


        public void CreateClient()
        {
            //ввод данных вручную
            //InputData();
            try
            {
                //Создание и подключение
                tcpClient = new TcpClient();
                tcpListener = new TcpListener(ipAddress, 8888);

                tcpClient.Connect(serverAddress, tcpServerPort);
                stream = tcpClient.GetStream();

                AcceptClientTcp(tcpClient, stream);

                //Отправка имени файла и порт UDP
                SendClientTcp(tcpClient, stream);
                            
                udpClient = new UdpClient();

                SendClientFileUdp(udpClient, stream, tcpClient,path);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }

            Console.ReadLine();
        }

        //Метод отправки файла по частям с подтверждением по TCP 
        private void SendClientFileUdp(UdpClient udpClient, NetworkStream stream, TcpClient tcpClient, string path)
        {
            using (FileStream fs = new FileStream(path.ToString(), FileMode.Open, FileAccess.Read))
            {
                // определяем размер файла
                int parts = SizeOfFile(fs);

                Thread.Sleep(500);
                IPEndPoint endPoint = new IPEndPoint(ipAddress, udpServerPort);               


                for (int i = 0; i < parts; i++)
                {
                    //Отправка блока файла
                    SendBlockOfFile(fs, i, endPoint);

                    //получение подтверждения СТАРЫЙ ВАРИАНТ
                    AcceptConfirm(stream);
                }
                string message = "111";
                Console.WriteLine($"Все блоки отправлены");
                byte[] sendBytes = Encoding.Default.GetBytes(message); 
                              
                udpClient.Send(sendBytes, sendBytes.Length, endPoint);
                Console.WriteLine($"Отправлено сообщение о завершении сеанса");




                stream.Close();
                udpClient.Close();
                tcpClient.Close();

                Console.WriteLine("-----------*******Клиент отключен*******-----------");
            }
        }

        //Отправка блока файла
        private void SendBlockOfFile(FileStream fs, int i, IPEndPoint endPoint)
        {
            byte[] sendBytes = new Byte[8192];
            fs.Read(sendBytes, 0, sendBytes.Length-4);


            //добавление ID в блок 
            byte[] idBytes = Encoding.Default.GetBytes(i.ToString());
            for (int j = 0; j < idBytes.Length; j++)
            {
                sendBytes[sendBytes.Length - 4 + j] = idBytes[j];
            }
            Console.WriteLine($"Отправка блока файла номер {i + 1}");
            udpClient.Send(sendBytes, sendBytes.Length, endPoint);
            Console.WriteLine($"Блок {i + 1} отправлен");
        }

        //Старая версия
        private void AcceptConfirm(NetworkStream stream)
        {
            byte[] data = new byte[256];
            StringBuilder readResponse = new StringBuilder();
            //Чтение данных
            do
            {
                //Если время ожидания превышено повторная отправка файла                        

                //Чтение ответа
                int bytes = stream.Read(data, 0, data.Length);
                readResponse.Append(Encoding.UTF8.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable); // пока данные есть в потоке
            Console.WriteLine("Получено сообщение: " + readResponse.ToString());
        }

        //Определение количества блоков, которые нужно отправить
        private int SizeOfFile(FileStream fs)
        {
            int packetSize = 8192;
            int parts = (int)fs.Length / packetSize;
            if ((int)fs.Length % packetSize != 0)
                parts++;
            return parts;
        }

        //Ввод данных вручную
        private void InputClientData()
        {
            Console.WriteLine("Введите : \n\t1)IP\n\t2)Номер порта подключения к серверу\n\t3)Порт для отправки Udp\n\t4)Путь к файлу\n\t5)Порт для отправки Udp\nВнимание, данные вводятся через пробел в одну строку.");
            string input = Console.ReadLine();
            string[] mass = input.Split(" ");
            while (input == null || input == "" || mass.Length < 6 || mass.Length > 6)
            {
                Console.WriteLine("Неверный ввод. Попробуйте повторить попытку.");
                input = Console.ReadLine();
            }
            try
            {
                FileStream fs = new FileStream(input, FileMode.Open, FileAccess.Read);
                while (fs.Length >= 10485760)
                {
                    Console.WriteLine("Файл слишком велик. Попробуйте выбрать другой файл и повторить попытку ввода");
                    input = Console.ReadLine();
                }
                fs.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }

        //Получение ответа об успешном соединение по TCP
        private void AcceptClientTcp(TcpClient tcpClient, NetworkStream stream)
        {
            //Переменные для чтения
            byte[] data = new byte[256];
            StringBuilder readResponse = new StringBuilder();

            //Чтение данных
            do
            {
                int bytes = stream.Read(data, 0, data.Length);
                readResponse.Append(Encoding.UTF8.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable); // пока данные есть в потоке

            Console.WriteLine("Получено сообщение: " + readResponse.ToString());

        }

        //Отправка данных об порте и имени файла
        private void SendClientTcp(TcpClient tcpClient, NetworkStream stream)
        {
            string response = Path.GetFileName(path) + " " + udpServerPort;
            byte[] data = Encoding.UTF8.GetBytes(response);
            stream.Write(data, 0, data.Length);
            Console.WriteLine("Отправлено сообщение: " + response);
            
        }

    }
}
