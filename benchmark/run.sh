#!/bin/env bash

./install.sh

DB=cassandra # Possible DBs - cassandra, mongo, es
GOPATH=$(go env | grep PATH | tr -d \" | awk -F'=' '{ print $2; }')
case ${DB} in
    cassandra)
        PORT=9042
        ;;
    mongo)
        PORT=27017
        ;;
    es)
        PORT=9300
        ;;
    *)
        echo "Invalid DB - ${DB}, exiting... " && exit 1
        ;;
esac
echo "DATABASE: ${DB}, PORT: ${PORT}, GO PATH: ${GOPATH}"
${GOPATH}/bin/bulk_data_gen | ${GOPATH}/bin/bulk_load_${DB} -urls db-vm-1.jbt.pl:${PORT} db-vm-2.jbt.pl:${PORT} db-vm-3.jbt.pl:${PORT}
# ${GOPATH}/bin/bulk_query_gen | ${GOPATH}/bin/query_benchmarker_${DB} -urls db-vm-1.jbt.pl:${PORT} db-vm-2.jbt.pl:${PORT} db-vm-3.jbt.pl:${PORT}
