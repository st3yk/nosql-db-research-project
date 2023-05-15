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
    operator_queries = [ {'operator_id': random.choice(self.operators)} for i in range(int(self.size/2)) ]
    region_queries = [ {'region': random.choice(self.regions)} for i in range(int(self.size/2)) ]
    return operator_queries + region_queries

  def write_test(self, scenario='one') -> None:
    data = self.generate_jsons()
    start = time.time()
    if scenario == 'one':
      for i in range(self.size):
        col.insert_one(data[i])
    else:
      col.insert_many(data)
    writing_time = time.time() - start
    print(f'Writing {test_size} values took {writing_time} seconds. Scenario: insert {scenario}.')

  def read_test(self) -> None:
    data = self.generate_queries()
    start = time.time()
    for i in range(self.size):
      col.find_one(data[i])
    reading_time = time.time() - start
    print(f'Reading {test_size} values took {reading_time} seconds')

if __name__ == '__main__':
  print('Enter test sample size:')
  test_size = int(input())
  tester = Test(test_size)
  client = MongoClient('mongodb://root:pass@localhost:27017/')
  # create new database
  db = client['5g-monitoring']
  col = db['test-data']

  # insert data, measure write time
  tester.write_test()
  tester.write_test('many')

  # send queries, measure response time
  tester.read_test()

  # see how much disk space has been used
