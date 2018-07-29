#include <ESP8266WiFi.h>
#include <WiFiUdp.h>
#include <OneWire.h>
#include <DallasTemperature.h>

#define ONE_WIRE_BUS 5 //D1
#define NOPACKET 0
#define HELLO 1
#define TEMPERATURE 2

const char* ssid="SSID"; //type your SSID here
const char* password = "password"; //Type your Password here
const String strHello = "Hello";
const unsigned int localUdpPort = 7210; 
const unsigned int remoteUdpPort = 51000;
const IPAddress RemoteIP = IPAddress(192,168,0,175);

WiFiUDP Udp;
OneWire oneWire(ONE_WIRE_BUS);
char response[80];  // a reply string to send back
String strMessage;


// Pass our oneWire reference to Dallas Temperature. 
DallasTemperature DS18B20(&oneWire);
char temperatureCString[7];
char temperatureFString[7];

void getTemperature() 
{
  float tempC;
  float tempF;
  do 
  {
    DS18B20.requestTemperatures(); 
    tempC = DS18B20.getTempCByIndex(0);
    dtostrf(tempC, 2, 2, temperatureCString);
    tempF = DS18B20.getTempFByIndex(0);
    dtostrf(tempF, 3, 2, temperatureFString);
    delay(100);
  } while (tempC == 85.0 || tempC == (-127.0));
    
    Serial.print("Temperature in C: ");
    Serial.print(temperatureCString);
    Serial.print(" deg C ");
    Serial.println();
    Serial.print("Temperature in F: ");
    Serial.print(temperatureFString);
    Serial.print("deg F");
    Serial.println();
    Serial.printf("Remote IP Address: %s on port: %d\n", Udp.remoteIP().toString().c_str(), Udp.remotePort());
}

void setup() 
{
  Serial.begin(115200);
  Serial.println();
  DS18B20.begin(); 
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
    getTemperature();
    memset(response,'\0',sizeof(response));
    strMessage = " temperature is: ";
    strMessage += temperatureCString ;
    strMessage += " deg C or ";
    strMessage += temperatureFString;
    strMessage += " deg F";
    strMessage.toCharArray(response, strMessage.length()+1);
    Serial.println(response);
    Udp.beginPacket(RemoteIP, remoteUdpPort);
    Udp.write(response);
    Udp.endPacket(); 
    delay(15000); 
}

