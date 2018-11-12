#!/bin/bash
docker-machine start cassandra
docker-machine ssh cassandra --  'docker volume create cassandrascaled-node1-volume;'
docker-machine ssh cassandra --  'docker volume create cassandrascaled-node2-volume;'

# Regenerating certs
docker-machine regenerate-certs cassandra -f

# Rm'ing old containers
docker-machine ssh cassandra -- 'docker rm cassandrascaled-node1 -f'
docker-machine ssh cassandra -- 'docker rm cassandrascaled-node2 -f'

#  Creating the containers
docker-machine ssh cassandra -- "docker run -d \
                                      --volume cassandrascaled-node1-volume:/dump/ \
                                      -p 9042:9042 \
                                      -p 9161:9160 \
                                      --name=cassandrascaled-node1 \
                                      --cpus=0.5 \
                                      --memory='2gb' \
                                      cassandra:latest;"

docker-machine ssh cassandra -- "docker run -d \
                                      --volume cassandrascaled-node2-volume:/dump/ \
                                      -p 9043:9042 \
                                      -p 9162:9160 \
                                      -e CASSANDRA_SEEDS="$(docker inspect --format='{{ .NetworkSettings.IPAddress }}' cassandrascaled-node1)" \
                                      --name=cassandrascaled-node2 \
                                      --cpus=0.5 \
                                      --memory='2gb' \
                                      cassandra:latest;"

echo "Sleeping for 40s to avoid accessing DB during intialization..."
sleep 40

# Creating keyspace: Keyspaces have to be manually recreated each run
docker-machine ssh cassandra -- 'docker exec cassandrascaled-node1 bash -c "echo \"CREATE KEYSPACE benchmarkdb WITH REPLICATION = { '"'"'class'"'"' : '"'"'SimpleStrategy'"'"', '"'"'replication_factor'"'"' : 1 };\" > createKeyspace.cql";'
docker-machine ssh cassandra -- 'docker exec cassandrascaled-node1 bash -c "cqlsh -f createKeyspace.cql"'

# Creating benchmark db keyspace:
# cqlsh -e "CREATE KEYSPACE benchmarkdb WITH REPLICATION = { 'class' : 'SimpleStrategy', 'replication_factor' : 1 };"
# cqlsh -e "CREATE TABLE benchmarkdb.test ( id UUID PRIMARY KEY );"
# cqlsh -e "INSERT INTO benchmarkdb.test (id) VALUES ( 6ab09bec-e68e-48d9-a5f8-97e6fb4c9b51 );"
# Taking snapshot
# nodetool snapshot benchmarkdb -t node1backup

