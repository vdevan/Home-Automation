# Home-Automation
This is a concept for Home automation. While there are many Home automation projects in the web, this project takes a different approach - integrating all the automation devices into one platform.
The User interface used is simple HTML5 with JQuery. This allows the user to control all the devices through a simple HTML page that can be delivered by any media - Phone/Tablet/Desktop or any OS - Windows, MacOS or Linux.
By taking the web approach for user interface, the project opens up to a pleathora of controllability. For instance usage of MQ services that let you to control anywhere in the world or Alexa/Google or similar voice controlled device to work with the devices. These are not currently added yet, but in the next few projects, I will list how to add these services. Due to the modular construction any addition can be treated separately.
Fully modular approach. Adding a new device or adding a new room with new devices is very simple. Just add the device list to the Configuration Sheet and you are ready to go. No other changes are required.
For under $100, a fully functional Home automation that can record temperature in your room to provide an input for buying AC or simple button to turn on /off intruder alert which can send an Email to you, when an intruder is detected. Have a Curtain controlled remotely or turn-on/off lights from your tablet. All of these are currently possible and the entire device construction with a working module are documented in Hackster.io.
Raspberry Pi, Windows IoT and Node MCU consisting of ESP8266 are the major components. For Temperature sensor a Dallas Temperature Sensor is used for accurate reading and normal off the shelf PIR are used for Intruder detect. For Curtain motor control to get an idea of where the curtain is at, a simple Ultrasonic sensor is used, tied to curtain to provide the feedback of the distance from the window ledge to the current curtain position.

A simple but efficient and expandable concept with a working model. If you like it feel free to browse the code and comment.

Thanks to:
