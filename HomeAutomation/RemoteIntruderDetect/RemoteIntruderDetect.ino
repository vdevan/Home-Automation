
#include <ESP8266WiFi.h>
#include <WiFiUdp.h>


#define SEND D1
#define RECV D2
#define TRANSMIT D5


// NodeMCU Pin D1 > TRIGGER | Pin D2 > ECHO
const char* ssid="SSID"; //type your SSID here
const char* password = "password"; //Type your Password here
const String strStart = "Start Monitor";
const String strStop = "Stop Monitor";
const String strInt = "Intruder Detected in Garage";
const String strAlive = "Monitoring Premises";
const unsigned int RemoteUdp = 51000;
const IPAddress RemoteIP = IPAddress(192,168,0,175);

bool bMonitor;
int timerCount;

WiFiUDP Udp;

unsigned int localUdpPort = 8210;  // local port to listen on
String strMessage;  // buffer for incoming packets
char response[40];  // a reply string to send back


void CheckUDP()
{

  int packetSize = Udp.parsePacket();
  if (packetSize)
  { 
    strMessage = Udp.readString();
    Serial.println(strMessage);

    int index = strMessage.indexOf(strStart);
    if (index < 0)
    {
      index = strMessage.indexOf(strStop);
      if (index <0)
        return;
      else
      {
        Serial.println("Stopped Monitoring");
        bMonitor = false;
        timerCount = 30;
        return;
      }
    }

    //For testing only
    //RemoteIP = Udp.remoteIP();
    //RemoteUdp = Udp.remotePort();
    
    bMonitor = true;
    timerCount = 30;
    Serial.println("Started Monitoring");
  }
}

void setup() 
{
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

  pinMode(SEND, OUTPUT);
  pinMode(RECV, INPUT);

  //Transmit pulse setup
  pinMode(TRANSMIT,OUTPUT);
  
  digitalWrite(SEND, HIGH);
  bMonitor = false;
  timerCount = 30;
}

void loop() 
{  
  if (timerCount <=0 && bMonitor)
  {
    //Send Alive Pulse
    
    Udp.beginPacket(RemoteIP, RemoteUdp);
    strAlive.toCharArray(response,strAlive.length()+1);
    Udp.write(response);
    Udp.endPacket();
    timerCount = 30;
    Serial.println(strAlive);
  }
  CheckUDP();
 
  if (digitalRead(RECV)==0 && bMonitor)
  {
    Udp.beginPacket(RemoteIP, RemoteUdp);
    strInt.toCharArray(response,strInt.length()+1);
    Udp.write(response);
    Udp.endPacket();
    Serial.println("Intruder Detected");
  }

  //For testing Transmit 10 4K pulse. That should do.
  for (int i=0;i<10; i++)
  {
    pinMode(TRANSMIT,HIGH);
    delayMicroseconds(125); 
    pinMode(TRANSMIT,LOW);
    delayMicroseconds(125);
    
  }
  
  delay(10000);
  if (bMonitor)
    timerCount --;
}

