docker-machine start mysql

# Re-running certificates (IP adress might have changed)
docker-machine regenerate-certs mysql -f

docker-machine ssh mysql -- 'docker rm mysqlscaled -f';

#  Creating the mariadb container 
docker-machine ssh mysql -- "docker run -d \
                                      --volume mysqlscaled-volume:/dump/ \
                                      -e MYSQL_ROOT_PASSWORD=password -e MYSQL_ROOT_HOST=% \
                                      -p 3308:3306 \
                                      --name=mysqlscaled \
                                      --cpus=1 \
                                      --memory='4gb' \
                                      mariadb \
                                      --innodb_flush_method=O_DSYNC \
                                      --innodb_use_native_aio=0 \
                                      --log_bin;"

# then, copying from our local backup dir into the container's mounted /dump/ dir, so that the volume is populated.
docker-machine ssh mysql -- 'docker cp /c/Users/kraaijeveld/DockerBackup/mysqlscaled-volume/. mysqlscaled:/dump/' 

echo "Sleeping for 20s to avoid accessing DB before it is fully initialized..."
sleep 20

# executing the dumped sql backup file
docker-machine ssh mysql -- "docker exec mysqlscaled bash -c 'mysql -u root --password=password < /dump/benchmarkdb-backup.sql'"
