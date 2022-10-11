using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace server
{
    public class SocketService
    {
        private TcpListener tcpServer;
        public TcpClient tcpClient;
        private Thread th;
        //public ChatDialog ctd;
        public List<ClientService> ctdList;
        private ArrayList formArray = new ArrayList();
        private ArrayList threadArray = new ArrayList();
        public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event ChangedEventHandler Changed;
        public delegate void SetListBoxItem(String str, String type);
        public SocketService()
        {
            // Add Event to handle when a client is connected
            Changed += new ChangedEventHandler(ClientAdded);

        }
        public void NewClient(Object obj)
        {
            ClientAdded(this, new MyEventArgs((TcpClient)obj));
        }

        public void ClientAdded(object sender, EventArgs e)
        {
            tcpClient = ((MyEventArgs)e).clientSock;
            String remoteIP = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
            String remotePort = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port.ToString();

            // Call Delegate Function to update client
            UpdateClientList(remoteIP + " : " + remotePort, "Add");


            // Show Dialog Box for Chatting
            var ctd = new ClientService("client_" + remotePort, tcpClient);
            if (ctdList == null || ctdList.Count <= 0)
                ctdList = new List<ClientService>();
            ctdList.Add(ctd);
            ctd.allClient = ctdList;
            formArray.Add(ctd);
            threadArray.Add(Thread.CurrentThread);
        }
        private void UpdateClientList(string str, string type)
        {
            Console.WriteLine("Update Client List remoteIp:" + str + "type:" + type);       
        }


        public void StartListen(string tbPortNumberTxt)
        {

            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            tcpServer = new TcpListener(localAddr, Int32.Parse(tbPortNumberTxt));
            tcpServer.Start();
            string input = "";

            // Keep on accepting Client Connection
            while (true)
            {
                // New Client connected, call Event to handle it.
                Thread t = new Thread(new ParameterizedThreadStart(NewClient));
                tcpClient = tcpServer.AcceptTcpClient();
                t.Start(tcpClient);
            }

        }
        public void StopServer()
        {

            if (tcpServer != null)
            {

                // Close all Socket connection
                foreach (ClientService c in formArray)
                {
                    c.connectedClient.Client.Close();
                }

                // Abort All Running Threads
                foreach (Thread t in threadArray)
                {
                    t.Abort();
                }

                // Clear all ArrayList
                threadArray.Clear();
                formArray.Clear();
                // Abort Listening Thread and Stop listening
                th.Abort();
                tcpServer.Stop();
            }
        }


    }

}
