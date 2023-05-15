#!/bin/python3
from pymongo import MongoClient
import random
import string
import time
# import sys

def generate_jsons(number: int) -> list:
  jsons = []

  for i in range(number):
    to_add = {} 
    to_add['id'] = int(random.choice(range(0,10000000)))
    to_add['timestamp_id'] = int(random.choice(range(0,10000000)))
    to_add['timestamp'] = int(random.choice(range(0,10000000)))
    to_add['region'] = ''.join(random.choice(string.ascii_lowercase) for j in range(10))
    to_add['value'] = float(random.choice(range(0,10000000)))
    to_add['operator_id'] = ''.join(random.choice(string.ascii_lowercase) for j in range(10))
    jsons.append(to_add)
  return jsons

if __name__ == '__main__':
  client = MongoClient('mongodb://root:pass@localhost:27017/')
  # create new database
  db = client['5g-monitoring']
  col = db['test-data']
  print(client.list_database_names())

  # insert data, measure write time
  test_size = 100000
  data = generate_jsons(test_size)
  start = time.time()
  for i in range(test_size):
    col.insert_one(data[i])
  writting_time = time.time() - start
  print('Inserting {} values took {} seconds'.format(test_size, writting_time))


  # send queries, measure response time

  # see how much disk space has been used
