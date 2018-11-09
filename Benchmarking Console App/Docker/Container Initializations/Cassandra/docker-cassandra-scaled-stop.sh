declare -a nodes=(cassandrascaled-node1 cassandrascaled-node2)

for (( index=0; index<2; index++ )) do
    # RM'ing stale previous backup to avoid possible conflicts when restoring
    docker-machine ssh cassandra -- 'rm -rf /c/Users/kraaijeveld/DockerBackup/'"${nodes[$index]}"'-volume/*;'

    docker-machine ssh cassandra -- 'docker exec '"${nodes[$index]}"' nodetool snapshot benchmarkdb -t '"${nodes[$index]}"'-backup;
                                     backupdir=$(docker exec '"${nodes[$index]}"' bash -c "cd /var/lib/cassandra/data/benchmarkdb/; \
                                                                                           cd */; \ 
                                                                                           cd snapshots/'"${nodes[$index]}"'-backup/; \
                                                                                           pwd;");
                                     rm -rf $backupdir; 

                                     docker cp '"${nodes[$index]}"':$backupdir/. /c/Users/kraaijeveld/DockerBackup/'"${nodes[$index]}"'-volume/;'
    docker-machine ssh cassandra -- 'docker stop '"${nodes[$index]}"''
done

