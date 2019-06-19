# playbag.py
#
# usage: python playbag.py <name of object e.g. "drone">
#
# send x,y,z,xrot,yrot,zrot,wrot move coordinates like "1,2,1" to a MQTT topic

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

lasttime=float(0)
firstsec=0

# return the value after the colon
def parse(s):
    return  s.replace(" ","").split(':')[1]


#while True:
for topic, msg, t in bag.read_messages(topics=['/cf1/vrpn_client_node/cf1/pose']):
    # sample line
    # 0  header: 
    # 1  seq: 4
    # 2  stamp: 
    # 3    secs: 1559582959
    # 4    nsecs: 186421857
    # 5  frame_id: "/world"
    # 6 pose: 
    # 7  position: 
    # 8    x: 0.00124780554324
    # 9    y: 0.014096390456
    # 10    z: 0.0175174046308
    # 11  orientation: 
    # 12    x: -0.0253387540579
    # 13    y: -0.014080057852
    # 14    z: 0.999438405037
    # 15    w: 0.0168110821396

    # each msg string contains multiple lines
    themsg=str(msg)
    splits=themsg.split('\n')

    secs=int(parse(splits[3]))
    if firstsec==0:
        firstsec=secs
    secs=secs-firstsec

    nsecs=int(parse(splits[4]))
    nowtime=float(secs)+float(nsecs)/float(1000000000)

    # Position get the Fifth Element(!)
    x=float(parse(splits[8]))
    y=float(parse(splits[9]))
    z=float(parse(splits[10]))
    # Orientation
    rotX=float(parse(splits[12]))
    rotY=float(parse(splits[13]))
    rotZ=float(parse(splits[14]))
    rotW=float(parse(splits[15]))

    # count frames per sec
    # first time: initialize
    sleeptime=float(0)
    if lasttime==float(0):
        lasttime=nowtime
    else:
        sleeptime=nowtime-lasttime
        lasttime=nowtime

    MESSAGE=OBJECT+","+"{0:0.3f}".format(x)+','+"{0:0.3f}".format(y)+','+"{0:0.3f}".format(z)+','+\
    "{0:0.3f}".format(rotX)+','+"{0:0.3f}".format(rotY)+','+"{0:0.3f}".format(rotZ)+','+"{0:0.3f}".format(rotW)+",on"
    print(MESSAGE)

    os.system("mosquitto_pub -h oz.andrew.cmu.edu -t " + TOPIC + " -m " + MESSAGE);

    time.sleep(sleeptime)

bag.close()
