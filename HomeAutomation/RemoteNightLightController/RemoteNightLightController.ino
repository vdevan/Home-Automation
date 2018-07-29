//Simple program to control LED ON / OFF through Web
// Written for 8266

#include <ESP8266WiFi.h>
#include <WiFiUdp.h>

const char* ssid="SSID"; //type your SSID here
const char* password = "password"; //Type your Password here
const String strBright = "Brightness Value=";
const String strNight = "Brightness Night Value=";
const String strGetBright = "Get Brightness";
const String strGetNightBright = "Get Night Brightness";

WiFiUDP Udp;

int PWMA=D3;  //Light +ve 
int PWMB=D2;  //Night Light +ve
int DA=D1;    //Light -ve 
int DB=D5;    //Night Light -ve
unsigned int localUdpPort = 5210;  // local port to listen on
String strMessage;  // buffer for incoming packets
char response[40];  // a reply string to send back
int Message;
bool bNight;
int nightValue;
int brightValue;


//On Board LED Dim
void LEDDim(int value)
{
  if (bNight)
  {
    pinMode(PWMB, OUTPUT);
    analogWrite(PWMB, value);
    nightValue =value;

  }
  else
  {
    Serial.printf("Setting Value: %d\n",value);
    pinMode(PWMA, OUTPUT);
    analogWrite(PWMA, value);   
    brightValue = value; 
  }
}

void SendMessage(bool bNightValue)
{
    Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
    String str;
    if (bNightValue)
      str = nightValue;    
    else
      str = brightValue;
        
    str.toCharArray(response,str.length()+1);
    Udp.write(response);
    Udp.endPacket();
}

int CheckUDP()
{
  int packetSize = Udp.parsePacket();
  bNight=false;
  if (packetSize)
  { 
    int index;
    strMessage = Udp.readString();
    Serial.println(strMessage);

    
    if (strMessage.indexOf("Get") ==0)
    {
      if (strMessage == strGetBright)
        SendMessage(false);
      else
      {
        if (strMessage == strGetNightBright)
          SendMessage(true);
      }
      return 0;
    }

    index = strMessage.indexOf(strNight);
    if (index < 0)
    {
      index = strMessage.indexOf(strBright);
      if (index <0)
        return 0;
    }
    else
      bNight=true;
     

    if (strMessage == strBright) //No value provided
      return 0;   

    if (strMessage == strNight) ////No value provided
      return 0;

    if (bNight)
      index = strMessage.substring(index + strNight.length()).toInt(); 
    else
      index = strMessage.substring(index + strBright.length()).toInt(); 
      
    Serial.printf("Computed Brightness: %d\n",index);
  
    if (index <= 0)
      return 0;

    if (index > 100)
      return 100;

    return index ;
  }
  return -1;
}


void setup() 
{ 
  //Set ports Output Note: LED_BUITIN is a built in LED
  pinMode(PWMA,OUTPUT);
  pinMode(DA,OUTPUT);
  pinMode(PWMB,OUTPUT);
  pinMode(DB,OUTPUT);
  //Set the range for Analogue (PWM) 
  analogWriteRange(101); //PWM: 0~100
  analogWriteFreq(1000); //1KHz
  
  //Turn all LED Off initially
  digitalWrite(PWMA,LOW);
  digitalWrite(PWMB,LOW);
  digitalWrite(DA,LOW);//Set initial brightness to zero (turned off)
  digitalWrite(DB,LOW);
  nightValue =0;
  brightValue =0;
  
  Serial.begin(115200);
  Serial.println();
  Serial.print("Wifi connecting to ");
  Serial.println( ssid );

  WiFi.begin(ssid,password);

  Serial.println();
  Serial.print("Connecting");

  while( WiFi.status() != WL_CONNECTED )
  {
    delay(500);
    Serial.print(".");        
  }

  Serial.println();
  Serial.println("Wifi Connected Success!");
  Serial.print("NodeMCU IP Address : ");
  Serial.println(WiFi.localIP() );

  Udp.begin(localUdpPort);
  Serial.printf("Now listening at IP %s, UDP port %d\n", WiFi.localIP().toString().c_str(), localUdpPort);

}

void loop() 
{
  // put your main code here, to run repeatedly:
  Message = CheckUDP();
  if (Message >=0 )
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
 
    if (Message == 0) //Turn off the light
    {
      if (bNight)
      {
        pinMode(PWMB,OUTPUT);
        digitalWrite(PWMB,LOW);
        nightValue =0;
      }
      else
      {
        pinMode(PWMA,OUTPUT);
        digitalWrite(PWMA,LOW);
        brightValue =0;
      }
    }
    else
    {
      LEDDim(Message);
    }
  }
}

