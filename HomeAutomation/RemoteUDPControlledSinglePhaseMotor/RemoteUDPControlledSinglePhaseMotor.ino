#include <ESP8266WiFi.h>
#include <WiFiUdp.h>
#include <NewPing.h>

/**** Note the Motor terminal port mapping
Terminal Pins GPIO  Arduino Desc
A-        1   5     D1      Red
B-        2   4     D2      Blue
A+        3   0     D3      Green
B+        4   2     D4      Black

+Vin  //Motor Input Voltage
-Vin
+Vcc  //Node MCU Input
-Vcc  
The number of steps in one revolution of 28BYJ-48 motor is 2038
*****/
/****Note on Stepper Motor ****
Stepper Motor: 28BYJ-48
Degree Rotation per Step: 5.625
Total Steps per Revolution of Motor Spindle = 360 / 5.625 = 64
Gear Ratio : 64:1
No. of turns / step sequence = 8  (Function: Clockwise)

Total Steps Per Revolution of outer Spindle = 64 * 64 / 8 = 512
The number of steps in one revolution of Outer Spindle of 28BYJ-48 motor is 512
*****/

const char* ssid = "RMHome";
const char* password = "paged0wn";

//Motor Control 
const String strOpen = "Curtain Open";
const String strStop = "Curtain Stop";
const String strClose = "Curtain Close";
const String strDist = "Get Distance";
const String strSetDist = "Set Distance=";

int PWMA = D1;
int DIRA = D3;


#define NOPACKET 0
#define STOP 1
#define OPEN 2
#define CLOSE 3
#define MIN 5  //Closed distance
#define MAX 90 //Opened distance

#define TRIGGER D5
#define ECHO D6
#define MAX_DISTANCE 200 // Maximum distance to ping for (cm). Up to ~450cm



WiFiUDP Udp;
unsigned int localUdpPort = 4210;  // local port to listen on
String strMessage;  // buffer for incoming packets
char response[40];  // a reply string to send back
int Message;
bool bOpen;
int Distance;

//Get Distance of the curtain 
long GetDistance()
{
  long pingTime = 0;
  NewPing sonar(TRIGGER, ECHO, MAX_DISTANCE);
  for (int i=0; i<5; i++)
  {
    pingTime += sonar.ping_median();
  }
  return sonar.convert_cm(pingTime/5);
}

int SetDistance()
{
  int setdistance = strMessage.substring(strSetDist.length()).toInt();
  if (setdistance == 0)
    return STOP;
  int distance = GetDistance();

  if (distance == setdistance)
    return STOP;
    
  //If the distance is more than Setdistance then close. else open
  
  //First stop the curtain motor if it is running     
  pinMode(PWMA,OUTPUT);
  analogWrite(PWMA,0);
  if (distance > setdistance)
  {
    Serial.println("Closing to reach set distance");
    while (setdistance < distance)
    {
      analogWrite(PWMA,100); //Full speed
      digitalWrite(DIRA,LOW);    
      if (CheckUDP() == STOP)
      {
        Serial.println("Set Distance Cancelled");
        return STOP;
      }
      distance = GetDistance();
    }
  }
  else
  {
    Serial.println("Opening to reach set distance");
    while (setdistance > distance)
    {
      analogWrite(PWMA,100); //Full speed
      digitalWrite(DIRA,HIGH);    
      if (CheckUDP() == STOP)
      {
        Serial.println("Set Distance Cancelled");
        return STOP;
      }
      distance = GetDistance();
    }
  }
  Serial.println("Set Distance Reached");
  pinMode(PWMA,OUTPUT);
  analogWrite(PWMA,0);
  return NOPACKET;
}

int CheckUDP()
{
  int packetSize = Udp.parsePacket();
  if (packetSize)
  { 
    strMessage = Udp.readString();
    Serial.println(strMessage);

    if (strMessage == strDist)
    {
      // send back a reply, to the IP address and port we got the packet from
      Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
      String str = (String)GetDistance() + " cm";
      str.toCharArray(response,str.length()+1);
      Udp.write(response);
      Udp.endPacket();
      return NOPACKET;
    }

    if (strMessage.indexOf(strSetDist)==0)
      return SetDistance();
    
    if (strMessage == strStop)
      return STOP;
      
    if (strMessage == strOpen)
      return OPEN;
      
    if (strMessage == strClose)
      return CLOSE;
  }
  return NOPACKET;
}



void setup()
{
  Serial.begin(115200);
  Serial.println();

  pinMode(PWMA, OUTPUT);
  pinMode(DIRA,OUTPUT);
  analogWriteRange(100); //PWM: to control speed 0~100 Not useful as the motor is very slow
  analogWriteFreq(1000); //1KHZ frequency
  digitalWrite(PWMA,LOW);
  digitalWrite(DIRA,LOW); 
  bOpen = true;
  
  pinMode(TRIGGER, OUTPUT);
  pinMode(ECHO, INPUT);
 
  digitalWrite(TRIGGER, LOW);

  Serial.printf("Connecting to %s ", ssid);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
  }
  Serial.println(" connected");

  Udp.begin(localUdpPort);
  Serial.printf("Now listening at IP %s, UDP port %d\n", WiFi.localIP().toString().c_str(), localUdpPort);
}


void loop()
{
  Message = CheckUDP(); 
  if (Message != NOPACKET)
  {
    memset(response,'\0',sizeof(response));

    Serial.print("Message Received from: ");
    Serial.print(Udp.remoteIP());
    Serial.print(" Reply Port: "); 
    Serial.println(Udp.remotePort());
    
    // send back a reply, to the IP address and port we got the packet from
    Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
    String str = "Messge Processed";
    str.toCharArray(response,str.length()+1);
    Udp.write(response);
    Udp.endPacket();
    
    if (Message ==STOP)
    {
      pinMode(PWMA,OUTPUT);
      analogWrite(PWMA,0);
    }
    else
    {     
      pinMode(PWMA, OUTPUT);
      analogWrite(PWMA,100);
      if (Message == OPEN)
      {
        Serial.println("Forward");
        digitalWrite(DIRA,HIGH);
        bOpen=true;
      }
      else
      {
        Serial.println("Reverse");
        digitalWrite(DIRA,LOW);
        bOpen=false;
      }
      
    }

  }
  Distance = GetDistance();
  if ((bOpen && Distance >= MAX)||(!bOpen && Distance <= MIN))
  {
    pinMode(PWMA,OUTPUT);
    analogWrite(PWMA,0);
    Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
    String str = "Max or Mini Reached";
    str.toCharArray(response,str.length()+1);
    Udp.write(response);
    Udp.endPacket();

  }
  
}


