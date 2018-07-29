using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace HomeAutomation
{
    public sealed class SocketServices
    {
        //Check and remove all the variables
        private HostName RemoteAddress;
        private string RemotePort;
        private string RemoteMessage;
        private DatagramSocket HostSocket;



        //If  you want to display message received to UI use this variable
        //public CoreDispatcher Dispatcher { get; private set; }


        public SocketServices()
        {
            HostSocket = new DatagramSocket();
            //HostSocket.MessageReceived += MessageReceived;

        }

        private void MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            DataReader reader = args.GetDataReader();
            uint len = reader.UnconsumedBufferLength;

            RemoteMessage = reader.ReadString(len);
            RemoteAddress = args.RemoteAddress;
            RemotePort = args.RemotePort;
            Debug.WriteLine("Remote Message Received from IP: {0} Port: {1} Message: {2}", RemoteAddress.ToString(), RemotePort, RemoteMessage);
        }

        public void Initialise(string HostIP, string ServicePort)
        {
            RemoteAddress = new HostName(HostIP);
            RemotePort = ServicePort;
        }

        public async void SendMessageToHost(string msg)
        {
            try
            {
                IOutputStream outputStream;
                outputStream = await HostSocket.GetOutputStreamAsync(RemoteAddress, RemotePort);

                using (DataWriter writer = new DataWriter(outputStream))
                {
                    writer.WriteString(msg);
                    await writer.StoreAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
           
        }

        /*
        public string RetrieveMessage()
        {
            string strReturn = RemoteMessage;
            if (strReturn.Length == 0)
                return "";
            else
            {
                RemoteMessage = "";
                return strReturn;
            }
        }*/

    }
}
