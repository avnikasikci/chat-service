using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
namespace client
{
    public class ClientExample
    {

        public static void Main()
        {
            Console.WriteLine("start tcp Client sample");
            byte[] data = new byte[1024];
            string input;
            int port;
            TcpClient server;

            System.Console.WriteLine("Please Enter the port number of Server:\n");
            port = Int32.Parse(System.Console.ReadLine());
            try
            {
                server = new TcpClient("127.0.0.1", port);
            }
            catch (SocketException)
            {
                Console.WriteLine("Unable to connect to server");
                return;
            }
            Console.WriteLine("Connected to the Server...");
            Console.WriteLine("Enter the message to send it to the Sever");
            NetworkStream ns = server.GetStream();

            StateObject state = new StateObject();
            state.workSocket = server.Client;
            server.Client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback((new ClientExample()).OnReceive), state);

            while (true)
            {
                
                input = Console.ReadLine();                
                if (input == "exit")
                    break;
                ns.Write(Encoding.ASCII.GetBytes(input), 0, input.Length);
                ns.Flush();               
            }
            Console.WriteLine("Disconnecting from server...");
            ns.Close();
            server.Close();
        }


        public void OnReceive(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead;

            if (handler.Connected)
            {

                // Read data from the client socket. 
                try
                {
                    bytesRead = handler.EndReceive(ar);
                    if (bytesRead > 0)
                    {
                        // There  might be more data, so store the data received so far.
                        state.sb.Remove(0, state.sb.Length);
                        state.sb.Append(Encoding.ASCII.GetString(
                                         state.buffer, 0, bytesRead));

                        content = state.sb.ToString();
                        Console.WriteLine(content);

                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(OnReceive), state);

                    }
                }

                catch (SocketException socketException)
                {
                    //WSAECONNRESET, the other side closed impolitely
                    if (socketException.ErrorCode == 10054 || ((socketException.ErrorCode != 10004) && (socketException.ErrorCode != 10053)))
                    {
                        handler.Close();
                    }
                }
                // Eat up exception....Hmmmm I'm loving eat!!!
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }


    }

    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }
}
