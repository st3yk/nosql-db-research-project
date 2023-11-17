from cassandra.cluster import Cluster
import uuid

cluster = Cluster(["192.168.56.21"])
session = cluster.connect()

session.execute(
    """
    CREATE KEYSPACE IF NOT EXISTS my_keyspace
    WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1}
"""
)

session.execute(
    """
    CREATE TABLE IF NOT EXISTS my_keyspace.my_table (
        id UUID PRIMARY KEY,
        name TEXT,
        age INT
    )
"""
)

insert_query = session.prepare(
    """
    INSERT INTO my_keyspace.my_table (id, name, age) VALUES (?, ?, ?)
"""
)

data_to_insert = {"id": uuid.uuid4(), "name": "John Doe", "age": 25}

session.execute(insert_query, data_to_insert)

select_query = session.prepare(
    """
    SELECT * FROM my_keyspace.my_table
"""
)

result = session.execute(select_query)

print("Table:")
for row in result:
    print(f"ID: {row.id}, Name: {row.name}, Age: {row.age}")

cluster.shutdown()
