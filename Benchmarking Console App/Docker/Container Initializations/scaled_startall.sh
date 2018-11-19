#!/bin/bash
cd "/c/Projects/Afstudeerexperimenten/Benchmarking Console App/Benchmarking Console App/Docker/Container Initializations"

./MySQL/docker-mysql-scaled-start.sh
./PostgreSQL/docker-postgres-scaled-start.sh
./Redis/docker-redis-scaled-start-new.sh 
./MongoDB/docker-mongodb-scaled-start.sh start
./Cassandra/docker-cassandra-scaled-start.sh
