using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation
{
    public sealed class TemperatureMonitor
    {
        private SocketServices SSL;


        public TemperatureMonitor()
        {
            SSL = new SocketServices();
        }

        public void EstablishConnection(string ip, string socket)
        {
            SSL.Initialise(ip, socket);
            //Send a Hello packet so the host can store the address though not required as it is hard coded
            SSL.SendMessageToHost(Constants.HELLO);
        }

    }
}
