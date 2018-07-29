using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation
{
    public sealed class ProcessActions
    {
        //Automation capability Defined.
        private List<CurtainMotor> Curtains = new List<CurtainMotor>();
        private List<LightDimmer> Dimmers = new List<LightDimmer>();

        //Only the IP Address. We will keep the sockets the same.
        //private string[] CurtainAddress = { "192.168.0.152", "192.168.0.151", "192.168.0.153" };
        //private string[] DimmerAddress = { "192.168.0.160", "192.168.0.161", "192.168.0.162" };

       

        public ProcessActions()
        {

        }


        /// <summary>
        /// This procedure will look for WebService application and then initiate and connect it. The  
        /// connection itself will be stored in the ApplicationServiceConnection variable which will be 
        /// used for communication with the Web Service.This will ensure to get the messages passed
        /// by the Web client(browser) and at the same time send a feedback to the WebService
        /// which will be picked by the Web client.
        /// It will also declare all the objects of Automation
        /// </summary>
        public void Initialise()
        {
            //Add the Automation Objects
            for (int i = 0; i < 3; i++)
            {
                //Add Curtain Motor
                CurtainMotor curtain = new CurtainMotor();
                curtain.EstablishConnection(Constants.CurtainAddress[i], Constants.CURTAINSOCKET);
                curtain.MaxDistance = Constants.CurtainMaxDistances[i];
                curtain.MinDistance = Constants.CurtainMinDistance[i];
                curtain.CurtainDistance = Constants.CurtainMinDistance[i];
                Curtains.Add(curtain);

               //Add Light Dimmer
                LightDimmer dimmer = new LightDimmer();
                dimmer.EstablishConnection(Constants.DimmerAddress[i], Constants.DIMMERSOCKET);
                Dimmers.Add(dimmer);
            }

        }

        public int GetDistance(int room)
        {
            if (room < Constants.ROOMS)
                return Curtains[room].CurtainDistance;
            return 0;
        }

        public int GetMinDistance(int room)
        {
            if (room < Constants.ROOMS)
                return Curtains[room].MinDistance;
            return 0;
        }

        public int GetMaxDistance(int room)
        {
            if (room < Constants.ROOMS)
                return Curtains[room].MaxDistance;
            return 0;
        }


        public int GetDimmerValue(int room, bool bNight)
        {
            if (room < Constants.ROOMS)
            {
                if (bNight)
                    return Dimmers[room].dimmerNightValue;
                else
                    return Dimmers[room].dimmerValue;          
            }
            return 0;
        }

        public void ProcessActionMessage(string data)
        {
            int index;
            int room;
            string msg;

            //Process Screen Button Messages
            if (data.IndexOf(Constants.BUTTON) >= 0) //Screen Button sends only value is clicked.
            {
                index = data.IndexOf("=");
                index--;

                //If more than 10 rooms, this will have to change to read 2 characters. Also use 01 instead of 1
                room = Convert.ToInt32(data.Substring(index, 1));
                msg = data.Substring(Constants.BUTTON.Length, index - Constants.BUTTON.Length);
                Curtains.ElementAt(room).ActionMessage(msg);
                return;
            }
            
            //Process Slider Message for Screens.
            if (data.IndexOf(Constants.SLIDER) >= 0) //Set Turns for Screen
            {
                index = data.IndexOf("=");

                //If more than 10 rooms, this will have to change to read 2 characters. Also use 01 instead of 1
                room = Convert.ToInt32(data.Substring(index - 1, 1));

                if (room < 3)
                {

                    msg = data.Substring(index + Constants.SLIDER.Length); //We need only values here. Skip '=Action_' that web returns
                    Curtains.ElementAt(room).SetDistance(Convert.ToInt32(msg));
                }
                else
                {
                    if (room < 6)
                    {
                        room -= 3;
                        msg = data.Substring(index + Constants.SLIDER.Length); //We need only values here. Skip '=Action_' that web returns
                        if (room == 1)
                        {
                            Dimmers.ElementAt(0).bNight = true;
                            Dimmers.ElementAt(0).ActionDim(Convert.ToInt32(msg));
                        }
                        else
                        {
                            Dimmers.ElementAt(room).bNight = false;
                            Dimmers.ElementAt(room).ActionDim(Convert.ToInt32(msg));
                        }
                    }

                }
                return;
            }
        }
    }
}
