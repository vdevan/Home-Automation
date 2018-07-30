using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using System.Text;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.ApplicationModel.AppService;
using System.Linq;
using Windows.Foundation.Collections;
using System.Collections.Generic;
using Windows.Foundation.Diagnostics; //Used for debugging
using System.Diagnostics;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Networking;

namespace HomeAutomation
{
    /// <summary>
    /// Web front service. Listens to request, prepares the response and submits to the incoming request. 
    /// It could be a simple web page request - by just sending "/" or a query to get a data - by sending "/?"
    /// This is where all messages arrive and are properly dispatched - both to the application and also to the 
    /// user. A simple background application that runs automatically when the system starts.
    /// </summary>
    public sealed class Webfront
    {
        private LoggingChannel lc = new LoggingChannel("Web Front", null, new Guid(Constants.GUIDSTRING)); //Used for debugging
        private bool writeLog = false;
        private bool LOG = false; //Used for debugging
        private string LogPath;

        private string headerFile;
        private string cssFile;
        private string bodyFile;

        private readonly ListenSocketServices LSS;
        private readonly ProcessActions PA;

        public Webfront()
        {
            LSS = new ListenSocketServices();
            PA = new ProcessActions();
        }

        /// <summary>
        /// Start of the Webpage. Load all the files required for webpage presentation
        /// Bind and listen to socket
        /// </summary>
        public async void Start()
        {
            await Task.Run(() => BackupLogFile());
            headerFile = File.ReadAllText("webpages\\header.html");
            cssFile = File.ReadAllText("webpages\\theme.css");
            bodyFile = File.ReadAllText("webpages\\body.html");

            StreamSocketListener listener = new StreamSocketListener();
            await listener.BindServiceNameAsync(Constants.SOCKET);
            listener.ConnectionReceived += Listener_ConnectionReceived;

            if (LOG)
                lc.LogMessage("Webhost started successfully");

            if (writeLog)
                WriteDebugInfo("Webhost started successfully");

            //UDP Listener for Temperature and Intruder sensors
            LSS.Initialise();

            //Actions for Automation
            PA.Initialise();
        }

        /// <summary>
        /// Take a backup of Log file if it exists. File names are provided in Constants. 
        /// All log files are stored under the folder
        /// "User Folders\LocalAppData\HomeAutomation\LocalState". While this path is hard coded, the name of 
        /// the file can be changed in Constants.
        /// </summary>
        private void BackupLogFile()
        {
            try
            {
                LogPath = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + Constants.LOGFILE;
                if (File.Exists(LogPath))
                {
                    string strBak = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\" + Constants.LOGBAKFILE;
                    File.Copy(LogPath, strBak, true);
                    File.Delete(LogPath);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// This the main interface to user - a simple HTML page with JQuery scripts. Socket used is 8080 for incoming
        /// HTML requests. When the request arrives a detailed HTML page is created and submitted to the requesting 
        /// browser. The stored variables are updated as well 
        /// </summary>
        /// <param name="sender">StreamSocketListener with all details are passed</param>
        /// <param name="args">Provides connection details. Use the details to provide to present the webpage</param>
        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            StringBuilder request = new StringBuilder();
            try
            {
                using (IInputStream input = args.Socket.InputStream)
                {
                    byte[] readBuffer = new byte[Constants.BUFFERSIZE];
                    IBuffer buffer = readBuffer.AsBuffer();
                    uint dataRead = Constants.BUFFERSIZE;
                    while (dataRead == Constants.BUFFERSIZE)
                    {
                        await input.ReadAsync(buffer, Constants.BUFFERSIZE, InputStreamOptions.Partial);
                        request.Append(Encoding.UTF8.GetString(readBuffer, 0, readBuffer.Length));
                        dataRead = buffer.Length;
                    }
                }

                string inputString = GetQuery(request);
                if (inputString == "/" || inputString == "/favicon.ico")
                {
                    //Set the variables in the script
                    string variables = "<script>$(window).load(function() {";

                    //Initial Values No need to specify previous action
                    variables += "$(\"#retVal\").text(\"None\");";
                    variables += "$(\"#log\").text(\"Initialised\");";

                    //Set Motor Turns Sliders correctly
                    variables += string.Format("$(\".trnclass#SliderA0\").attr(\"min\", {0});", PA.GetMinDistance(0));
                    variables += string.Format("$(\".trnclass#SliderA0\").attr(\"max\", {0});", PA.GetMaxDistance(0));
                    variables += string.Format("$(\".trnclass#SliderA0\").val({0});", PA.GetDistance(0));
                    variables += "$(\".trnclass#SliderA0\").trigger('change');";
                    variables += string.Format("$(\".trnclass#SliderA0\").prev().text(\"{0}\");", PA.GetDistance(0));

                    variables += string.Format("$(\".trnclass#SliderA1\").attr(\"min\", {0});", PA.GetMinDistance(1));
                    variables += string.Format("$(\".trnclass#SliderA1\").attr(\"max\", {0});", PA.GetMaxDistance(1));
                    variables += string.Format("$(\".trnclass#SliderA1\").val({0});", PA.GetDistance(1));
                    variables += "$(\".trnclass#SliderA1\").trigger('change');";
                    variables += string.Format("$(\".trnclass#SliderA1\").prev().text(\"{0}\");", PA.GetDistance(1));

                    variables += string.Format("$(\".trnclass#SliderA2\").attr(\"min\", {0});", PA.GetMinDistance(2));
                    variables += string.Format("$(\".trnclass#SliderA2\").attr(\"max\", {0});", PA.GetMaxDistance(2));
                    variables += string.Format("$(\".trnclass#SliderA2\").val({0});", PA.GetDistance(2));
                    variables += "$(\".trnclass#SliderA2\").trigger('change');";
                    variables += string.Format("$(\".trnclass#SliderA2\").prev().text(\"{0}\");", PA.GetDistance(2));

                    //Set Light Dimmers Sliders correctly
                    variables += string.Format("$(\".sldrclass#SliderB0\").val({0});", PA.GetDimmerValue(0,false));
                    variables += "$(\".sldrclass#SliderB0\").trigger('change');";
                    variables += string.Format("$(\".sldrclass#SliderB0\").next().text(\"{0}\");", PA.GetDimmerValue(0,false));

                    variables += string.Format("$(\".sldrclass#SliderB1\").val({0});", PA.GetDimmerValue(0, true));
                    variables += "$(\".sldrclass#SliderB1\").trigger('change');";
                    variables += string.Format("$(\".sldrclass#SliderB1\").next().text(\"{0}\");", PA.GetDimmerValue(0, true));

                    variables += string.Format("$(\".sldrclass#SliderB2\").val({0});", PA.GetDimmerValue(2, false));
                    variables += "$(\".sldrclass#SliderB2\").trigger('change');";
                    variables += string.Format("$(\".sldrclass#SliderB2\").next().text(\"{0}\");", PA.GetDimmerValue(2, false));

                    // Log Information
                    variables += string.Format("$(\"input[name=LogDiag]\").prop(\"checked\", {0});", LOG.ToString().ToLowerInvariant());
                    variables += string.Format("$(\"input[name=LogFile]\").prop(\"checked\", {0});", writeLog.ToString().ToLowerInvariant());
                    variables += string.Format("$(\"input[name=LogTemp]\").prop(\"checked\", {0});", LSS.bRec.ToString().ToLowerInvariant());

                    //Port Setter switches are used for Starting or stopping Monitoring for Intruders
                    variables += string.Format("$(\"input[name=PortSetter0]\").prop(\"checked\", {0});", LSS.GetPortSwitchStatus(0));
                    variables += string.Format("$(\"input[name=PortSetter1]\").prop(\"checked\", {0});", LSS.GetPortSwitchStatus(1));
                    variables += string.Format("$(\"input[name=PortSetter2]\").prop(\"checked\", {0});", LSS.GetPortSwitchStatus(2));

                    //Not used
                    /*variables += string.Format("$(\"input[name=PortSetter3]\").prop(\"checked\", {0});", LSS.GetPortSwitchStatus(3));
                    variables += string.Format("$(\"input[name=PortSetter4]\").prop(\"checked\", {0});", LSS.GetPortSwitchStatus(4));
                    variables += string.Format("$(\"input[name=PortSetter5]\").prop(\"checked\", {0});", LSS.GetPortSwitchStatus(5));*/

                    variables += "})</script > ";
                    
                    using (IOutputStream output = args.Socket.OutputStream)
                    {
                        using (Stream response = output.AsStreamForWrite())
                        {

                            byte[] bodyArray = Encoding.UTF8.GetBytes(
                                    $"<!DOCTYPE html>\n<html>\n<head>{headerFile}{variables}<style>{cssFile}</style>\n</head>\n<body>{bodyFile}</body>\n</html>");

                            var bodyStream = new MemoryStream(bodyArray);

                            var header = "HTTP/1.1 200 OK\r\n" +
                                         $"Content-Length: {bodyStream.Length}\r\n" +
                                         "Connection: close\r\n\r\n";

                            // Note Header file will not contain the above header. This is added here separately, storing in response variable
                            // and bodyStream will be constructed from headerFile, cssFile and bodyFile. This is appended to response and then 
                            // output as Web Front

                            byte[] headerArray = Encoding.UTF8.GetBytes(header);
                            await response.WriteAsync(headerArray, 0, headerArray.Length);
                            await bodyStream.CopyToAsync(response);
                            await response.FlushAsync();
                            return;

                        }
                    }
                }
                if (inputString.Contains("/?LogDiag"))
                {
                    LOG = inputString.Contains("Action_Checked") ? true : false;
                    PostFeedback("None", args);
                    return;
                }

                if (inputString.Contains("/?LogFile"))
                {
                    writeLog = inputString.Contains("Action_Checked") ? true : false;
                    PostFeedback("None", args);
                    return;
                }
                if (inputString.Contains("/?LogTemp"))
                {
                    if (inputString.Contains("Action_Checked"))
                        await Task.Run(()=>LSS.StartRec(true));
                    else
                        await Task.Run(() => LSS.StartRec(false));
                    PostFeedback("None", args);
                    return;
                }

                if (LOG)
                    lc.LogMessage("Message: " + inputString + " is passed to calling program");

                if (writeLog)
                    WriteDebugInfo("Message: " + inputString + " is passed to calling program");


                if (!inputString.Contains("Action")) //Process anything that is not connected with Action here
                {
                    PostFeedback("None", args);
                    return;
                }
               
                if (inputString.Contains("Switch"))
                {
                    int index = inputString.IndexOf("=");
                    if (index < 0)
                    {
                        PostFeedback("None", args);
                        return;
                    }

                    index = Convert.ToInt32(inputString.Substring(index - 1, 1));
                    if (index < Constants.ROOMS)
                        inputString = LSS.GetTemperature(index);

                }
                else
                {
                    //Process Slide and Button messages here
                    if (inputString.Contains("Button") || inputString.Contains("Slider"))
                        await Task.Run(() => PA.ProcessActionMessage(inputString));

                    else //Port Switches
                    {
                        if (inputString.Contains ("PortSetter"))
                            await Task.Run(() => LSS.PortSwitchMessage(inputString));
                    }

                    inputString += " Time Stamp: " + DateTime.Now.ToString();
                }

                PostFeedback(inputString, args);


            }
            catch (Exception Ex)
            {
                Debug.WriteLine("Exception occurred: {0}", Ex);
                //Next few lines for logging the exception errors
                if (LOG)
                    lc.LogMessage("Exception at Input String: " + Ex);

                if (writeLog)
                    WriteDebugInfo("Exception at Input String: " + Ex);

            }

        }


        /// <summary>
        /// Provide feedback to inform the action processed. Appends with proper message header before being sent
        /// </summary>
        /// <param name="strMessage">Message to be sent to web client</param>
        private async void PostFeedback(string strMessage, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                using (IOutputStream output = args.Socket.OutputStream)
                {
                    using (Stream response = output.AsStreamForWrite())
                    {
                        byte[] dataArray = Encoding.UTF8.GetBytes(strMessage);
                        var dataStream = new MemoryStream(dataArray);
                        var fbheader = "HTTP/1.1 200 OK\r\n" +
                                        $"Content-Length: {dataStream.Length}\r\n" +
                                        "Connection: close\r\n\r\n";
                        byte[] fbArray = Encoding.UTF8.GetBytes(fbheader);
                        await response.WriteAsync(fbArray, 0, fbArray.Length);
                        await dataStream.CopyToAsync(response);
                        await response.FlushAsync();
                    }
                }
            }
            catch (Exception Ex)
            {
                Debug.WriteLine("Exception occurred: {0}", Ex);
                //Next few lines for logging the exception errors
                if (LOG)
                    lc.LogMessage("Exception at Input String: " + Ex);

                if (writeLog)
                    WriteDebugInfo("Exception at Input String: " + Ex);

            }
        }

        /// <summary>
        /// Used to filter the incoming request and send only the valid data, stripping of the message headers
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private string GetQuery(StringBuilder request)
        {
            string data = "";
            var requestLines = request.ToString().Split(' ');

            if (requestLines[0] == "POST")
            {
                return requestLines[requestLines.Length - 1];
            }

            data = requestLines.Length > 1 ? requestLines[1] : "Action_Unregistered".ToString();

            return data;
        }

        /// <summary>
        /// If log to file is turned on, then the events are logged here as a tracing mechanism. More details if required
        /// can be added, if this is not adequate.
        /// </summary>
        /// <param name="strText"></param>
        public async void WriteDebugInfo(string strText)
        {
            try
            {
                FileStream fs = new FileStream(LogPath, FileMode.Append);

                using (StreamWriter writer = new StreamWriter(fs))
                {
                    strText += " Time Stamp: " + DateTime.Now.ToString();
                    await writer.WriteLineAsync(strText);
                    writer.Flush();
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
