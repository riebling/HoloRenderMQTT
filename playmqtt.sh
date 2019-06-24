while read line; do
    #mosquitto_pub -h oz.andrew.cmu.edu -t /topic/skeleton -m $line
    echo mosquitto_pub -h oz.andrew.cmu.edu -t /topic/skeleton -m $line
    sleep 1
done
