# Home-Automation
This is a concept for Home automation. While there are many Home automation projects in the web, this project takes a different approach - integrating all the automation devices into one platform.  The user interface used is simple HTML5 with JQuery. This allows the user to control all the devices through a simple HTML page that can be delivered by any media - Phone/Tablet/Desktop or any OS - Windows, MacOS. Android or Linux. By taking the web approach for user interface, the project opens up to a pleathora of controllability. For instance usage of MQ services that let you to control anywhere in the world or Alexa/Google or similar voice controlled device to work with the devices. Though these are not currently added yet, but I will show you how it can be done by in my next extension to this project. 
Because of modular construction any addition can be treated separately. Adding a new device or adding a new room with new devices is very simple. Just add the device list to the Configuration Sheet, recompile the program and you are ready to go. No other changes are required. For under $100, this project is a fully functional that can be used for following things: 
1.	You can record temperature in your room and outside to understand the functioning of insulation / AC
2.	Use a simple button to turn on /off intruder alert. This can then send an Email to you, when an intruder is detected. 
3.	Have a Curtain controlled remotely to open or close.
4.	Remotely Turn-on/off lights from your tablet. 

All of these are currently possible and the entire device construction with a working module with code are provided. Raspberry Pi, Windows IoT and Node MCU consisting of ESP8266 are the major components. For Temperature sensor a Dallas Temperature Sensor is used for accurate reading. Intruder alert, curtain controller and Light controller are your normal daily use device to which you can add current capability.

A simple but efficient and expandable concept with a working model. If you like it feel free to browse the code and comment.

Thanks to:

LightBuzz team for their code on sending SMTP EMail from Windows IoT.
Link: https://github.com/LightBuzz/SMTP-WinRT

NewPing Arduino Library for Arduino by Tim Eckel
Link: https://bitbucket.org/teckel12/arduino-new-ping/wiki/Home

Library for Dallas/Maxim 1-Wire Chips by Paul Stoffregen
Link: https://github.com/PaulStoffregen/OneWire

Arduino-Temperature-Control-Library by Miles Burton
Link: https://github.com/milesburton/Arduino-Temperature-Control-Library

and lastly but not least: ESP8266 Community Forum for wonderful work done by them making coding the ESP8266 a breeze
Link: https://github.com/esp8266
