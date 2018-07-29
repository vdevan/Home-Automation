//Simple program to control LED ON / OFF through Web
// Written for 8266

#include <ESP8266WiFi.h>
#include <WiFiUdp.h>

/**** Note the Motor terminal port mapping
Terminal Pins GPIO  Arduino Desc
A-        1   5     D1      PWMA
A+        3   0     D3      Direction
B-        2   4     D2      PWMB
B+        4   2     D4      Direction


//Set PWM frequency 500, default is 1000
//Set range 0~100, default is 0~1023
analogWriteFreq(500);
analogWriteRange(100);
*****/

const char* ssid="RMHome";
const char* password = "paged0wn";
const String strBright = "Brightness Value=";

WiFiUDP Udp;

int PWMA=0; //Light +ve 
int DA=5;   //Light -ve 
unsigned int localUdpPort = 5210;  // local port to listen on
String strMessage;  // buffer for incoming packets
char response[40];  // a reply string to send back
int Message;


//On Board LED Dim
void LEDDim(int value)
{
  pinMode(PWMA, OUTPUT);
  analogWrite(PWMA, value);
}

int CheckUDP()
{
  int packetSize = Udp.parsePacket();
  if (packetSize)
  { 
    strMessage = Udp.readString();
    Serial.println(strMessage);

    int index = strMessage.indexOf(strBright);
    if (index < 0)
      return 0;

    if (strMessage == strBright)
      return 0;   

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

  //Set the range for Analogue (PWM) 
  analogWriteRange(101); //PWM: 0~100
  analogWriteFreq(2000);

  //Turn all LED Off initially
  digitalWrite(PWMA,LOW);
  digitalWrite(DA,LOW);//Set initial brightness to zero (turned off)

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
        pinMode(PWMA,OUTPUT);
        digitalWrite(PWMA,LOW);
    }
    else
    {
      LEDDim(Message);
    }
  }
}

