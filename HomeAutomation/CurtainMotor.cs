using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation
{   
    /// <summary>
    /// Create an instance of this class for each window that has curtain motors. Note that maximum and minimum 
    /// value which is required for positioning of the curtain is defined in Constants. Measure the window and 
    /// provide adequate distance both for close and open of the curtain.
    /// </summary>
    public sealed class CurtainMotor
    {
        private SocketServices SS;
        public int CurtainDistance { get; set; }
        public int MinDistance { get; set; }
        public int MaxDistance { get; set; }

        /// <summary>
        /// Define a socket service that will be used to communicate with NodeMCU. 
        /// Curtain distance is the variable that actually stores the current position of the curtain in cm. 
        /// By defining as 1 we assume the curtain is about 1cm from the window ledge - that is closed.
        /// </summary>
        public CurtainMotor()
        {
            SS = new SocketServices();
            CurtainDistance = 1;
        }

        /// <summary>
        /// Initialise the socket for communication. Though Each window curtain will have same socket 
        /// for communication, they are unique in their IP addresses
        /// </summary>
        /// <param name="ip">IP Address from Constants. Unique to each NodeMCU</param>
        /// <param name="socket">Defined socket for communication. Same to all curtain controllers</param>
        public void EstablishConnection(string ip, string socket)
        {
            SS.Initialise(ip, socket);
        }

        /// <summary>
        /// Action messages to be sent to curtain motors. Typically Open, close and Stop
        /// </summary>
        /// <param name="msg">The message to be sent</param>
        public void ActionMessage(string msg)
        {
            if (msg.Contains(Constants.SCREENUP))
            {
                SS.SendMessageToHost(Constants.OPEN);
                CurtainDistance = MaxDistance;
            }
            else
            {
                if (msg.Contains(Constants.SCREENDOWN))
                {
                    SS.SendMessageToHost(Constants.CLOSE);
                    CurtainDistance = MinDistance;
                }
                else
                    SS.SendMessageToHost(Constants.STOP);
            }
        }

        /// <summary>
        /// Unique to Curtain motors. With the slider control you can select where you want the curtains to be. 
        /// Very useful if you have a sunlight entering the room. You can control the amount of sunlight in the room
        /// by adjusting the curtain position. The motor will close or open depending upon its present position. 
        /// You can always send a "STOP" message anytime to stop the motor.
        /// </summary>
        /// <param name="dist">Distance from the ledge which amounts to the curtain Open/Close position</param>
        public void SetDistance(int dist)
        {
            if (dist == 0)
                SS.SendMessageToHost(Constants.STOP);
            else
                SS.SendMessageToHost(Constants.SETDIST + (dist).ToString() );//required for Motor controller which looks for > 10 for turns

            CurtainDistance = dist;
        }


    }
}
