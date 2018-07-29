using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation
{
    class IntruderDetect
    {
        private SocketServices SSI;
        public string strAlive { get; set; }
        public bool bStartMonitor { get; set; }

        public IntruderDetect()
        {
            SSI = new SocketServices();
            bStartMonitor = false;
        }

        public void EstablishConnection(string ip, string socket)
        {
            SSI.Initialise(ip, socket);
            //Send a Hello packet so the host can store the address though not required as it is hard coded
            SSI.SendMessageToHost(Constants.HELLO);
        }

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
