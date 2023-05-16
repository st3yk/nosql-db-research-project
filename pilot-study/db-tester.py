#!/bin/python3
from pymongo import MongoClient
import random
import string
import time
import sys
import getopt

class Test(object):
  def __init__(self, size : int, col_type : str) -> None:
    super().__init__()
    self.size = size
    self.col_type = col_type
    self.regions = [ 'region' + str(i) for i in range(10) ]
    self.operators = [ 'operator' + str(i) for i in range(10) ]
    self.client = MongoClient('mongodb://root:pass@localhost:27017/')
    self.db = self.client['5g-monitoring']
    if col_type == 'document':
      self.col = self.db['test-document-collection']
    elif col_type == 'timeseries':
      self.col = self.db.create_collection('test-timeseries-collection', timeseries={ 'timeField': 'timestamp' })

  def generate_data(self) -> list:
    data = []
    for i in range(self.size):
      to_add = {}
      to_add['id'] = int(random.choice(range(0,10000000)))
      to_add['timestamp_id'] = int(random.choice(range(0,10000000)))
      to_add['timestamp'] = int(random.choice(range(0,10000000)))
      to_add['region'] = random.choice(self.regions)
      to_add['value'] = float(random.choice(range(0,10000000)))
      to_add['operator_id'] = random.choice(self.operators)
      data.append(to_add)
    return data

  def generate_queries(self) -> list:
    operator_queries = [ {'operator_id': random.choice(self.operators)} for i in range(int(self.size/2)) ]
    region_queries = [ {'region': random.choice(self.regions)} for i in range(int(self.size/2)) ]
    return operator_queries + region_queries

  def write_test(self, scenario='one') -> None:
    data = self.generate_data()
    start = time.time()
    if scenario == 'one':
      for i in range(self.size):
        self.col.insert_one(data[i])
    else:
      self.col.insert_many(data)
    writing_time = time.time() - start
    print(f'Writing {test_size} values took {writing_time} seconds. Scenario: insert {scenario}.')

  def read_test(self) -> None:
    data = self.generate_queries()
    start = time.time()
    for i in range(self.size):
      self.col.find_one(data[i])
    reading_time = time.time() - start
    print(f'Reading {test_size} values took {reading_time} seconds')

def get_options():
  collection_type = 'document'
  test_size = 1000
  argv = sys.argv[1:]
  print(argv)
  try:
    opts, args = getopt.gnu_getopt(argv, 't:s:', ['type=', 'size='])
  except:
    raise Exception('Invalid option')

  for opt, arg in opts:
    if opt in ['-t', '--type']:
      collection_type = arg
    elif opt in ['-s', '--size']:
      test_size = arg

  return int(test_size), collection_type

if __name__ == '__main__':
  # initialize tester
  test_size, collection_type = get_options()
  print(test_size, collection_type)
  tester = Test(test_size, collection_type)

  # insert data, measure write time
  tester.write_test()
  tester.write_test('many')

  # send queries, measure response time
  tester.read_test()

  # see how much disk space has been used
