using System;
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
        private string serverAddress = string.Empty;//Ip сервера, получается из ввода
        IPAddress ipAddress = null;
        private int tcpServerPort; //Порт для Tcp подключения к серверу, получается из ввода
        private int udpServerPort; //Порт для Udp подключения, получается из ввода
        private string path = string.Empty; //Путь к файлу, получаемый из ввода
        private int timeOut; // Таймаут, получается из ввода
        private int sizeOfPacket = 8192; //Размер пакета в байтах

        
        //Основной метод клиента
        public void CreateClient()
        {
            //ввод данных вручную
            InputClientData();
            try
            {
                //Создание 
                TcpClient tcpClient = new TcpClient();
                TcpListener tcpListener = new TcpListener(ipAddress, tcpServerPort);

                //Подключение
                tcpClient.Connect(serverAddress, tcpServerPort);
                NetworkStream stream = tcpClient.GetStream();

                //Получение сообщения об успешном подключении по TCP от сервера
                AcceptClientTcp(stream);

                //Отправка имени файла и порт UDP на сервер
                SendClientTcp(stream);

                //Создание UdpClient -а для отправки блоков файла
                UdpClient udpClient = new UdpClient();

                //Отравка файла по Udp с подтверждением по TCP сокету 
                SendClientFileUdp(udpClient, stream, tcpClient, path);
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

        //Ввод данных вручную
        private void InputClientData()
        {
            Console.WriteLine("Введите : \n\t1)IP\n\t2)Номер порта подключения к серверу\n\t3)Порт для отправки Udp\n\t4)Путь к файлу\n\t5)Таймаут\nВнимание, данные вводятся через пробел в одну строку.");
            string input = Console.ReadLine();
            string[] mass = input.Split(" ");
            //Если ввод был пустым или было введено некорректное значение параметров
            while (input == null || input == "" || mass.Length < 4)
            {
                Console.WriteLine("Пустой ввод или введено недостаточное количество параметров. Попробуйте повторить попытку.");
                input = Console.ReadLine();
            }
            try
            {
                int length = mass.Length;
                //Ввод данных
                serverAddress = mass[0];
                ipAddress = IPAddress.Parse(serverAddress);
                tcpServerPort = Int32.Parse(mass[1]);
                udpServerPort = Int32.Parse(mass[2]);
                //Парсинг, в случае, если путь к файлу содержит пробелы
                for (int i = 3; i < mass.Length - 1; i++)
                    path += " " + mass[i];
                path = path.Substring(1, path.Length - 1);
                timeOut = Int32.Parse(mass[mass.Length - 1]);

                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    //По условию загружается файл размером до 10МБ
                    while (fs.Length >= 10485760)
                    {
                        Console.WriteLine("Файл слишком велик и не соответствует условию. Попробуйте выбрать другой файл и повторить попытку ввода");
                        input = Console.ReadLine();
                        length = mass.Length;
                        //Ввод данных
                        serverAddress = mass[0];
                        ipAddress = IPAddress.Parse(serverAddress);
                        tcpServerPort = Int32.Parse(mass[1]);
                        udpServerPort = Int32.Parse(mass[2]);
                        for (int i = 3; i < mass.Length - 1; i++)
                            path += " " + mass[i];
                        path = path.Substring(1, path.Length - 1);
                        timeOut = Int32.Parse(mass[mass.Length - 1]);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }

        //Получение ответа об успешном соединение по TCP
        private void AcceptClientTcp(NetworkStream stream)
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
        private void SendClientTcp(NetworkStream stream)
        {
            string response = Path.GetFileName(path) + " " + udpServerPort;
            byte[] data = Encoding.UTF8.GetBytes(response);
            stream.Write(data, 0, data.Length);
            Console.WriteLine("Отправлено сообщение: " + response);

        }

        //Метод отправки файла по частям с подтверждением по TCP 
        private void SendClientFileUdp(UdpClient udpClient, NetworkStream stream, TcpClient tcpClient, string path)
        {
            using (FileStream fs = new FileStream(path.ToString(), FileMode.Open, FileAccess.Read))
            {
                // определяем размер файла
                int parts = SizeOfFile(fs);

                IPEndPoint endPoint = new IPEndPoint(ipAddress, udpServerPort);

                //Отправка блока файла
                for (int i = 0; i < parts; i++)                
                    SendBlockOfFile(udpClient, stream, fs, i, endPoint);

                //Формирование сообщения об успешной отправке всех блоков
                string message = "111";
                Console.WriteLine($"Все блоки отправлены");
                byte[] sendBytes = Encoding.Default.GetBytes(message);

                //Отправка сообщения об успешной отправке всех блоков
                udpClient.Send(sendBytes, sendBytes.Length, endPoint);
                Console.WriteLine($"Отправлено сообщение о завершении сеанса");

                //Закрытие
                stream.Close();
                udpClient.Close();
                tcpClient.Close();

                Console.WriteLine("-----------*******Клиент отключен*******-----------");
            }
        }

        //Определение количества блоков, которые нужно отправить
        private int SizeOfFile(FileStream fs)
        {
            int parts = (int)fs.Length / sizeOfPacket;
            if ((int)fs.Length % sizeOfPacket != 0)
                parts++;
            return parts;
        }

        //Отправка блока файла
        private void SendBlockOfFile(UdpClient udpClient, NetworkStream stream,FileStream fs, int i, IPEndPoint endPoint)
        {
            //Формирование отправляемого пакета из файла с выделением 4-х позиций в байтовом массиве под ID пакета
            byte[] sendBytes = new Byte[sizeOfPacket];
            fs.Read(sendBytes, 0, sendBytes.Length - 4);


            //добавление ID в пакет 
            byte[] idBytes = Encoding.Default.GetBytes((i+1).ToString());
            for (int j = 0; j < idBytes.Length; j++)
            {
                sendBytes[sendBytes.Length - 4 + j] = idBytes[j];
            }

            //Отправка пакета
            Console.WriteLine($"Отправка блока файла номер {i + 1}");
            udpClient.Send(sendBytes, sendBytes.Length, endPoint);
            Console.WriteLine($"Блок {i + 1} отправлен");
            //Ожидание получения ответа по TCP Сокету
            AcceptConfirm(udpClient, stream, sendBytes, endPoint);
        }

        //Ожидание ответа от сервера на посылку пакета
        private void AcceptConfirm(UdpClient udpClient, NetworkStream stream, byte[] sendBytes, IPEndPoint endPoint)
        {
            //получаемый ответ 
            byte[] data = new byte[256];
            int bytes=0;
            StringBuilder readResponse = new StringBuilder();
            //Чтение данных
            do
            {
                //Если время ожидания превышено повторная отправка файла                        
                stream.Socket.ReceiveTimeout = timeOut;
                try
                {
                    //Получение ответа
                    bytes = stream.Socket.Receive(data);
                    readResponse.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                catch (Exception ex)
                {
                    //Повторная отправка, в случае превышения ожидания ответа
                    Console.WriteLine("Произошла ошибка при отправке, сервер не отправил ответ на получаемые данные." +
                        "\nПроизводится попытка повторной отправки");
                    udpClient.Send(sendBytes, sendBytes.Length, endPoint);                    
                }
            }
            while (stream.DataAvailable || bytes<=0); // пока данные есть в потоке или пока не прийдёт ответ
            Console.WriteLine("Получено сообщение: " + readResponse.ToString());
        }

    }
}
