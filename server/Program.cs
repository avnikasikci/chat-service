using System;

namespace server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server Socket Starting...");
            try
            {
                var socketService = new SocketService();
                socketService.StartListen("1453");
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }

        }

    }
}
