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

        public ListenSocketServices()
        {
            //bIAlert = false;
            GuestSocket =  new DatagramSocket();
            LogTempr = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + "TR.txt";
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

            //Initialise the buffer for storing Temperatures and Intruder Detection strings
            temperatureAlerts = new string[Constants.ROOMS];
            //intruderAlerts = new string[Constants.ROOMS];

            for (int i = 0; i < Constants.ROOMS; i++)
            {
                temperatureAlerts[i] = "No Connectivity to Room: " + (i + 1).ToString() + "\n";
                //intruderAlerts[i] = "";
            }
            GuestSocket.MessageReceived += MessageReceivedAsync;
            GuestSocket.BindServiceNameAsync(Constants.LISTENPORT).AsTask().Wait();

        }

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

                        //counter--;
                        //if (counter <= 0)
                        //    source.Cancel();
                    }
                }, period);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

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

                if (RemoteMessage.Contains("temperature is: "))  //10 characters
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
                        if (RemoteMessage.Contains("Garage")) //Un comment this if all six rooms are used.
                        {
                            RemoteMessage += " @ " + DateTime.Now.ToString();
                            await Task.Run(() => SendEmailAsync(RemoteMessage));
                        }

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
        /// Data received from "MessageReceived" must be sorted out here and properly stored
        /// </summary>
        /// <param name="feedBack"></param>
        private void StoreData(string data, string ipAddress)
        {


        }

        public string GetTemperature(int Room)
        {
            return temperatureAlerts[Room];
        }

        /*
        public string GetIntruderAlert(int Room)
        {
            string strRet = intruderAlerts[Room];
            intruderAlerts[Room] = "";
            return strRet;
        }
        */

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

        public string GetPortSwitchStatus(int room)
        {
            return Detectors[room].bStartMonitor.ToString().ToLowerInvariant();
        }


         public async void SendEmailAsync(string strMessage)
         {
             try
             {
                 using (SmtpClient client = new SmtpClient("alpha-dal.serversecuredns.com", 465, true, "no-reply@brahas.com", "p@ssW0rd"))
                 {
                     EmailMessage emailMessage = new EmailMessage();

                    emailMessage.To.Add(new EmailRecipient("vdevan@gmail.com"));
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
