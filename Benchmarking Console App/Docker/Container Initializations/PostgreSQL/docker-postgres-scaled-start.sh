#!/bin/bash
docker-machine start postgresql

# Re-running certificates (IP adress might have changed)
docker-machine regenerate-certs postgresql -f

docker-machine ssh postgresql --  'docker rm postgresqlscaled -f';
docker-machine ssh postgresql --  'docker volume create postgresqlscaled-volume;'

#  Creating the container 
docker-machine ssh postgresql --  "docker run -d \
                                      --volume postgresqlscaled-volume:/dump/ \
                                      -p 5434:5432 \
                                      -e POSTGRES_PASSWORD=password \
                                      --name=postgresqlscaled \
                                      --cpus=1 \
                                      --memory='4gb' \
                                      postgres;"

echo "Sleeping for 20s to avoid accessing DB before it is fully initialized..."
sleep 20

# executing the dumped sql backup file
docker-machine ssh postgresql --  "docker exec postgresqlscaled bash -c 'PGPASSWORD=password; psql -U postgres  -c \"create database benchmarkdb;\"'"