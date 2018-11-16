docker-machine ssh redis-manager1 -- 'docker rm $(docker ps -a --format {{.Names}} | grep "redis") -f'

#------------ bootstrap the cluster nodes --------------------

start_cmd='redis-server --port 6379 --cluster-enabled yes --cluster-config-file nodes.conf --cluster-node-timeout 5000 --appendonly yes'
redis_image='redis'
network_name='redis_cluster_net'

docker-machine ssh redis-manager1 -- 'docker network create '$network_name''
echo $network_name " created"

#---------- create the cluster ------------------------

for port in `seq 6380 6385`; do \
 docker-machine ssh redis-manager1 -- 'docker run -d --name "redis-"'$port' -p '$port':6379 --net '$network_name' --memory="400mb" '$redis_image' '$start_cmd';'
 echo "created redis cluster node redis-"$port
done

cluster_hosts=''
spaec=' '
for port in `seq 6380 6385`; do \
 filter="'{{(index .NetworkSettings.Networks \"redis_cluster_net\").IPAddress}}'"
 hostip=$(docker-machine ssh redis-manager1 -- 'docker inspect -f '$filter' redis-'$port'');
 cluster_hosts=$cluster_hosts$spaec$hostip:6379;
done

echo "cluster hosts "$cluster_hosts
echo "creating cluster...."
echo 'yes' | docker-machine ssh redis-manager1 -- 'docker run -i --rm --net '$network_name' '$redis_image' redis-cli --cluster create '$cluster_hosts' --cluster-replicas 1';

echo '-------------------------------------------------------------------'
echo '-------------------------------------------------------------------'
echo 'Done! Access Redis via '$(docker-machine ip redis-manager1)':6380'
echo '-------------------------------------------------------------------'
echo '-------------------------------------------------------------------'

