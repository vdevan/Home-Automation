using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation
{
    class LightDimmer
    {
        private SocketServices SSL;
        public bool bNight { get; set; }
        public int dimmerValue { get; set; }
        public int dimmerNightValue { get; set; }
        public LightDimmer()
        {
            SSL = new SocketServices();
            bNight = false;
            dimmerValue = 0;
            dimmerNightValue = 0;
        }

        public void EstablishConnection(string ip, string socket)
        {
            SSL.Initialise(ip, socket);
        }

        public void ActionDim(int dim)
        {
            if (dim >= 0 && dim <= 100)
            {
                if (bNight)
                {
                    SSL.SendMessageToHost(Constants.NIGHT + dim);
                    dimmerNightValue = dim;
                }
                else
                {
                    SSL.SendMessageToHost(Constants.BRIGHT + dim);
                    dimmerValue = dim;
                }
            }
        }
    }
}
