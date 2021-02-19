# SnailMail
Snail mail is a project idea I had when I was craving something more old fassioned, something in the terminal yet still pleasing to look at to a degree, That was the original thought. Then I thought about how I felt weird giving my address out to people on the internet to be pen pals with so I thought "Lets make email but with, extra steps"
This is my first project with sockets programming and just networking in general so if you catch bugs, please let me know and if you have advice on how to make it better, that is also very helpful!

# Preview
<img src="https://github.com/GhostFire90/SnailMail/blob/main/2021-01-13%2017-53-09.gif">

# Overview
Basically, this sends files but with a delay that will eventually be configurable on the servers end. Adds a bit more importance to your messages as the person on the other end will not be able to read it for a few days.

# Usage 
  - server is pretty self explanitory, port used atm is 9000, auto binds to 0.0.0.0, enter to quit
  - Client will automatically connect to 127.0.0.1 aka Localhost if not provided with a valid ip in the args ```./SnailMailClient.exe <Server ip here>```
  - Client is currently unavaliable with linux
  - Requires .net core 3.1

# Issues

  - Random binaryFormatter fails over network instead of local 

# Todo
 
  - ~~Add config for certain things on both server and client side, like default server ip and changing of port along with time between file recieve ~~
    - Added port options for server and delay
    - added default ip and port for client
  - Add client linux support

# Credits
  - Binaries used
    - [Terminal.Gui](https://github.com/migueldeicaza/gui.cs) 
    - [NewtonSoft.Json](https://github.com/JamesNK/Newtonsoft.Json) 
