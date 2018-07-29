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
    /// <summary>
    /// Socket services used by all the NodeMCU devices. This program dispatches the message to the appropriate
    /// device and obtains feedback as well. At this stage the feedback from NodeMCU is ignored.
    /// </summary>
    public sealed class SocketServices
    {
        //Check and remove all the variables
        private HostName RemoteAddress;
        private string RemotePort;
        private string RemoteMessage;
        private DatagramSocket HostSocket;

        /// <summary>
        /// Declare and Initialise a new DatagramSocket.
        /// </summary>
        public SocketServices()
        {
            HostSocket = new DatagramSocket();
            //HostSocket.MessageReceived += MessageReceived;

        }

        /// <summary>
        /// Handles Message received from the NodeMCU device. Currently not used. Left for future implementation
        /// </summary>
        /// <param name="sender">DatagramSocket</param>
        /// <param name="args">Contains received information, IP address and port</param>
        private void MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            DataReader reader = args.GetDataReader();
            uint len = reader.UnconsumedBufferLength;

            RemoteMessage = reader.ReadString(len);
            RemoteAddress = args.RemoteAddress;
            RemotePort = args.RemotePort;
            Debug.WriteLine("Remote Message Received from IP: {0} Port: {1} Message: {2}", RemoteAddress.ToString(), RemotePort, RemoteMessage);
        }

        /// <summary>
        /// Initialise the socket services with the right port and Host IP
        /// </summary>
        /// <param name="HostIP">IP address for the device</param>
        /// <param name="ServicePort">Port used by that device for communication</param>
        public void Initialise(string HostIP, string ServicePort)
        {
            RemoteAddress = new HostName(HostIP);
            RemotePort = ServicePort;
        }

        /// <summary>
        /// Only method used to send the appropriate message to the NodeMCU device that is configured with
        /// the IP and port which the device will be listening too.
        /// </summary>
        /// <param name="msg"></param>
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



    }
}
