
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
            //string connectionString = "mongodb://db-vm-1.jbt.pl:27017,db-vm-2.jbt.pl:27017,db-vm-3.jbt.pl:27017/?replicaSet=jbt-cluster";
            MongoBenchmark benchmark = new MongoBenchmark("mongodb://root:pass@localhost:27017/", DateTime.UtcNow);
            //MongoBenchmark benchmark = new MongoBenchmark(connectionString, DateTime.UtcNow);
            
            //benchmark.ResetDB();
            //benchmark.SetupDB();
            //benchmark.BulkLoad(5_000, 1_000);
            benchmark.AggregationTest(1);
            /*
            for (int i = 0; i < 5; i++){
                Thread.Sleep(500);   
            //benchmark.ResetDB();
            //benchmark.SetupDB();
            //benchmark.BulkLoad(100_000, 10_000);
                benchmark.AggregationTest(3);
            }
            */
            //benchmark.SequentialReadTest(10_000);
            //benchmark.AggregationTest(100_000);
        }

        static void StartElasticBenchmark(){
            var nodes = new Uri[]{
                new Uri("http://db-vm-1.jbt.pl:9200"),
                new Uri("http://localhost:9200"),
                new Uri("http://db-vm-3.jbt.pl:9200")
            };
            ElasticBenchmark benchmark = new ElasticBenchmark(nodes, DateTime.UtcNow);
            //ElasticBenchmark benchmark = new ElasticBenchmark(nodes[0], DateTime.UtcNow);
            //return;
            benchmark.ResetDB();
            benchmark.SetupDB();
            benchmark.BulkLoad(100, 10);
            benchmark.AggregationTest(1);
            //Thread.Sleep(500);
            //benchmark.AggregationTest(1);
            /*
            for (int i = 0; i < 1; i++){
                Thread.Sleep(500);   
            //benchmark.ResetDB();
            //benchmark.SetupDB();
            //benchmark.BulkLoad(100_000, 10_000);
                benchmark.AggregationTest(3);
            }
            */
            /*
            foreach (var timePointsCount in new int[]{1000, 10000, 100000}){
                
                for (int i = 0; i < 5; i++){
                benchmark.ResetDB();
                benchmark.SetupDB();
                benchmark.BulkLoad(timePointsCount, timePointsCount/10);
                }
                foreach (var readCount in new int[]{100, 1000, 10000}){
                    for (int i = 0; i < 5; i++){
                        benchmark.SequentialReadTest(readCount);
                    }
                }
            

            }
            */
            
            //benchmark.ResetDB();
            //benchmark.SetupDB();
            //benchmark.BulkLoad(10_000, 1_000);
            //benchmark.SequentialReadTest(10_000);
            //benchmark.AggregationTest(1_000);
        }
    }
}