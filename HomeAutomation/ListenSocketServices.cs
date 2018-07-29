using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using LightBuzz.SMTP;
using Windows.ApplicationModel.Email;
using System.Net;
using System.Threading;
using Windows.UI.Xaml;
using System.IO;
using Windows.System.Threading;


namespace HomeAutomation
{
    /// <summary>
    /// This is the reverse of ProcessAction class. While Process Action sends message to NodeMCU devices, this listens 
    /// to the message from the devices. Temperature and Intruder Alert are the messages sent from the devices. 
    /// Temperature sensor will update at frequent intervals while Intruder Alert will only send a message if Alert 
    /// is on and an intruder is detected.
    /// </summary>
    public sealed class ListenSocketServices
    {
        //Check and remove all the variables
        private HostName RemoteAddress;
        private string RemotePort;
        private string RemoteMessage;
        private string[] temperatureAlerts;
        //private string[] intruderAlerts;



        //Automation Objects
        private List<IntruderDetect> Detectors = new List<IntruderDetect>();
        private List<TemperatureMonitor> Temperatures = new List<TemperatureMonitor>();
        //public bool bIAlert { get; set; }
        private DatagramSocket GuestSocket;

        string LogTempr;
        private ThreadPoolTimer timer;
        public bool bRec { get; set; }

        /// <summary>
        /// Temperature sensor can log text to a file. All log files are stored under the folder
        /// "User Folders\LocalAppData\HomeAutomation\LocalState". While this path is hard coded, the name of 
        /// the file can be changed in Constants.
        /// </summary>
        public ListenSocketServices()
        {
            //bIAlert = false;
            GuestSocket =  new DatagramSocket();
            LogTempr = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + Constants.TEMPLOGFILE;
            bRec = false;
        }

        /// <summary>
        /// This is only used by Webfront class. Thus only one listen Port while multiple send ports 
        /// are used by Curtain Motor and light dimmers
        /// </summary>       
        public void Initialise()
        {
            //Initialise the Automation Objects
            //Add the Automation Objects
            for (int i = 0; i < Constants.ROOMS; i++)
            {
                //Add Temperature Monitor Objects
                TemperatureMonitor tm = new TemperatureMonitor();
                tm.EstablishConnection(Constants.TemperatureMonitorAddress[i], Constants.TEMPSOCKET);
                Temperatures.Add(tm);

                //Add Intruder Detectors
                IntruderDetect detector = new IntruderDetect();
                detector.EstablishConnection(Constants.IntruderDetectAddress[i], Constants.INTRUDERSOCKET);
                Detectors.Add(detector);
            }

            //Initialise the buffer for storing Temperatures strings
            temperatureAlerts = new string[Constants.ROOMS];

            for (int i = 0; i < Constants.ROOMS; i++)
            {
                temperatureAlerts[i] = "No Connectivity to Room: " + (i + 1).ToString() + "\n";
                
            }
            GuestSocket.MessageReceived += MessageReceivedAsync;
            GuestSocket.BindServiceNameAsync(Constants.LISTENPORT).AsTask().Wait();

        }

        /// <summary>
        /// If Temperature recording is required then this function will start or stop the action. Note currently
        /// only one sensor is used and the recordings are done for one sensor only. If you need to use more sensors
        /// then uncomment the two lines - //for(inti=0.... and //writer.WriteLine...
        /// </summary>
        /// <param name="Start">Flag to start or stop the recordings</param>
        public void StartRec(bool Start)
        {
            try
            {
                if (!Start)
                {
                    bRec = false;
                    timer.Cancel(); //Stop the recording
                    return;
                }
                bRec = true;
                TimeSpan period = TimeSpan.FromMinutes(10);

                timer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
                {

                    FileStream fs = new FileStream(LogTempr, FileMode.Append);
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                    //for (int i=0; i<Constants.ROOMS; i++)
                    //    writer.WriteLineAsync(temperatureAlerts[i]);

                        await writer.WriteLineAsync(temperatureAlerts[0]);

                        writer.Flush();

                    }
                }, period);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }


        /// <summary>
        /// This will monitor various messages and act on them accordingly. Temperature sensor messages are stored in the
        /// variable with DateTime Stamp added. Thus if the datetime stamp is different from current time (10 minutes grace)
        /// then you know the device is not functioning. Intruder Detect also sends INTRUDERALIVE pulse. Though not used
        /// currently, similar methods to be followed for expanding this to all devices, to provide keep alive pulse and
        /// initmate user when it fails. Need to expand across all devices. Left for next version.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void MessageReceivedAsync(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            try
            {
                DataReader reader = args.GetDataReader();
                uint len = reader.UnconsumedBufferLength;

                RemoteMessage = reader.ReadString(len);
                RemoteAddress = args.RemoteAddress;
                RemotePort = args.RemotePort;
                //Debug.WriteLine("Remote Message Received from IP: {0} Port: {1} Message: {2}", RemoteAddress.ToString(), RemotePort, RemoteMessage);

                if (RemoteMessage.Contains(Constants.TEMPSENSE))  //10 characters
                {
                    for (int i = 0; i < Constants.ROOMS; i++)
                    {
                        if (RemoteAddress.ToString() == Constants.TemperatureMonitorAddress[i])
                        {
                            temperatureAlerts[i] = "Room " + i + 1 + RemoteMessage + " @ " + DateTime.Now.ToString() + "\n";
                            break;
                        }
                    }
                }
                else
                {
                    if(RemoteMessage.Contains (Constants.INTRUDERDETECT))
                    {
                        Debug.WriteLine(RemoteMessage);
                        RemoteMessage += " @ " + DateTime.Now.ToString();
                        await Task.Run(() => SendEmailAsync(RemoteMessage));
                    }
                    else
                    {
                        if (RemoteMessage.Contains (Constants.INTRUDERALIVE))
                        {
                            for (int i = 0; i < Constants.ROOMS; i++)
                            {
                                if (RemoteAddress.ToString() == Constants.IntruderDetectAddress[i]) 
                                {
                                    Detectors[i].strAlive = RemoteMessage;
                                    break;
                                }
                            }
                        }
                    }    

                    
                }
                reader.Dispose();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Data received from "MessageReceived" is sorted out here and properly stored.
        /// Reserved for next version
        /// </summary>
        /// <param name="feedBack"></param>
        private void StoreData(string data, string ipAddress)
        {


        }

        /// <summary>
        /// If a user query about Temperature of a particular room, that message is passed here. This method 
        /// will retrieve the latest stored temperature.
        /// </summary>
        /// <param name="Room">Room for which Temperature is requested</param>
        /// <returns></returns>
        public string GetTemperature(int Room)
        {
            return temperatureAlerts[Room];
        }

        /// <summary>
        /// This method is used for setting Intruder Alert on or Off
        /// </summary>
        /// <param name="data">Room no for setting the alerts</param>
        public void PortSwitchMessage(string data)
        {
            int room;
            string msg;
            int index = data.IndexOf("=");
            
            //If more than 10 rooms, this will have to change to read 2 characters. Also use 01 instead of 1
            room = Convert.ToInt32(data.Substring(index -1, 1));
            msg = data.Substring(index+1);
            if (msg == "Action_Checked")
                Detectors[room].StartStopMonitoring(true);
            else
            {
                if (msg=="Action_UnChecked")
                    Detectors[room].StartStopMonitoring(false);
            }
        }

        /// <summary>
        /// You can also retrieve the current status of the monitoring of Intruder Alert. This will be required
        /// by Webfront class to provide current status
        /// </summary>
        /// <param name="room">Room no for which status requested</param>
        /// <returns></returns>
        public string GetPortSwitchStatus(int room)
        {
            return Detectors[room].bStartMonitor.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// Sends and Email alert when a user is detected. Email address is provided in Constants. If cc and bcc
        /// are required they can be configured too.
        /// </summary>
        /// <param name="strMessage"></param>
         public async void SendEmailAsync(string strMessage)
         {
             try
             {
                 using (SmtpClient client = new SmtpClient("alpha-dal.serversecuredns.com", 465, true, "no-reply@brahas.com", "p@ssW0rd"))
                 {
                     EmailMessage emailMessage = new EmailMessage();

                    emailMessage.To.Add(new EmailRecipient(Constants.EMAILADDRESS));
                     //emailMessage.CC.Add(new EmailRecipient("someone2@anotherdomain.com"));
                     //emailMessage.Bcc.Add(new EmailRecipient("someone3@anotherdomain.com"));
                    emailMessage.Subject = "Intruder Detection Service";
                    emailMessage.Body = strMessage;
                    emailMessage.Body += "\nThis is an email sent from Home Automation. Please do not reply to this mail.";

                    await client.SendMailAsync(emailMessage);
                 }
             }
             catch (Exception ex)
             {
                 Debug.WriteLine(ex.ToString());
             }

         }

        
    }
}
