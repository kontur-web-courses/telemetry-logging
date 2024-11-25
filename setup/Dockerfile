ARG ELASTIC_VERSION

# https://www.docker.elastic.co/
FROM docker.io/elasticsearch:${ELASTIC_VERSION:-8.16.1}

ENTRYPOINT ["/entrypoint.sh"]
