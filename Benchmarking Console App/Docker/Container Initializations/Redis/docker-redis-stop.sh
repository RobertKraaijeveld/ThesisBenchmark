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
    echo "Going to back-up service ${servicenames[$index]}"

    # removing previous backup on disk
    docker-machine ssh ${machines[$index]} -- 'rm -rf /c/Users/kraaijeveld/DockerBackup/'"${volumes[$index]}"'/*;'

    # removing previous instance of volumecopier if any
    docker-machine ssh ${machines[$index]} -- 'docker rm volumecopier;'
    
    # telling redis to dump its data 
    docker-machine ssh ${machines[$index]} -- 'docker exec \
                                               $(docker ps --format "{{.Names}}" | grep '"${servicenames[$index]}"') \
                                               redis-cli -c -p '${ports[$index]}' SAVE'

    # creating a container which uses the dump volume. Then, copying the contents of the dump volume, namely, the dump.rdb file, from the container to the local disk
    docker-machine ssh ${machines[$index]} -- 'docker container create --name volumecopier --volumes-from $(docker ps --format "{{.Names}}" | grep '"${servicenames[$index]}"') hello-world;'
    docker-machine ssh ${machines[$index]} -- 'docker cp volumecopier:/redis/data/dump.rdb /c/Users/kraaijeveld/DockerBackup/'"${volumes[$index]}"';'

    docker-machine ssh ${machines[$index]} -- 'docker rm volumecopier;'
    
    docker-machine stop ${machines[$index]}
done