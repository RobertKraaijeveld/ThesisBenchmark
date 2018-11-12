#!/bin/bash
cd "/c/Projects/Afstudeerexperimenten/Benchmarking Console App/Benchmarking Console App/Docker/Container Initializations"

./MySQL/docker-mysql-unscaled-start.sh
./PostgreSQL/docker-postgres-unscaled-start.sh
./Redis/docker-redis-unscaled-start.sh start
./Cassandra/docker-cassandra-unscaled-start.sh
./MongoDB/docker-mongodb-unscaled-start.sh start

