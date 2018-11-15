#!/bin/sh
if [ $# -eq 0 ]; then
  echo "Please provide a value of 'start' or 'create' as the first argument."
  exit 0
elif [ $1 == "create" ]; then
  docker-machine rm redis-manager1 -f
  docker-machine rm redis-worker1 -f
  docker-machine rm redis-worker2 -f
   # Creating machines
  docker-machine create redis-manager1
  docker-machine create redis-worker1
  docker-machine create redis-worker2
   # Creating persistence volumnes
  docker-machine ssh redis-manager1 -- "docker volume create --name redis-manager1-volume"
  docker-machine ssh redis-worker1 -- "docker volume create --name redis-worker1-volume"
  docker-machine ssh redis-worker2 -- "docker volume create --name redis-worker2-volume"
elif [ $1 == "start" ]; then
  docker-machine start redis-manager1
  docker-machine start redis-worker1
  docker-machine start redis-worker2
   # Leaving existing swarm 
  docker-machine ssh redis-worker2 -- "docker swarm leave --force"
  docker-machine ssh redis-worker1 -- "docker swarm leave --force"
  docker-machine ssh redis-manager1 -- "docker swarm leave --force"
   # Removing network 
  docker-machine ssh redis-manager1 -- "docker network rm redis_network"
   # Re-running certificates (IP adress might have changed)
  docker-machine regenerate-certs redis-worker2 -f
  docker-machine regenerate-certs redis-worker1 -f
  docker-machine regenerate-certs redis-manager1 -f
fi

 # Creating basic swarm
managerip=$(docker-machine ip redis-manager1)
docker-machine ssh redis-manager1 -- "docker swarm init --advertise-addr $managerip"
jointoken=$(docker-machine ssh redis-manager1 -- "docker swarm join-token worker --quiet")
docker-machine ssh redis-worker1 -- "docker swarm join --token $jointoken $managerip:2377"
docker-machine ssh redis-worker2 -- "docker swarm join --token $jointoken $managerip:2377"
 # Promoting all nodes to managers since we're going to be running a master-only Redis cluster
docker-machine ssh redis-manager1 -- "docker node promote redis-worker1"
docker-machine ssh redis-manager1 -- "docker node promote redis-worker2"
 # Creating network
docker-machine ssh redis-manager1 -- 'docker network create --driver overlay redis_network'


 ############################
# Creating master services #
############################
docker-machine ssh redis-manager1 -- "
    echo creating service 'redis-cluster-m-7002';
    docker service create --constraint 'node.hostname == redis-manager1'\
      --name 'redis-cluster-m-7002'\
      --mount type=volume,src=redis-manager1-volume,dst=/redis/data/ \
      --network redis_network\
      --publish 7002:7002/tcp\
      --restart-condition any\
      --stop-grace-period 60s\
        comodal/alpine-redis\
          --appendfsync everysec\
          --appendonly yes\
          --auto-aof-rewrite-percentage 20\
          --cluster-enabled yes\
          --cluster-node-timeout 60000\
          --cluster-require-full-coverage no\
          --logfile redis-cluster-m-7002.log\
          --port 7002\
          --protected-mode no\
          --repl-diskless-sync yes\
          --save ''''  "
 docker-machine ssh redis-manager1 -- "
    echo creating service 'redis-cluster-m-7003';
    docker service create --constraint 'node.hostname == redis-worker1'\
      --name 'redis-cluster-m-7003'\
      --mount type=volume,src=redis-worker1-volume,dst=/redis/data/ \
      --network redis_network\
      --publish 7003:7003/tcp\
      --restart-condition any\
      --stop-grace-period 60s\
        comodal/alpine-redis\
          --appendfsync everysec\
          --appendonly yes\
          --auto-aof-rewrite-percentage 20\
          --cluster-enabled yes\
          --cluster-node-timeout 60000\
          --cluster-require-full-coverage no\
          --logfile redis-cluster-m-7003.log\
          --port 7003\
          --protected-mode no\
          --repl-diskless-sync yes\
          --save ''''  "
 docker-machine ssh redis-manager1 -- "
    echo creating service 'redis-cluster-m-7001';
    docker service create --constraint 'node.hostname == redis-worker2'\
      --name 'redis-cluster-m-7001'\
      --mount type=volume,src=redis-worker2-volume,dst=/redis/data/ \
      --network redis_network\
      --publish 7001:7001/tcp\
      --restart-condition any\
      --stop-grace-period 60s\
        comodal/alpine-redis\
          --appendfsync everysec\
          --appendonly yes\
          --auto-aof-rewrite-percentage 20\
          --cluster-enabled yes\
          --cluster-node-timeout 60000\
          --cluster-require-full-coverage no\
          --logfile redis-cluster-m-7001.log\
          --port 7001\
          --protected-mode no\
          --repl-diskless-sync yes\
          --save ''''  "
 ##################################
# Letting nodes meet each other  #
##################################
  docker-machine ssh redis-manager1 -- '
      for port in 7001 7002 7003
      do
        LOCAL_CONTAINER_ID=`docker ps -q | head -n 1`;
         LOCAL_PORT=`docker inspect --format="{{index .Config.Labels \"com.docker.swarm.service.name\"}}" $LOCAL_CONTAINER_ID | sed '\''s|.*-||'\''`;
         if [ $LOCAL_PORT == $port ]; then
          echo Local port is current port, no meet and greet necessary.
          continue
        fi
         meet_node="redis-cluster-m-$port"
        meet_ip=`docker service inspect -f "{{(index .Endpoint.VirtualIPs 1).Addr}}" $meet_node  | sed '\''s|/.*||'\''`;
        echo I am going to be meeting node $meet_node with IP $meet_ip at port $port.
         docker exec $LOCAL_CONTAINER_ID \
                    redis-cli -c -p $LOCAL_PORT \
                    CLUSTER MEET $meet_ip $port
      done
       docker exec $LOCAL_CONTAINER_ID\
                    redis-cli -c -p $LOCAL_PORT\
                    CLUSTER NODES
    '
 ###################
# Assigning slots #
###################
 docker-machine ssh redis-manager1 -- '
    MAX_SLOT=$((16383));
    SLOT_RANGE=$(((MAX_SLOT + 3 - 1) / 3))
    slot=0;
     for port in 7003 7002 7001
    do
       end_slot=$((slot + SLOT_RANGE));
      end_slot=$((end_slot > MAX_SLOT ? MAX_SLOT : end_slot));
       LOCAL_CONTAINER_ID=`docker ps -q | head -n 1`;
      LOCAL_PORT=`docker inspect --format="{{index .Config.Labels \"com.docker.swarm.service.name\"}}" $LOCAL_CONTAINER_ID | sed '\''s|.*-||'\''`;
      host=`[ $LOCAL_PORT == $port ] && echo "127.0.0.1" || echo "redis-cluster-m-$port"`;
      
      echo "CLUSTER ADDSLOTS $slot - $end_slot"
       docker exec $LOCAL_CONTAINER_ID \
                  redis-cli -h $host -p $port \
                  CLUSTER ADDSLOTS $(seq $slot $end_slot);
       docker exec $LOCAL_CONTAINER_ID \
                  redis-cli -h $host -p $port \
                  CONFIG SET "save" "60 1";
       slot=$((end_slot + 1));
    done
    '
 # CLI ACCESS, ON DOCKER MACHINE THAT MANAGES REDIS RUN:
# docker run -it --rm --net=host --entrypoint redis-cli comodal/alpine-redis:unstable -p 7002 -c  