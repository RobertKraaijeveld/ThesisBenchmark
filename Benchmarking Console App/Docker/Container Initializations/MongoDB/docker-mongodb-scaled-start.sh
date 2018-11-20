#!/bin/bash

if [ $# -eq 0 ]; then
  echo "Please provide a value of 'create' or 'start' as the first argument."
  exit 0
elif [ $1 == "create" ]; then
  docker-machine rm mongodb-manager1 -f
  docker-machine rm mongodb-worker1 -f
  docker-machine rm mongodb-worker2 -f

  docker-machine create mongodb-manager1 
  docker-machine create mongodb-worker1 
  docker-machine create mongodb-worker2 
elif [ $1 == "start" ]; then
  docker-machine start mongodb-manager1 
  docker-machine start mongodb-worker1 
  docker-machine start mongodb-worker2 
fi

# Leaving existing swarm 
docker-machine ssh mongodb-worker2 -- "docker swarm leave --force"
docker-machine ssh mongodb-worker1 -- "docker swarm leave --force"
docker-machine ssh mongodb-manager1 -- "docker swarm leave --force"

# Removing network 
docker-machine ssh mongodb-manager1 -- "docker network rm mongoshard"

# Re-running certificates (IP adress might have changed)
docker-machine regenerate-certs mongodb-worker2 -f
docker-machine regenerate-certs mongodb-worker1 -f
docker-machine regenerate-certs mongodb-manager1 -f

# Re-Creating basic swarm
managerip=$(docker-machine ip mongodb-manager1)
docker-machine ssh mongodb-manager1 -- "docker swarm init --advertise-addr $managerip"
jointoken=$(docker-machine ssh mongodb-manager1 -- "docker swarm join-token worker --quiet")
docker-machine ssh mongodb-worker1 -- "docker swarm join --token $jointoken $managerip:2377"
docker-machine ssh mongodb-worker2 -- "docker swarm join --token $jointoken $managerip:2377"

# Re-creating network 
docker-machine ssh mongodb-manager1 -- "docker network create --driver overlay mongoshard"


echo "Creating mongocfg service..."
docker-machine ssh mongodb-manager1 -- 'docker volume create --name=mongodb-manager1-db-data' # creating volume which will contain dir that mongodb data files will be written to. 
docker-machine ssh mongodb-manager1 -- "docker service create --constraint 'node.hostname==mongodb-manager1' \
                                    --network mongoshard \
                                    --name mongocfg1 \
                                    --endpoint-mode dnsrr \
                                    --restart-condition on-failure \
                                    --restart-max-attempts 5 \
                                    mongo:3.2 \
                                    --bind_ip 0.0.0.0 \
                                    --replSet conf_replicaset \
                                    --configsvr"

echo "Creating mongoS service..."
docker-machine ssh mongodb-manager1 -- 'docker service create --constraint "node.hostname==mongodb-manager1" \
                                --network mongoshard \
                                --name mongos1 \
                                --restart-condition on-failure \
                                --restart-max-attempts 5 \
                                 --publish published=27018,mode=host,target=27017 \
                                crimsonglory/mongos32 \
                                --configdb mongocfg1;' 

echo "Creating mongo shard 1 service..."
docker-machine ssh mongodb-worker1 -- 'docker volume create --name=mongodb-worker1-db-shard' # creating volume which will contain dir that mongodb data files will be written to. 
docker-machine ssh mongodb-manager1 -- "docker service create --constraint 'node.hostname==mongodb-worker1' \
                                    --network mongoshard \
                                    --name mongosh1 \
                                    --restart-condition on-failure \
                                    --restart-max-attempts 5 \
                                    --endpoint-mode dnsrr \
                                    mongo:3.2 \
                                    --bind_ip 0.0.0.0 \
                                    --port 27017 \
                                    --replSet sh_replicaset \
                                    --shardsvr"

echo "Creating mongo shard 2 service..."
docker-machine ssh mongodb-worker2 -- 'docker volume create --name=mongodb-worker2-db-shard' # creating volume which will contain dir that mongodb data files will be written to. 
docker-machine ssh mongodb-manager1 -- "docker service create --constraint 'node.hostname==mongodb-worker2' \
                                    --network mongoshard \
                                    --name mongosh2 \
                                    --restart-condition on-failure \
                                    --restart-max-attempts 5 \
                                    --endpoint-mode dnsrr \
                                    mongo:3.2 \
                                    --bind_ip 0.0.0.0 \
                                    --replSet sh_replicaset \
                                    --port 27017 \
                                    --shardsvr"

# Initializing the (currently one-member only) replica sets for the mongocfg and the shards.
docker-machine ssh mongodb-manager1 -- $'docker exec  $(docker ps --format {{.Names}} | grep mongocfg) mongo localhost:27019 --eval \
                                        \'
                                          rs.initiate({   _id: "conf_replicaset",   configsvr: true,    members: [{ _id : 0, host : "mongocfg1:27019" }] });
                                        \''

docker-machine ssh mongodb-worker1 -- $'docker exec  $(docker ps --format {{.Names}} | grep mongosh) mongo --eval \
                                        \'
                                          rs.initiate({   _id: "sh_replicaset",  members: [{ _id : 0, host : "mongosh1" }, { _id : 1, host : "mongosh2" }] });
                                        \''

sleep 15

# Enabling sharding and adding shards using mongo CLI running in mongoS container.
docker-machine ssh mongodb-manager1 -- $'docker exec  $(docker ps --format {{.Names}} | grep mongos) mongo --eval \
                                        \'
                                        sh.addShard(\"sh_replicaset/mongosh1\");
                                        sh.addShard(\"sh_replicaset/mongosh2\");
                                        \''


# If first-time create, creating DB and setting sharding key
#if [ $1 == "create" ]; then
  # Create database in one of the shards
  docker-machine ssh mongodb-worker1 -- $'docker exec $(docker ps --format {{.Names}} | grep mongosh) bash -c "echo '\''use BenchmarkDB;'\'' | mongo"'

  # Finally, enable sharding on that db and set sharding key
  docker-machine ssh mongodb-manager1 -- $'docker exec  $(docker ps --format {{.Names}} | grep mongos) mongo --eval \
                                          \'  
                                          db.runCommand({\"enablesharding\": \"BenchmarkDB\"});
                                          db.runCommand({\"shardCollection\": \"BenchmarkDB.fs.chunks\", \"key\": {\"files_id\": 1, \"n\": 1}, \"numInitialChunks\": 1000});
                                          \''
#fi

##################################################
# TODO: Create benchmark DB + schema here IF START
##################################################



# # Stopping the MongoS service, leaving the shards running
# docker-machine ssh mongodb-manager1 -- 'docker service rm mongos1'

# declare -a volumes=(mongodb-manager1-db-data mongodb-worker1-db-shard mongodb-worker2-db-shard)
# declare -a machines=(mongodb-manager1 mongodb-worker1 mongodb-worker2)
# declare -a servicenames=(mongocfg mongosh1 mongosh2)

# for (( index=0; index<3; index++ )) do
#     # rm'ing possible dangling volumecopier container
#     docker-machine ssh ${machines[$index]} -- 'docker rm volumecopier -f'

#     # creating a container that uses the current volume
#     docker-machine ssh ${machines[$index]} -- 'docker container run --name volumecopier --volume '"${volumes[$index]}"':/dump/ -d centos:latest sleep infinity'

#     # then, copying from our local backup dir into that container's dir that is mounted to the volume, so that the volume is populated.
#     docker-machine ssh ${machines[$index]} -- 'docker cp /c/Users/kraaijeveld/DockerBackup/'"${volumes[$index]}"'/. volumecopier:/dump/' 

#     # removing the volume carrying container now that it's work is done
#     docker-machine ssh ${machines[$index]} -- 'docker rm volumecopier -f'

#     # using mongorestore, restoring the dump that we just received from our local disk to the mongoDB instance.
#     docker-machine ssh ${machines[$index]} -- 'docker exec \
#                                               $(docker ps --format "{{.Names}}" | grep '"${servicenames[$index]}"') \
#                                               mongorestore --dir=/dump/ --oplogReplay'
# done

# # Finally, restarting the mongoS service
# docker-machine ssh mongodb-manager1 -- 'docker service create --constraint "node.hostname==mongodb-manager1" \
#                                 --network mongoshard \
#                                 --name mongos1 \
#                                 --restart-condition on-failure \
#                                 --restart-max-attempts 5 \
#                                 -p 27018:27017 \
#                                 crimsonglory/mongos32 \
#                                 --configdb mongocfg1;' 
