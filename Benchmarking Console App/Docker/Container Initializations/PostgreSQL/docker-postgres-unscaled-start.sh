#!/bin/bash
docker-machine start postgresql

# Re-running certificates (IP adress might have changed)
docker-machine regenerate-certs postgresql -f

docker-machine ssh postgresql --  'docker rm postgresqlunscaled -f';
docker-machine ssh postgresql --  'docker volume create postgresqlunscaled-volume;'

#  Creating the container 
docker-machine ssh postgresql --  "docker run -d \
                                      --volume postgresqlunscaled-volume:/dump/ \
                                      -p 5433:5432 \
                                      -e POSTGRES_PASSWORD=password \
                                      --name=postgresqlunscaled \
                                      --cpus=0.5 \
                                      --memory='2gb' \
                                      postgres;"

echo "Sleeping for 20s to avoid accessing DB before it is fully initialized..."
sleep 20

# executing the dumped sql backup file
docker-machine ssh postgresql --  "docker exec postgresqlunscaled bash -c 'PGPASSWORD=password; psql -U postgres -c \"create database benchmarkdb;\"'"
