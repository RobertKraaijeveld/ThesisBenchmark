#!/bin/bash
cd "/c/Projects/Afstudeerexperimenten/Benchmarking Console App/Benchmarking Console App/Docker/Container Initializations"

./Cassandra/docker-cassandra-scaled-stop.sh
./Cassandra/docker-cassandra-unscaled-stop.sh

./MongoDB/docker-mongodb-stop.sh scaled
./MongoDB/docker-mongodb-stop.sh unscaled

./MySQL/docker-mysql-scaled-stop.sh
./MySQL/docker-mysql-unscaled-stop.sh

./PostgreSQL/docker-postgres-scaled-stop.sh
./PostgreSQL/docker-postgres-unscaled-stop.sh

./Redis/docker-redis-stop.sh scaled
./Redis/docker-redis-stop.sh unscaled