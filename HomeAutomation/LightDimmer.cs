using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation
{
    /// <summary>
    /// Simple Dimmer circuit controlled by NodeMCU. As an additional requirement I have provided a separate Night
    /// lamp control on the same circuit. If this is not your requirement, this can be deleted. Or if you want to 
    /// you can replicate this to all the room - GUI must have slider control to adjust this.
    /// </summary>
    class LightDimmer
    {
        private SocketServices SSL;
        public bool bNight { get; set; }
        public int dimmerValue { get; set; }
        public int dimmerNightValue { get; set; }

        /// <summary>
        /// Initialise the class with Nightlamp flag and Socket
        /// </summary>
        public LightDimmer()
        {
            SSL = new SocketServices();
            bNight = false;
            dimmerValue = 0;
            dimmerNightValue = 0;
        }

        /// <summary>
        /// Establish connection. IP is unique while Socket is same for all light dimmers
        /// </summary>
        /// <param name="ip">Unique IP address</param>
        /// <param name="socket">Common Socket for all Light dimmers</param>
        public void EstablishConnection(string ip, string socket)
        {
            SSL.Initialise(ip, socket);
        }

        /// <summary>
        /// The only action that this method will perform. You can set the status of Dim from 0 to  100
        /// if you have a night light then that will be separately controlled. If you are not using the 
        /// Night lamp then you can comment the line if(bNight)...up to else and opening { and the closing } 
        /// braces of the else statement.
        /// </summary>
        /// <param name="dim"></param>
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
