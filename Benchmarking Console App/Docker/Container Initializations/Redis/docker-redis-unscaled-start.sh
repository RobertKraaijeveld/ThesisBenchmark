#!/bin/sh
if [ $# -eq 0 ]; then
  echo "Please provide a value of 'start' or 'create' as the first argument."
  exit 0
elif [ $1 == "create" ]; then
  docker-machine rm redis-manager0 -f
  
  # Creating machines
  docker-machine create redis-manager0

  # Creating persistence volumnes
  docker-machine ssh redis-manager0 -- "docker volume create --name redis-manager0-volume"
elif [ $1 == "start" ]; then
  docker-machine start redis-manager0

  # Leaving existing swarm 
  docker-machine ssh redis-manager0 -- "docker swarm leave --force"

  # Removing network 
  docker-machine ssh redis-manager0 -- "docker network rm redis_network"

  # Re-running certificates (IP adress might have changed)
  docker-machine regenerate-certs redis-manager0 -f
fi

# Creating basic swarm
managerip=$(docker-machine ip redis-manager0)
docker-machine ssh redis-manager0 -- "docker swarm init --advertise-addr $managerip"

# Creating network
docker-machine ssh redis-manager0 -- 'docker network create --driver overlay redis_network'


####################################
# Restoring backups to the volumes #
####################################
./docker-redis-restore-backup.sh unscaled


############################
# Creating master services #
############################
docker-machine ssh redis-manager0 -- "
    echo creating service 'redis-m-7001';
    docker service create --constraint 'node.hostname == redis-manager0'\
      --name 'redis-m-7001'\
      --mount type=volume,src=redis-manager0-volume,dst=/redis/data/ \
      --network redis_network\
      --publish 7001:7001/tcp\
      --restart-condition any\
      --stop-grace-period 60s\
        comodal/alpine-redis\
          --appendfsync everysec\
          --appendonly yes\
          --auto-aof-rewrite-percentage 20\
          --cluster-enabled no\
          --logfile redis-m-7002.log\
          --port 7001\
          --protected-mode no\
          --repl-diskless-sync yes\
          --save ''''  "
