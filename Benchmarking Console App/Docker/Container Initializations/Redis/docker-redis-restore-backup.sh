#!/bin/bash
if [ $# -eq 0 ]; then
  echo "Please provide a value of 'unscaled' or 'scaled' as the first argument."
  exit 0
elif [ $1 == "unscaled" ]; then
    declare -a volumes=(redis-manager0-volume)
    declare -a machines=(redis-manager0)
    declare -a servicenames=(redis-m-7001)
    declare -a ports=(7001)
    nodesCount=1
elif [ $1 == "scaled" ]; then
    declare -a volumes=(redis-manager1-volume redis-worker2-volume redis-worker2-volume)
    declare -a machines=(redis-manager1 redis-worker1 redis-worker2)
    declare -a servicenames=(redis-cluster-m-7002 redis-cluster-m-7003 redis-cluster-m-7001)
    declare -a ports=(7002 7003 7001)
    nodesCount=3
fi

for (( index=0; index<$nodesCount; index++ )) do
    # rm'ing possible dangling volumecopier container
    docker-machine ssh ${machines[$index]} -- 'docker rm volumecopier -f'

    # creating a container that uses that volume
    docker-machine ssh ${machines[$index]} -- 'docker container run --name volumecopier --volume '"${volumes[$index]}"':/redis/data/ -d centos:latest sleep infinity'


    # then, copying from our backup dir into that container's dir that is mounted to the volume, so that the volume is populated.
    docker-machine ssh ${machines[$index]} -- 'docker cp /c/Users/kraaijeveld/DockerBackup/'"${volumes[$index]}"'/dump.rdb volumecopier:/redis/data/' 
    docker-machine ssh ${machines[$index]} -- 'docker exec volumecopier chmod 777 /redis/data/dump.rdb'

    # removing the volume carrying container, the volume lives on
    docker-machine ssh ${machines[$index]} -- 'docker rm volumecopier -f'
done

