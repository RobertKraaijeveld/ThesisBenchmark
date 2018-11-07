#!/bin/bash

if [ $# -eq 0 ]; then
  echo "Please provide a value of 'unscaled' or 'scaled' as the first argument."
  exit 0
elif [ $1 == "unscaled" ]; then
    declare -a volumes=(mongodb-db-data )
    declare -a machines=(mongodb-manager0)
    declare -a servicenames=(mongodb)

    for (( index=0; index<1; index++ )) do
        echo "Going to backup service ${servicenames[$index]}"

        # removing previous backup on disk
        docker-machine ssh ${machines[$index]} -- 'rm -rf /c/Users/kraaijeveld/DockerBackup/'"${volumes[$index]}"'/*;'

        # removing previous instance of volumecopier if any
        docker-machine ssh ${machines[$index]} -- 'docker rm volumecopier;'

        docker-machine ssh ${machines[$index]} -- 'docker exec \
                                        $(docker ps --format "{{.Names}}" | grep '"${servicenames[$index]}"') \
                                        mongodump' # standard :27017 port 

        # creating a container which uses the dump volume. Then, copying the contents of the dump volume from the container to the local disk
        docker-machine ssh ${machines[$index]} -- 'docker container create --name volumecopier --volumes-from $(docker ps --format "{{.Names}}" | grep '"${servicenames[$index]}"') hello-world;'
        docker-machine ssh ${machines[$index]} -- 'docker cp volumecopier:/dump/. /c/Users/kraaijeveld/DockerBackup/'"${volumes[$index]}"';'

        docker-machine ssh ${machines[$index]} -- 'docker rm volumecopier;'

        docker-machine stop ${machines[$index]}
    done
elif [ $1 == "scaled" ]; then
    declare -a volumes=(mongodb-manager1-db-data mongodb-worker1-db-shard mongodb-worker2-db-shard)
    declare -a machines=(mongodb-manager1 mongodb-worker1 mongodb-worker2)
    declare -a servicenames=(mongocfg mongosh1 mongosh2)
    
    # Stopping the balancer so no data is moved during backup
    docker-machine ssh mongodb-manager1 -- 'docker exec \
                                            $(docker ps --format "{{.Names}}" | grep mongos) \
                                            mongo --eval "sh.stopBalancer();"'

    for (( index=0; index<3; index++ )) do
        echo "Going to backup service ${servicenames[$index]}"

        # removing previous backup on disk
        docker-machine ssh ${machines[$index]} -- 'rm -rf /c/Users/kraaijeveld/DockerBackup/'"${volumes[$index]}"'/*;'

        # removing previous instance of volumecopier if any
        docker-machine ssh ${machines[$index]} -- 'docker rm volumecopier;'

        if [ ${servicenames[$index]} == "mongocfg" ]; then 
            docker-machine ssh mongodb-manager1 -- 'docker exec \
                                            $(docker ps --format "{{.Names}}" | grep '"${servicenames[$index]}"') \
                                            mongodump --host=127.0.0.1:27019 --oplog' # conf server runs at port 27019, needs to be specified explicitly to mongodump
        else # shard
            docker-machine ssh ${machines[$index]} -- 'docker exec \
                                            $(docker ps --format "{{.Names}}" | grep '"${servicenames[$index]}"') \
                                            mongodump --oplog' # standard :27017 port 
        fi

        # creating a container which uses the dump volume. Then, copying the contents of the dump volume from the container to the local disk
        docker-machine ssh ${machines[$index]} -- 'docker container create --name volumecopier --volumes-from $(docker ps --format "{{.Names}}" | grep '"${servicenames[$index]}"') hello-world;'
        docker-machine ssh ${machines[$index]} -- 'docker cp volumecopier:/dump/. /c/Users/kraaijeveld/DockerBackup/'"${volumes[$index]}"';'

        docker-machine ssh ${machines[$index]} -- 'docker rm volumecopier;'
    done

    docker-machine ssh mongodb-manager1 -- 'docker exec \
                                            $(docker ps --format "{{.Names}}" | grep mongos) \
                                            mongo --eval "sh.setBalancerState(true);"' # restarts balancer
    docker-machine stop ${machines[$index]}
fi
