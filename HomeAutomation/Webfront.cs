﻿using System;
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
                    variables += string.Format("$(\".trnclass#Slider0\").attr(\"min\", {0});", PA.GetMinDistance(0));
                    variables += string.Format("$(\".trnclass#Slider0\").attr(\"max\", {0});", PA.GetMaxDistance(0));
                    variables += string.Format("$(\".trnclass#Slider0\").val({0});", PA.GetDistance(0));
                    variables += "$(\".trnclass#Slider0\").trigger('change');";
                    variables += string.Format("$(\".trnclass#Slider0\").prev().text(\"{0}\");", PA.GetDistance(0));

                    variables += string.Format("$(\".trnclass#Slider1\").attr(\"min\", {0});", PA.GetMinDistance(1));
                    variables += string.Format("$(\".trnclass#Slider1\").attr(\"max\", {0});", PA.GetMaxDistance(1));
                    variables += string.Format("$(\".trnclass#Slider1\").val({0});", PA.GetDistance(1));
                    variables += "$(\".trnclass#Slider1\").trigger('change');";
                    variables += string.Format("$(\".trnclass#Slider1\").prev().text(\"{0}\");", PA.GetDistance(1));

                    variables += string.Format("$(\".trnclass#Slider2\").attr(\"min\", {0});", PA.GetMinDistance(2));
                    variables += string.Format("$(\".trnclass#Slider2\").attr(\"max\", {0});", PA.GetMaxDistance(2));
                    variables += string.Format("$(\".trnclass#Slider2\").val({0});", PA.GetDistance(2));
                    variables += "$(\".trnclass#Slider2\").trigger('change');";
                    variables += string.Format("$(\".trnclass#Slider2\").prev().text(\"{0}\");", PA.GetDistance(2));

                    //Set Light Dimmers Sliders correctly
                    variables += string.Format("$(\".sldrclass#Slider3\").val({0});", PA.GetDimmerValue(0,false));
                    variables += "$(\".sldrclass#Slider3\").trigger('change');";
                    variables += string.Format("$(\".sldrclass#Slider3\").next().text(\"{0}\");", PA.GetDimmerValue(0,false));

                    variables += string.Format("$(\".sldrclass#Slider4\").val({0});", PA.GetDimmerValue(0, true));
                    variables += "$(\".sldrclass#Slider4\").trigger('change');";
                    variables += string.Format("$(\".sldrclass#Slider4\").next().text(\"{0}\");", PA.GetDimmerValue(0, true));

                    variables += string.Format("$(\".sldrclass#Slider5\").val({0});", PA.GetDimmerValue(2, false));
                    variables += "$(\".sldrclass#Slider5\").trigger('change');";
                    variables += string.Format("$(\".sldrclass#Slider5\").next().text(\"{0}\");", PA.GetDimmerValue(2, false));

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
                    /*else
                    {
                        //Testing for sending email
                        await Task.Run(()=>LSS.SendEmailAsync());
                        inputString += " Time Stamp: " + DateTime.Now.ToString() + "\n";
                    }*/
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
        /// Provide feedback to inform the action processed
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
