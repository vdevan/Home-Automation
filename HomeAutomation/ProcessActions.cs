using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation
{
    /// <summary>
    /// Process Action sends message to NodeMCU devices, unlike ListenSocketServices which this listens 
    /// to the message from the devices. Curtain Motor control and Light dimmer devices come under this 
    /// category. Both the devices are controlled by sending action messages that are pre-defined to 
    /// take appropriate action
    /// </summary>
    public sealed class ProcessActions
    {
        //Automation capability Defined.
        private List<CurtainMotor> Curtains = new List<CurtainMotor>();
        private List<LightDimmer> Dimmers = new List<LightDimmer>();

     

        public ProcessActions()
        {

        }


        /// <summary>
        /// Create the instances of Curtain Motor class and Light Dimmer class. Initially only 3 rooms are defined.
        /// If you have more rooms to control just increae the Rooms in Constants. Establish connection for each of
        /// the defined devices, so they are ready to receive messages
        /// /// </summary>
        public void Initialise()
        {
            //Add the Automation Objects
            for (int i = 0; i < Constants.ROOMS; i++)
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

        /// <summary>
        /// Currently used as feedback to the web interface so the slider button can be properly synchrnised. 
        /// One drawback is that it does not actually get the distance though that facility is available in the device
        /// program. This will send the stored value which need not be the same as the current curtain position if it 
        /// has been moved manually or stopped half way through an operation. 
        /// </summary>
        /// <param name="room">Room or Window for which the position is required</param>
        /// <returns></returns>
        public int GetDistance(int room)
        {
            if (room < Constants.ROOMS)
                return Curtains[room].CurtainDistance;
            return 0;
        }

        /// <summary>
        /// This is used for setting the minimum value for sliders as the web interface HTML page is prepared. 
        /// Apart from that it has no use.
        /// </summary>
        /// <param name="room">Room or Window for which this value is required</param>
        /// <returns></returns>
        public int GetMinDistance(int room)
        {
            if (room < Constants.ROOMS)
                return Curtains[room].MinDistance;
            return 0;
        }

        /// <summary>
        /// This is used for setting the maximum value for sliders as the web interface HTML page is prepared. 
        /// Apart from that it has no use.
        /// </summary>
        /// <param name="room">Room or Window for which this value is required</param>
        /// <returns></returns>
        public int GetMaxDistance(int room)
        {
            if (room < Constants.ROOMS)
                return Curtains[room].MaxDistance;
            return 0;
        }

        /// <summary>
        /// To set the slider at the correct position this value is sought by Web Interface.
        /// </summary>
        /// <param name="room">Room for which this value is required</param>
        /// <param name="bNight">Flag to indicate night light or normaly lighting</param>
        /// <returns></returns>
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

        /// <summary>
        /// This is where the webinterface drops the messages that are received from user browser. The method
        /// will sort through the message and initiates appropriate action by calling in the NodeMCU devices. 
        /// The message itself will carry room nos and the action to be taken - unique for light dimmers and 
        /// curtain motors
        /// </summary>
        /// <param name="data">The data received fromt the user web query</param>
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
