#!/bin/python3
from pymongo import MongoClient
import random
import string
import time
# import sys

class Test(object):
  def __init__(self, size : int) -> None:
    super().__init__()
    self.size = size
    self.regions = [ 'region' + str(i) for i in range(10) ]
    self.operators = [ 'operator' + str(i) for i in range(10) ]

  def generate_jsons(self) -> list:
    jsons = []
    for i in range(self.size):
      to_add = {}
      to_add['id'] = int(random.choice(range(0,10000000)))
      to_add['timestamp_id'] = int(random.choice(range(0,10000000)))
      to_add['timestamp'] = int(random.choice(range(0,10000000)))
      to_add['region'] = random.choice(self.regions)
      to_add['value'] = float(random.choice(range(0,10000000)))
      to_add['operator_id'] = random.choice(self.operators)
      jsons.append(to_add)
    return jsons

  def generate_queries(self) -> list:
    queries = []
    operator_queries = [ {'operator_id': random.choice(self.operators)} for i in range(self.size/2) ]
    region_queries = [ {'region': random.choice(self.regions)} for i in range(self.size/2) ]
    return queries

  def insert(self) -> None:
    start = time.time()
    data = self.generate_jsons()
    for i in range(self.size):
      col.insert_one(data[i])
    writting_time = time.time() - start
    print('Inserting {} values took {} seconds'.format(test_size, writting_time))

if __name__ == '__main__':
  test_size = 100000
  tester = Test(test_size)
  client = MongoClient('mongodb://root:pass@localhost:27017/')
  # create new database
  db = client['5g-monitoring']
  col = db['test-data']

  # insert data, measure write time
  tester.insert()

  # send queries, measure response time

  # see how much disk space has been used
