using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation
{
    public sealed class CurtainMotor
    {
        private SocketServices SS;
        public int CurtainDistance { get; set; }
        public int MinDistance { get; set; }
        public int MaxDistance { get; set; }

        public CurtainMotor()
        {
            SS = new SocketServices();
            CurtainDistance = 1;
        }

        public void EstablishConnection(string ip, string socket)
        {
            SS.Initialise(ip, socket);
        }


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
