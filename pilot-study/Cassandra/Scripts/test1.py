from cassandra.cluster import Cluster

if __name__ == "__main__":
	cluster = Cluster(['0.0.0.0'], port=9042)
	session = cluster.connect('test', wait_for_all_pools=True)
	session.execute('USE test')
	rows = session.execute('SELECT * FROM ludzie')
	for row in rows:
		print(row.imie,row.nazwisko,row.wiek, row.id)
