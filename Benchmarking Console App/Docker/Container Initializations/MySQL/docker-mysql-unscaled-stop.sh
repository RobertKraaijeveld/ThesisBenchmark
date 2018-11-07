#!/bin/bash/
docker-machine ssh mysql -- 'rm -rf /c/Users/kraaijeveld/DockerBackup/mysqlunscaled-volume/*;'
docker-machine ssh mysql -- 'docker exec mysqlunscaled bash -c "export MYSQL_PWD=password; mysqldump -u root --all-databases > /dump/benchmarkdb-backup.sql"'
docker-machine ssh mysql -- 'docker cp mysqlunscaled:/dump/benchmarkdb-backup.sql /c/Users/kraaijeveld/DockerBackup/mysqlunscaled-volume/;'
docker-machine ssh mysql -- 'docker stop mysqlunscaled'