#!/bin/bash

if [ $# -eq 0 ]; then
  echo "Please provide a value of 'create' or 'start' as the first argument."
  exit 0
elif [ $1 == "create" ]; then
  docker-machine create mongodb-manager0 
elif [ $1 == "start" ]; then
  docker-machine start mongodb-manager0 
fi


# Leaving existing swarm 
docker-machine ssh mongodb-manager0 -- "docker swarm leave --force"

# Removing network 
docker-machine ssh mongodb-manager0 -- "docker network rm mongoshard"

# Re-running certificates (IP adress might have changed)
docker-machine regenerate-certs mongodb-manager0 -f

# Re-Creating basic swarm
managerip=$(docker-machine ip mongodb-manager0)
docker-machine ssh mongodb-manager0 -- "docker swarm init --advertise-addr $managerip"

# Re-creating network 
docker-machine ssh mongodb-manager0 -- "docker network create --driver overlay mongonetwork"

echo "Creating mongoDB service..."
docker-machine ssh mongodb-manager0 -- 'docker volume create --name=mongo-db-data' # creating volume which will contain dir that mongodb data files will be written to. 
docker-machine ssh mongodb-manager0 -- "docker service create --constraint 'node.hostname==mongodb-manager0' \
                                    --network mongonetwork \
                                    --name mongodb \
                                    --restart-condition on-failure \
                                    --restart-max-attempts 5 \
                                    -p 27017:27017 \
                                    --mount type=volume,src=mongo-db-data,dst=/dump/ \
                                    mongo:3.2 \
                                    --bind_ip 0.0.0.0 \
                                    --port 27017"

##################################################
# TODO: Create benchmark DB + schema here IF START
##################################################

# rm'ing possible dangling volumecopier container
docker-machine ssh mongodb-manager0 -- 'docker rm volumecopier -f'

# creating a container that uses the current volume
docker-machine ssh mongodb-manager0 -- 'docker container run --name volumecopier --volume mongo-db-data:/dump/ -d centos:latest sleep infinity'

# then, copying from our local backup dir into that container's dir that is mounted to the volume, so that the volume is populated.
docker-machine ssh mongodb-manager0 -- 'docker cp /c/Users/kraaijeveld/DockerBackup/mongo-db-data/. volumecopier:/dump/' 

# removing the volume carrying container now that it's work is done
docker-machine ssh mongodb-manager0 -- 'docker rm volumecopier -f'

# using mongorestore, restoring the dump that we just received from our local disk to the mongoDB instance.
docker-machine ssh mongodb-manager0 -- 'docker exec \
                                              $(docker ps --format "{{.Names}}" | grep mongodb) \
                                              mongorestore --dir=/dump/ --oplogReplay'