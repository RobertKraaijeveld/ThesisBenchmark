#!/bin/bash/
docker-machine ssh mysql -- 'rm -rf /c/Users/kraaijeveld/DockerBackup/mysqlscaled-volume/*;'
docker-machine ssh mysql -- 'docker exec mysqlscaled bash -c "export MYSQL_PWD=password; mysqldump -u root --all-databases > /dump/benchmarkdb-backup.sql"'
docker-machine ssh mysql -- 'docker cp mysqlscaled:/dump/benchmarkdb-backup.sql /c/Users/kraaijeveld/DockerBackup/mysqlscaled-volume/;'
docker-machine ssh mysql -- 'docker stop mysqlscaled'