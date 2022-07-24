using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class MainClient
    {
        private const string serverAddress = "127.0.0.1"; //IP server
        private const int tcpServerPort = 8888; //TCP port
        private const int udpServerPort = 9999;//UDP port
        private string path = @"C:\Users\user\Desktop\Тестовые задания\Ратекс\Тестовое задание два языка.txt"; //Путь к файлу
        private const int timeOut = 500; //ТаймАут

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

            Console.WriteLine("Получено сообщение: "+readResponse.ToString());
            
        }

        private void SendClientTcp(TcpClient tcpClient, NetworkStream stream)
        {
            string response = path + " " + udpServerPort;
            byte[] data = Encoding.UTF8.GetBytes(response);          
            stream.Write(data, 0, data.Length);
            Console.WriteLine("Отправлено сообщение: " + response);
            // Закрываем потоки
            stream.Close();
        }

        //private void SendClientUdp()
        //{
        //    UdpClient sender = new UdpClient(); // создаем UdpClient для отправки сообщений
        //    try
        //    {
        //        while (true)
        //        {
        //            //отправка пакетов файла
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    finally
        //    {
        //        sender.Close();
        //    }
        //}

        //private void SendFile(UdpClient sender)
        //{
        //    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        //    // Создаем файловый поток и переводим его в байты
        //    Byte[] bytes = new Byte[fs.Length];
        //    fs.Read(bytes, 0, bytes.Length);

        //    Console.WriteLine("Отправка файла размером " + fs.Length + " байт");
        //    try
        //    {
        //        // Отправляем файл
        //        sender.Send(bytes, bytes.Length, endPoint);
        //    }
        //    catch (Exception eR)
        //    {
        //        Console.WriteLine(eR.ToString());
        //    }
        //    finally
        //    {
        //        // Закрываем соединение и очищаем поток
        //        fs.Close();
        //        sender.Close();
        //    }
        //    Console.WriteLine("Файл успешно отправлен.");
        //    Console.Read();
        //}

        private void InputClientData()
        {            
            Console.WriteLine("Введите : \n\t1)IP\n\t2)Номер порта подключения к серверу\n\t3)Порт для отправки Udp\n\t4)Путь к файлу\n\t5)Порт для отправки Udp\nВнимание, данные вводятся через пробел в одну строку.");
            string input = Console.ReadLine();
            string[] mass = input.Split(" ");
            while (input == null || input == "" ||  mass.Length < 6 || mass.Length > 6)
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
            catch(Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }

        public void CreateClient()
        {
            //InputData();
            try
            {
                //Создание и подключение
                TcpClient tcpClient = new TcpClient();
                tcpClient.Connect(serverAddress, tcpServerPort);
                NetworkStream stream = tcpClient.GetStream();

                AcceptClientTcp(tcpClient, stream);

                //Отправка имени файла и порт UDP
                SendClientTcp(tcpClient, stream);

                //SendClientUdp();
                
                tcpClient.Close();
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }

            Console.WriteLine("Подключение завершено...");
            Console.Read();
        }
        
    }
}
