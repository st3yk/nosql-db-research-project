#!/bin/env bash

# Generators
go install github.com/influxdata/influxdb-comparisons/cmd/bulk_data_gen@latest github.com/influxdata/influxdb-comparisons/cmd/bulk_query_gen@latest

# Loaders
go install github.com/influxdata/influxdb-comparisons/cmd/bulk_load_cassandra@latest \
            github.com/influxdata/influxdb-comparisons/cmd/bulk_load_mongo@latest \
            github.com/influxdata/influxdb-comparisons/cmd/bulk_load_es@latest

# Query benchmarkers
go install github.com/influxdata/influxdb-comparisons/cmd/query_benchmarker_cassandra@latest \
            github.com/influxdata/influxdb-comparisons/cmd/query_benchmarker_mongo@latest \
            github.com/influxdata/influxdb-comparisons/cmd/query_benchmarker_es@latest
