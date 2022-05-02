# SnailMail
Snail mail is a project idea I had when I was craving something more old fassioned, something in the terminal yet still pleasing to look at to a degree, That was the original thought. Then I thought about how I felt weird giving my address out to people on the internet to be pen pals with so I thought "Lets make email but with, extra steps"
This is my first project with sockets programming and just networking in general so if you catch bugs, please let me know and if you have advice on how to make it better, that is also very helpful!

# Preview
<img src="https://github.com/GhostFire90/SnailMail/blob/main/newLook.gif">

# Overview
SnailMail works almost exaclty like email does except with the twist of adding tension through the means of a "delivery" system. Configurable on server side, you can control how many days it takes for the file to be "Delivered", what this means is, the recipient cannot download the file until that amount of time has passed.

SnailMail automatically uses an asymetric public key encryption algorithm to ecrypt your files so noone except the planned recipient can view them (public keys are stored on the server and then downloaded by sender, while private keys are saved on recipient's computer and used to decrypt, private keys are never transmitted)

[Terminal.Gui by Miguel Deicaza](https://github.com/migueldeicaza/gui.cs) brings it the older, DOS-like UI that I wanted to use for this project 

# Usage 
Both the server and client automatically generate a config file with the neccesary entries
 - ip is the ip the client will connect to or the server will bind to
 - port is the port the client will connect to or the server will bind to
 - days is the amount of days the server will delay the delivery by


# Issues
- recipients not having connected to the server yet is currently unhandled and will cause a crash on server end, this is because the public key requested by the client will not exist, the plan is to add a system to instead send the file un-encrypted
- client hasnt been setup to handle the server closing yet


# Todo
 - fix previous issues
 - add a progress bar to the send so the program doesnt just freeze until the full file is sent

# Credits
  - Binaries used
    - [Terminal.Gui](https://github.com/migueldeicaza/gui.cs) 
    - [NewtonSoft.Json](https://github.com/JamesNK/Newtonsoft.Json) 
