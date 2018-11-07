#!/bin/bash/
docker-machine ssh postgresql -- 'rm -rf /c/Users/kraaijeveld/DockerBackup/postgresqlscaled-volume/*;'
docker-machine ssh postgresql -- 'docker exec postgresqlscaled bash -c "PGPASSWORD=password; pg_dump -U postgres --clean -C BenchmarkDB > /dump/BenchmarkDB.dump"'
docker-machine ssh postgresql -- 'docker cp postgresqlscaled:/dump/BenchmarkDB.dump /c/Users/kraaijeveld/DockerBackup/postgresqlscaled-volume/;'
docker-machine ssh postgresql -- 'docker stop postgresqlscaled'