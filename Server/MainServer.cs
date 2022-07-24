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
        TcpListener server = null;        
        IPAddress adress = IPAddress.Parse("127.0.0.1");//IP server
        int tcpPort = 8888;//TCP port server
        int udpPort = 9999;//Udp port server
        string pathOfFile = string.Empty; //расположение файла, берётся из ввода
        string nameOfFile = string.Empty; //название файла, получается от клиента

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
                pathOfFile = mass[2];
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }

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

        

        private void AcceptServerTcp(TcpClient client, NetworkStream stream)
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

            
            pathOfFile = response.Substring(0,response.LastIndexOf(" "));
            Console.WriteLine("\tНазвание файла: " + pathOfFile);
            udpPort = Int32.Parse(response.Substring(response.LastIndexOf(" ") + 1));
            Console.WriteLine("\tUDP порт: " + udpPort);
            stream.Close();
        }


        //private void AcceptServerUdp()
        //{
        //    UdpClient receiver = new UdpClient(); // UdpClient для получения данных
        //    IPEndPoint remoteIp = null; // адрес входящего подключения
        //    try
        //    {
        //        while (true)
        //        {
        //            byte[] data = receiver.Receive(ref remoteIp); // получаем данные
        //            string message = Encoding.Unicode.GetString(data);
        //            Console.WriteLine("Собеседник: {0}", message);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    finally
        //    {
        //        receiver.Close();
        //    }
        //}

        public void CreateServer()
        {
            server = new TcpListener(adress, tcpPort);
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

                    AcceptServerTcp(client, stream);
                    

                    
                    //// закрываем подключение
                    //client.Close();
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
