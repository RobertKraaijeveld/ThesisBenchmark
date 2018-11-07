#!/bin/bash
cd "/c/Projects/Afstudeerexperimenten/Benchmarking program/Docker/Container Initializations"

./MySQL/docker-mysql-unscaled-start.sh
./PostgreSQL/docker-postgres-unscaled-start.sh
./Redis/docker-redis-unscaled-start.sh start
# ./MongoDB/docker-mongodb-unscaled-start.sh
./Cassandra/docker-cassandra-unscaled-start.sh
