docker-machine start redis-manager1
hostip=$(docker-machine ip redis-manager1)
docker-machine ssh redis-manager1 -- 'docker rm $(docker ps -a --format {{.Names}} | grep "redis") -f'

#------------ bootstrap the cluster nodes --------------------

start_cmd='redis-server --cluster-enabled yes --cluster-config-file nodes.conf --cluster-node-timeout 5000 --appendonly yes'
redis_image='redis'
network_name='host'

#---------- create the cluster ------------------------

for port in `seq 6380 6385`; do \
 docker-machine ssh redis-manager1 -- 'docker run -d --name "redis-"'$port' --net '$network_name' --memory="400mb" '$redis_image' '$start_cmd' --port '$port';'
 echo "created redis cluster node redis-"$port
done

cluster_hosts=''
spaec=' '
for port in `seq 6380 6385`; do \
 cluster_hosts=$cluster_hosts$spaec$hostip:$port;
done

echo "cluster hosts "$cluster_hosts
echo "creating cluster...."
echo 'yes' | docker-machine ssh redis-manager1 -- 'docker run -i --rm --net '$network_name' '$redis_image' redis-cli --cluster create '$cluster_hosts' --cluster-replicas 1';

echo '-------------------------------------------------------------------'
echo '-------------------------------------------------------------------'
echo 'Done! Access Redis via '$(docker-machine ip redis-manager1)':6380'
echo '-------------------------------------------------------------------'
echo '-------------------------------------------------------------------'

