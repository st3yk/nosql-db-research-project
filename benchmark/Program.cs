
using MongoDB.Driver;
using Models;


namespace Benchmark
{
    
    class Benchmark {
        private static List<string> contactPoints = [];
        private static List<MongoClient> clients = [];
        private static List<IMongoCollection<Record>> collections = [];
        private static DateTime start_date;
        static void Main(string[] args){

            int dbArgIdx = Array.IndexOf(args, "--db");
            if(dbArgIdx == -1 || args.Length <= dbArgIdx + 1){
                throw new ArgumentException("No database has been specified, add \"--db\" argument followed by database name: mongo/cassandra/elastic.");
            }

            switch(args[dbArgIdx+1]){
                case "mongo":
                    StartMongoBenchmark();
                    break;
                case "elastic":
                    StartElasticBenchmark();
                    break;
            }
        }

        static void StartMongoBenchmark(){
            string connectionString = "mongodb://db-vm-1:27017";
            MongoBenchmark benchmark = new MongoBenchmark(connectionString, DateTime.UtcNow);
            
        }

        static void StartElasticBenchmark(){
            var nodes = new Uri[]{
                new Uri("http://db-vm-1:9200"),
                new Uri("http://db-vm-2:9200"),
                new Uri("http://db-vm-3:9200")
            };
            ElasticBenchmark benchmark = new ElasticBenchmark(nodes[0], DateTime.UtcNow, true); 
            
            //benchmark.ResetDB();
            //benchmark.SetupDB();
            //benchmark.SequentialWriteTest(1000, 1);
	    //benchmark.SequentialReadTest(1000);

  /*          foreach (var timePointsCount in new int[]{1000, 10000, 100000}){
                
                 for (int i = 0; i < 3; i++){
                 benchmark.ResetDB();
                 benchmark.SetupDB();
                 benchmark.SequentialWriteTest(timePointsCount, 1);
                 }
		 Thread.Sleep(10000);
                 foreach(var readCount in new int[]{1000,10000,100000}){
		 	for (int i = 0; i < 3; i++){
                 		benchmark.SequentialReadTest(readCount);
                 	}
		 }
                 

            

            }
           
*/
	    
	    benchmark.ResetDB();
            benchmark.SetupDB();
            //benchmark.BulkLoad(1_000, 1_000);
            benchmark.SequentialWriteTest(25_000, 50);
            Thread.Sleep(20000);
            benchmark.AggregationTest(2);
            Thread.Sleep(10000);
            benchmark.AggregationTest(2);
	    Thread.Sleep(10000);
            benchmark.AggregationTest(2);

	}
    }
}
