#!/bin/bash
if [ $# -eq 0 ]; then
  echo "Please provide a value of 'unscaled' or 'scaled' as the first argument."
  exit 0
elif [ $1 == "unscaled" ]; then
    declare -a machines=(redis-manager0)
    declare -a servicenames=(redis-m-7001)
    nodesCount=1
elif [ $1 == "scaled" ]; then
    declare -a machines=(redis-manager1 redis-worker1 redis-worker2)
    declare -a servicenames=(redis-cluster-m-7002 redis-cluster-m-7003 redis-cluster-m-7001)
    nodesCount=3
fi

for (( index=0; index<$nodesCount; index++ )) do
    echo "Going to back-up service ${servicenames[$index]}"
    docker-machine stop ${machines[$index]}
done