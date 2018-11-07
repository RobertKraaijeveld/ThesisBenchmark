docker-machine ssh cassandra -- 'rm -rf /c/Users/kraaijeveld/DockerBackup/cassandraunscaled-node1-volume/*;

                                 docker exec cassandraunscaled-node1 nodetool snapshot benchmarkdb -t cassandraunscaled-node1-backup;
                                 backupdir=$(docker exec cassandraunscaled-node1 bash -c "cd /var/lib/cassandra/data/benchmarkdb/; \
                                                                                       cd */; \ 
                                                                                       cd snapshots/cassandraunscaled-node1-backup/; \
                                                                                       pwd;");
                                 rm -rf $backupdir; 

                                 docker cp cassandraunscaled-node1:$backupdir/. /c/Users/kraaijeveld/DockerBackup/cassandraunscaled-node1-volume/;'
docker-machine ssh cassandra -- 'docker stop cassandraunscaled-node1'



