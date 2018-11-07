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

# then, copying from our local backup dir into the container's mounted /dump/ dir, so that the volume is populated.
docker-machine ssh postgresql --  'docker cp /c/Users/kraaijeveld/DockerBackup/postgresqlscaled-volume/. postgresqlscaled:/dump/' 

echo "Sleeping for 20s to avoid accessing DB before it is fully initialized..."
sleep 20

# executing the dumped sql backup file
docker-machine ssh postgresql --  "docker exec postgresqlscaled bash -c 'PGPASSWORD=password; psql -U postgres < /dump/BenchmarkDB.dump'"