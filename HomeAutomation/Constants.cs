using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation
{
    static class Constants
    {
        //General Constants
        public const int ROOMS = 3; //Change depending upon your implementation

        //Sockets Services Constants for various Hosts
        public const string CURTAINSOCKET = "4210";
        public const string DIMMERSOCKET = "5210";
        public const string INTRUDERSOCKET = "8210";
        public const string TEMPSOCKET = "7210";
        public const string LISTENPORT = "51000";

        //Web Services Constants
        public const string SOCKET = "8090"; //Listening socket for Web Front
        public const string UDPSOCKET = "30285";
        public const int BUFFERSIZE = 8192;

        //IPAddress Constants used by ProcessAction - Curtain Motor and Light Dimmer
        public static string[] CurtainAddress = { "192.168.0.152", "192.168.0.151", "192.168.0.153" };
        public static string[] DimmerAddress = { "192.168.0.160", "192.168.0.161", "192.168.0.162" };

        //IP Address Constants used by ListenSocket Services. We will keep the sockets the same. Used by Temperature control and Intruder Detector
        public static string[] IntruderDetectAddress = { "192.168.0.191", "192.168.0.192", "192.168.0.193" };
        public static string[] TemperatureMonitorAddress = { "192.168.0.181", "192.168.0.182", "192.168.0.183" };

        //Curtain distances that vary with window / room
        public static int[] CurtainMaxDistances = { 90, 120, 150 };
        public static int[] CurtainMinDistance = { 5, 3, 1 };

    

        //Constants for Remote Screen Motor Commands
        //Motor Control 
        public const String OPEN = "Curtain Open";
        public const String STOP = "Curtain Stop";
        public const String CLOSE = "Curtain Close";
        public const String GETDIST = "Get Distance";
        public const String SETDIST = "Set Distance=";


        //Constants for Remote Light Control Commands
        //Light Control 
        public const String BRIGHT = "Brightness Value=";
        public const String NIGHT = "Brightness Night Value=";

        //Constants for Remote Temperature Monitor
        //Temperature Monitor 
        public const String HELLO = "Hello";    //Not really used

        //Constants for Intruder Detect
        //Intruder Detect
        public const String MONITORSTART = "Start Monitoring";
        public const String MONITORSTOP = "Stop Monitoring";
        public const String INTRUDERALIVE = "Monitoring Premises";
        public const String INTRUDERDETECT = "Intruder Detected";


        //Debug Constants
        public const string GUIDSTRING = "4bd2826e-54a1-4ba9-bf63-92b73ea1ac4a"; //GUID for Microsoft Diagnostic Logging Channel
        public const string LOGFILE = "HomeAutomationLog.txt";
        public const string LOGBAKFILE = "HomeAutomationBak.txt";

        //Webpage Messages
        public const string BUTTON = "/?Button";
        public const string SCREENUP = "Up";
        public const string SCREENDOWN = "Down";
        public const string SCREENSTOP = "Stop";
        public const string SLIDER = "/?Slider";
        public const string SWITCH = "/?Switch";
        public const string PORTSWITCH = "/?PortSetter";

        //Feedback Messages
        public const string TEMP = "0";
        public const string INTRUDER = "1";

        
    }
}
