using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace server
{
 
    public partial class ClientService
    {
        private TcpClient client;
        private NetworkStream clientStream;
        public delegate void SetTextCallback(string s);
        private string clientName;
        public List<ClientService> allClient;
        //private Form1 owner;

        public TcpClient connectedClient
        {
            get { return client; }
            set { client = value; }

        }

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ClientService()
        {
        }

        /// <summary>
        /// Constructor which accepts Client TCP object
        /// </summary>
        /// <param name="tcpClient"></param>
        public ClientService(string paramClientName,/*Form1 parent, */TcpClient tcpClient)
        {
            clientName = paramClientName;
            // Get Stream Object
            connectedClient = tcpClient;
            clientStream = tcpClient.GetStream();

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = connectedClient.Client;

            //Call Asynchronous Receive Function
            connectedClient.Client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(OnReceive), state);
        }

        #endregion

        /// <summary>
        /// This function is used to display data in Rich Text
        /// </summary>
        /// <param name="text"></param>
        private void SetText(string text)
        {
            foreach (var item in allClient)
            {
                var clientNameLocal = item.clientName;
                string input = clientNameLocal + ":" + text;
                var clientStramLocal = item.clientStream;
                clientStramLocal.Write(Encoding.ASCII.GetBytes(input), 0, input.Length);
                clientStramLocal.Flush();
            }
            Console.WriteLine(clientName + ":" + text);
        }

        #region Send/Receive Data From Scokets
        /// <summary>
        /// Function to Send Data to Client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void send_message(string sendMsg)
        {
            byte[] bt;
            bt = Encoding.ASCII.GetBytes(sendMsg);
            connectedClient.Client.Send(bt);
        }

        /// <summary>
        /// Asynchronous Callback function which receives data from Server
        /// </summary>
        /// <param name="ar"></param>
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

                        // Display Text in Rich Text Box
                        content = state.sb.ToString();
                        SetText(content);

                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(OnReceive), state);

                    }
                }

                catch (SocketException socketException)
                {
                    //WSAECONNRESET, the other side closed impolitely
                    if (socketException.ErrorCode == 10054 || ((socketException.ErrorCode != 10004) && (socketException.ErrorCode != 10053)))
                    {
                        // Complete the disconnect request.
                        String remoteIP = ((IPEndPoint)handler.RemoteEndPoint).Address.ToString();
                        String remotePort = ((IPEndPoint)handler.RemoteEndPoint).Port.ToString();
                        handler.Close();
                        handler = null;

                    }
                }

                // Eat up exception....Hmmmm I'm loving eat!!!
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }

        #endregion

   
    }


    #region StateObject Class Definition
    /// <summary>
    /// StateObject Class to read data from Client
    /// </summary>
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
    #endregion
}
