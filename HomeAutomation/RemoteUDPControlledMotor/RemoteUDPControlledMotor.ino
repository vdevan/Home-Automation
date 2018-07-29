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
const String strStart = "Motor Start";
const String strStop = "Motor Stop";
const String strClock = "Motor Clock";
const String strAnticlock = "Motor Anticlock";
const String strTurns = "Motor Turns=";
const String strDist = "Get Distance";

int PWMA = D1;
int DIRA = D3;


#define NOPACKET 0
#define STOP 1
#define START 2
#define CLOCK 3
#define ACLOCK 4
#define MAXTURNS 100
#define TRIGGER D5
#define ECHO D6
#define MAX_DISTANCE 200 // Maximum distance to ping for (cm). Up to ~450cm

//Rotation turns are above 10. 11 indicates 1 turn. 

WiFiUDP Udp;
unsigned int localUdpPort = 4210;  // local port to listen on
String strMessage;  // buffer for incoming packets
char response[40];  // a reply string to send back
int Message;
int Turns = 1;

int STEPS = 513;
int Blue = 5;
int Pink =  4; 
int Yellow = 0; 
int Orange = 2;
bool bClock = false;
unsigned long timeDelay = 1; // in millisecs < 1 motor driver will not drive

void sequence(bool a, bool b, bool c, bool d){  /* four step sequence to stepper motor */
  digitalWrite(Orange, a);
  digitalWrite(Yellow, b);
  digitalWrite(Pink, c);
  digitalWrite(Blue, d);
  delay(timeDelay);
}

//Get Distance of the curtain 
long GetDistance()
{
  unsigned int pings = 0;

  NewPing sonar(TRIGGER, ECHO, MAX_DISTANCE);

  long pingTime = sonar.ping_median();
  
  //unsigned int pingTime = pings / 5;
  //Check the distance required to start / Stop the Motor
  //Serial.println(sonar.convert_cm(pingTime));

  return sonar.convert_cm(pingTime);
  /*long duration;
  long distance;
  digitalWrite(TRIGGER, LOW);
  digitalWrite(ECHO, LOW);
  delayMicroseconds(2);
  digitalWrite(TRIGGER, HIGH);
  delayMicroseconds(15);
  digitalWrite(TRIGGER, LOW);
  duration = pulseIn(ECHO, HIGH);
  distance = (duration/2) / 29.1;
  return distance;*/
  
 /* String str = "Distance: ";
  str += distance;
  str += " cm";
  Serial.println(str);
  */
  

}

//Motor Rotate
void StepMotorForward()
{
  for (int j =0; j < Turns; j++)
  {
    for(int i = 0; i<STEPS; i++)
    {     
      //0  0 0 1
      //0 0 1 1
      //0 0 1 0
      //0 1 1 0
      //0 1 0 0
      //1 1 0 0
      //1 0 0 0
      //1 0 0 1

      sequence(LOW, LOW, LOW, HIGH);  
      sequence(LOW, LOW, HIGH, HIGH); 
      sequence(LOW, LOW, HIGH, LOW);  
      sequence(LOW, HIGH, HIGH, LOW); 
      sequence(LOW, HIGH, LOW, LOW); 
      sequence(HIGH, HIGH, LOW, LOW); 
      sequence(HIGH, LOW, LOW, LOW);  
      sequence(HIGH, LOW, LOW, HIGH);
      GetDistance();
      
      if (CheckUDP() == STOP)
      {
        memset(response,'\0',sizeof(response));
        String str =  "Motor Stopped at: " ;
        str += j+1 ;      
        str.toCharArray(response, str.length()+1);
        //Serial.println(response);
        Serial.println(str);
  
        // send back a reply, to the IP address and port we got the packet from
        Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
        Udp.write(response);
        Udp.endPacket();
        i = STEPS;
        j = Turns;
      }
    }


  }
  sequence(LOW, LOW, LOW, LOW);
  
}

void StepMotorReverse()
{

  for (int j =0; j < Turns; j++)
  {
    for(int i = 0; i<STEPS; i++)
    {
      //1  0 0 1
      //1 0 0 0
      //1 1 0 0
      //0 1 0 0
      //0 1 1 0
      //0 0 1 0
      //0 0 1 1
      //0 0 0 1
      sequence(HIGH, LOW, LOW, HIGH);  
      sequence(HIGH, LOW, LOW, LOW); 
      sequence(HIGH, HIGH, LOW, LOW); 
      sequence(LOW, HIGH, LOW, LOW); 
      sequence(LOW, HIGH, HIGH, LOW); 
      sequence(LOW, LOW, HIGH, LOW); 
      sequence(LOW, LOW, HIGH, HIGH);  
      sequence(LOW, LOW, LOW, HIGH);

      GetDistance();
 
      if (CheckUDP() == STOP)
      {
        memset(response,'\0',sizeof(response));
        String str =  "Motor Stopped at: " ;
        str += j+1 ;      
        str.toCharArray(response, str.length()+1);
        //Serial.println(response);
        Serial.println(str);
  
        // send back a reply, to the IP address and port we got the packet from
        Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
        Udp.write(response);
        Udp.endPacket();
        j = Turns;
        i=STEPS;
      }

    }

  }
  sequence(LOW, LOW, LOW, LOW);
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
    }
    
    if (strMessage == strStop)
      return STOP;
      
    if (strMessage == strStart)
      return START;
      
    if (strMessage == strClock)
    {
      bClock = true;
      return CLOCK;
    }

    if (strMessage == strAnticlock)
    {
      bClock = false;
      return ACLOCK;
    }

    int index = strMessage.indexOf(strTurns);
    if (index < 0)
      return STOP;

    index = strMessage.substring(index + strTurns.length()).toInt(); 
      
    Serial.print("Computed Rotation: ");
    Serial.println(index); 

    if (index <= 0)
      return STOP;

    if (index < 10)
    {
      Turns = 1;
      return 11;
    }

    if (index > MAXTURNS + 10)
    {
      Turns = MAXTURNS;
      return MAXTURNS + 10;  
    }

    Turns = index - 10;  
    return index ;

  }
  return NOPACKET;
}



void setup()
{
  Serial.begin(115200);
  Serial.println();
 // declaring the four pins to be outputs
  //pinMode(Blue, OUTPUT);
  //pinMode(Pink, OUTPUT);
 // pinMode(Yellow, OUTPUT);
 // pinMode(Orange, OUTPUT);
 //pinMode(PWMA,OUTPUT);
  pinMode(PWMA, OUTPUT);
  pinMode(DIRA,OUTPUT);
  analogWriteRange(101); //PWM: 0~100
  analogWriteFreq(1000);
  digitalWrite(PWMA,LOW);
  
  pinMode(TRIGGER, OUTPUT);
  pinMode(ECHO, INPUT);
 
  // setting the inital state of the electromagnets for Low power consumption
  //digitalWrite(Blue, LOW);
  //digitalWrite(Pink, LOW);
  //digitalWrite(Yellow, LOW);
  //digitalWrite(Orange, LOW);
  digitalWrite(TRIGGER, LOW);
  delay(10); // a small time delay to allow the motor to settle down

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
  //GetDistance();
  //delay (2000);
  
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
    
    if (Message == START || Message > 10)
    {
      Serial.print("Rotation Request: "); 
      Serial.println(Turns);
      if (bClock)
      {
         Serial.println("Forward");
        digitalWrite(DIRA,HIGH);
        analogWrite(PWMA,101);
        delay(15000);
        digitalWrite(PWMA,LOW);
        digitalWrite(DIRA,LOW);
        
        //StepMotorForward();
      }
      else
      {
        Serial.println("Reverse");
        digitalWrite(DIRA,LOW);
        analogWrite(PWMA,101);
        delay(15000);
        digitalWrite(PWMA,LOW);
        digitalWrite(DIRA,LOW);
        //StepMotorReverse();
      }
    }

  }
}


