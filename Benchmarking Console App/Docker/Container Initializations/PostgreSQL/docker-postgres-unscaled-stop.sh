#!/bin/bash/
docker-machine ssh postgresql -- 'docker exec postgresqlunscaled bash -c "PGPASSWORD=password; pg_dump -U postgres --clean -C BenchmarkDB > /dump/BenchmarkDB.dump"'
docker-machine ssh postgresql -- 'docker cp postgresqlunscaled:/dump/BenchmarkDB.dump /c/Users/kraaijeveld/DockerBackup/postgresqlunscaled-volume/;'
docker-machine ssh postgresql -- 'docker stop postgresqlunscaled'