# A Comparison of Cloud Native NoSQL Databases for Processing 5G RAN Time Series Data

This repository contains the code used for benchmarking the performance of three cloud-native NoSQL databases—**Cassandra**, **Elasticsearch**, and **MongoDB**—in the context of processing 5G Radio Access Network (RAN) time series data.

The benchmarking tests evaluate each database's performance in both **stand-alone** and **clustered** configurations, focusing on key metrics such as **write speed**, **read speed**, and **aggregation speed**. These tests simulate real-world scenarios by generating synthetic time series data representing 5G network monitoring.

## Structure

- **/benchmark**: Contains the C# source code for the benchmark tests.
- **/cloud-infra**: Configuration files for setting up the databases on the Google Cloud Platform.
- **/db-provisioning**: Ansible Playbooks for setting up database services.
- **/pilot-study**: Contains the Python scripts that were used for testing databases locally.

## Benchmark Overview

- **Databases Tested**:
  - Cassandra 4.1.3
  - Elasticsearch 8.11.3
  - MongoDB 6.0.4
- **Cloud Platform**: Google Cloud Platform
- **Test Environment**: AlmaLinux 8.9-based VMs with 2 vCPUs, 8GB RAM, and 20GB disk space for database servers.

### Test Descriptions
- **Write Test**: Measures the time taken to insert various amounts of synthetic time series data into each database.
- **Read Test**: Evaluates the speed of querying the databases for a single row by a primary key.
- **Aggregation Test**: Measures the performance of filtering and aggregating data based on timestamp ranges.

### Database Configurations
- **Cassandra**: Wide column store, peer-to-peer gossip-based cluster.
- **Elasticsearch**: Search engine, sharded cluster.
- **MongoDB**: Document store, 1:1 replicaset architecture.
