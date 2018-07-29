using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation
{
    /// <summary>
    /// Temperature monitor class to monitor the room temperature
    /// </summary>
    public sealed class TemperatureMonitor
    {
        private SocketServices SSL;

        /// <summary>
        /// Declare and initialise SocketServices
        /// </summary>
        public TemperatureMonitor()
        {
            SSL = new SocketServices();
        }

        /// <summary>
        /// Establish connection with the right IP address and Socket for communicating with the device
        /// </summary>
        /// <param name="ip">IP address - unique to the device</param>
        /// <param name="socket">The socket through which messages are sent and the device is configured to
        /// listen to this socket</param>
        public void EstablishConnection(string ip, string socket)
        {
            SSL.Initialise(ip, socket);
            //Send a Hello packet so the host can store the address though not required as it is hard coded
            SSL.SendMessageToHost(Constants.HELLO);
        }

    }
}
