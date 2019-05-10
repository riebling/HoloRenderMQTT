# mover.py
#
# send x,y,z move coordinates like "1,2,1" to a UDP port (on hololens)

import socket,time,random

# CMU_DEVICES
hololens_ip="172.26.197.103"
# ARENA
#hololens_ip="192.168.1.239"

FRAMERATE=10

framerate=1/FRAMERATE # sleep seconds 'frame rate'
incr=0.2
port1=9005
port2=9006
x=0.0
y=0.0
z=0.0
MESSAGE=""

sock = socket.socket(socket.AF_INET, # Internet
                     socket.SOCK_DGRAM) # UDP

def randmove():
    rando=random.random()
    if rando < 0.33:
        num=incr
    elif rando < 0.66:
        num=-incr
    else:
        num=0
    return num

Xaway = True
Zaway = True

while True:
    MESSAGE=str(x)+','+str(y)+','+str(z)
    print("Message: " + MESSAGE)
    sock.sendto(MESSAGE, (hololens_ip, port2))
    if (x > 60):
        Xaway = False;
    elif (x < 0):
        Xaway = True;

    if Xaway:
        x+=randmove()
    else:
        x-=randmove()

    if (z > 60):
        Xaway = False;
    elif (z < 0):
        Xaway = True;

    if Zaway:
        z+=randmove()
    else:
        z-=randmove()
    
    time.sleep(framerate)




