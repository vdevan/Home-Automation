using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation
{
    /// <summary>
    /// This class does not contain much other than storing Monitoring status and sending the same to the 
    /// monitoring devices. In future this can be used to store photos of the intruder or streaming videos.
    /// </summary>
    class IntruderDetect
    {
        private SocketServices SSI;
        public string strAlive { get; set; }
        public bool bStartMonitor { get; set; }

        /// <summary>
        /// Store the socket required for communication. Initially monitoring alert will be OFF
        /// </summary>
        public IntruderDetect()
        {
            SSI = new SocketServices();
            bStartMonitor = false;
        }

        /// <summary>
        /// Initialise and establish the connection. The socket is the same used by ListenSocketservices class
        /// </summary>
        /// <param name="ip">Unique IP address of each NodeMCU monitoring the PIR status</param>
        /// <param name="socket">ListenSocketservices which is common for all devices</param>
        public void EstablishConnection(string ip, string socket)
        {
            SSI.Initialise(ip, socket);
            //Send a Hello packet so the host can store the address though not required as it is hard coded
            SSI.SendMessageToHost(Constants.HELLO);
        }

        /// <summary>
        /// Method to start or stop monitoring of Intruder 
        /// </summary>
        /// <param name="bStart">Flag to indicate Monitoring status On or Off</param>
        public void StartStopMonitoring(bool bStart)
        {
            if (bStart)
            {
                bStartMonitor = true;
                SSI.SendMessageToHost(Constants.MONITORSTART);
            }
            else
            {
                bStartMonitor = false;
                SSI.SendMessageToHost(Constants.MONITORSTOP);
            }
        }

    }
}
