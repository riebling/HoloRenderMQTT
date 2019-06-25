#!/bin/bash
#
# filter.sh
#
# usage: pipe the output of OpenPose or a captured file that
# prints out the 3d pose data (User ID, 24 points)
# for now the 3rd dimension is the confidence value of each point
#
# result: reformats as an MQTT message consisting of comma separated:
# person ID, 24 triples of x,y,z data, and a string "on" or "off"
sed -e "s/Person /Person-/" | sed -e "s/ /,/g" | tr -d "\n" | sed -e "s/Person-./\\n&,/g" | grep . | \
{
while read line; do
    mosquitto_pub -h oz.andrew.cmu.edu -t /topic/skeleton -m "$line"on
done
}
