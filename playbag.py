# mover.py
#
# send x,y,z move coordinates like "1,2,1" to a UDP port (on hololens)

import socket,threading,SocketServer,time,random,os,sys,rosbag

# CMU_DEVICES
hololens_ip="172.26.197.103"
# ARENA
#hololens_ip="192.168.1.239"

OBJECT=sys.argv[1]

FRAMERATE=10
framerate=1/FRAMERATE # sleep seconds 'frame rate'
incr=0.1 # max incremental distance to move per frame

TOPIC="/topic/render"
# UDP send ports (deprecated)
port1=9005
port2=9006
# listen
HOST, PORT = "128.237.154.148", 9007

x=0.0
y=0.0
z=0.0
MESSAGE=""

bag = rosbag.Bag('bagfile_conix3.bag')

# goofy random walk algorithm I made up :)
def randmove():
    rando=random.random()
    if rando < 0.33:
        num=incr
    elif rando < 0.66:
        num=-incr
    else:
        num=0
    return num

#while True:
for topic, msg, t in bag.read_messages(topics=['/cf1/vrpn_client_node/cf1/pose']):
    themsg=str(msg)
    splits=themsg.split('\n')
    x=float(splits[8].split(' ')[5])*100
    y=float(splits[9].split(' ')[5])*100
    z=float(splits[10].split(' ')[5])*100

    MESSAGE=OBJECT+","+"{0:0.3f}".format(float(x))+','+"{0:0.3f}".format(float(y))+','+"{0:0.3f}".format(float(z))+",on"
    print(MESSAGE)

    os.system("mosquitto_pub -h oz.andrew.cmu.edu -t " + TOPIC + " -m " + MESSAGE);
    time.sleep(framerate)

    #    x+=randmove()
    #    z+=randmove()
    
bag.close()



