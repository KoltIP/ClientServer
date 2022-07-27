using Client;
using Server;

namespace Work
{
    class Program
    {

        static void Main()
        {
            MainServer server = new MainServer();
            Thread serverThread = new Thread(server.CreateServer);
            serverThread.Start();

            
            MainClient client = new MainClient();
            Thread clientThread = new Thread(client.CreateClient);
            clientThread.Start();
        }

    }
}