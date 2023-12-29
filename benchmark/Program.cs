
using MongoDB.Driver;
using Models;


namespace Benchmark
{
    /*
    class ElasticTester{

        private static ElasticsearchClient client;
        private static DateTime start_date;
        public ElasticTester(){}
        static void SetupClient(){
            client = new ElasticsearchClient(new Uri(""));
            start_date = DateTime.ParseExact("20230501T00:00:00Z", "yyyyMMddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime();
        } 

        static void BulkLoad(int no_records){
            
            client.Indices.Create("metrics");
            var data = GenerateTestData(no_records);
            
        }

        static List<Record> GenerateTestData(int count){
            var testData = new List<Record>();
            var random = new Random();
            Record r;
            for (var i = 0; i < count; i++){
              
                for (int bs_id = 0; bs_id < 3; bs_id++){
                        r = new Record { 
                        timestamp = start_date.AddSeconds(i).ToUniversalTime(), 
                        BSData = new BSData{
                                bs_id = bs_id
                            } 
                        };
                    testData.Add(r);
                    for (int ue_id = 0; ue_id < 5; ue_id++){
                            r = new Record { 
                            timestamp = start_date.AddSeconds(i).ToUniversalTime(), 
                            UEData = new UEData{
                                    ue_id = ue_id,
                                    pci = bs_id 
                                } 
                        };
                        testData.Add(r);  
                    }
                }
                
            }

            return testData;
        }
        static void ResetDB(){
            client.Indices.Delete("metrics");
        }
        static void ReadTest(int no_reads){
            var watch = new System.Diagnostics.Stopwatch();
            Console.WriteLine($"Starting read test with {no_reads} reads...");
            double time_sum = 0;
            for (int i = 0; i < no_reads; i++){
                
                watch.Restart();
                var res = client.Get<Record>(1, x => x.Index("metrics"));
                watch.Stop();
            }
            Console.WriteLine($"Test finished after {watch.Elapsed.TotalSeconds}seconds.");
            Console.WriteLine($"Average ops/s: {no_reads/watch.Elapsed.TotalSeconds}.");
        }
       static void AggregationTest(int n, bool two_weeks){
            var watch = new System.Diagnostics.Stopwatch();
            var end_date = two_weeks ? start_date.AddDays(14):start_date.AddDays(7);
            Console.WriteLine($"Starting aggregation test with {n} querries ({start_date} - {end_date}...)");
            for (int i = 0; i < n; i++){
                var request = new SearchRequest<Record>{
                    Query = new DateRangeQuery(Infer.Field<Record>(x => x.timestamp)){
                        Gte = start_date,
                        Lte = end_date
                    },
                    Aggregations = new AverageAggregation("avg", Infer.Field<Record>(x => x.UEData.dlul_brate))
                };

                watch.Restart();
                var res = client.Search<Record>(request);
                watch.Stop();
            }
            Console.WriteLine($"Test finished after {watch.Elapsed.TotalSeconds}seconds.");
            Console.WriteLine($"Average ops/s: {n/watch.Elapsed.TotalSeconds}.");
        }

    }
    */
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
            string connectionString = "mongodb://db-vm-1.jbt.pl:27017,db-vm-2.jbt.pl:27017,db-vm-3.jbt.pl:27017/?replicaSet=jbt-cluster";
            //MongoBenchmark benchmark = new MongoBenchmark("mongodb://root:pass@localhost:27017/");
            MongoBenchmark benchmark = new MongoBenchmark(connectionString, DateTime.UtcNow);
            
            //benchmark.ResetDB();
            //benchmark.SetupDB();
            //benchmark.BulkLoad(250_000, 10_000);
            for (int i = 0; i < 1; i++){
                Thread.Sleep(500);   
            //benchmark.ResetDB();
            //benchmark.SetupDB();
            //benchmark.BulkLoad(100_000, 10_000);
                benchmark.AggregationTest(1);
            }
            //benchmark.SequentialReadTest(10_000);
            //benchmark.AggregationTest(100_000);
        }

        static void StartElasticBenchmark(){
            var nodes = new Uri[]{
                new Uri("http://db-vm-1.jbt.pl:9200"),
                new Uri("http://db-vm-2.jbt.pl:9200"),
                new Uri("http://db-vm-3.jbt.pl:9200")
            };
            ElasticBenchmark benchmark = new ElasticBenchmark(nodes, DateTime.UtcNow);
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
            
            //benchmark.ResetDB();
            //benchmark.SetupDB();
            //benchmark.BulkLoad(10_000, 1_000);
            //benchmark.SequentialReadTest(10_000);
            //benchmark.AggregationTest(1_000);
        }
    }
}