https://blog.devgenius.io/run-elasticsearch-and-kibana-as-docker-containers-c5f5f5460afd

docker pull docker.elastic.co/elasticsearch/elasticsearch:8.3.3
docker pull docker.elastic.co/kibana/kibana:8.3.3

docker network create elknetwork
docker run --name esc1 --net elknetwork -p 9200:9200 -p 9300:9300 -it docker.elastic.co/elasticsearch/elasticsearch:8.3.3
docker run --name esc1  --net elknetwork -p 9200:9200 -p 9300:9300 -e discovery.type=single-node -e ES_JAVA_OPTS="-Xms512m -Xmx512m" -e xpack.security.enabled=false -e xpack.monitoring.enabled=true  -it   docker.elastic.co/elasticsearch/elasticsearch:8.3.3
docker run --name esc1  --net elknetwork -p 9200:9200 -e discovery.type=single-node -e ES_JAVA_OPTS="-Xms1g -Xmx1g" -e xpack.security.enabled=false -e xpack.monitoring.enabled=true  -it   docker.elastic.co/elasticsearch/elasticsearch:8.3.3

yg jalan2 line dibawah ini
docker run --name elasticsearch4  --net elastic -p 9200:9200 -e discovery.type=single-node -e ES_JAVA_OPTS="-Xms1g -Xmx1g" -e xpack.security.enabled=false  -it   docker.elastic.co/elasticsearch/elasticsearch:8.3.3
docker run --name kibc1 --net elastic -p 5601:5601 docker.elastic.co/kibana/kibana:8.3.3

https://localhost:9200.
docker run --name kibc1 --net elknetwork -p 5601:5601 docker.elastic.co/kibana/kibana:8.3.3
docker exec -it esc1 /usr/share/elasticsearch/bin/elasticsearch-reset-password -u elastic
docker exec -it esc1 /usr/share/elasticsearch/bin/elasticsearch-create-enrollment-token -s node


docker network create elastic
docker run --name elasticsearch  --net elastic -p 9200:9200 -p 9300:9300 -e discovery.type=single-node -e ES_JAVA_OPTS="-Xms512m -Xmx512m" -e xpack.security.enabled=false -e xpack.monitoring.enabled=true  -it   docker.elastic.co/elasticsearch/elasticsearch:8.3.3


docker-compose up -d --build
docker ps
elastick http://localhost:9200
kibana http://localhost:5601

using powershell
Invoke-WebRequest -method DELETE http://localhost:9200/_all