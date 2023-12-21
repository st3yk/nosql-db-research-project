
using Microsoft.VisualBasic;
using MongoDB.Driver;
using MongoDB.Bson;
using Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.IO;

namespace Generator
{
    class Generator {
        private static List<string> contactPoints = [];
        private static List<MongoClient> clients = [];
        private static List<IMongoCollection<TimeSeries>> collections = [];
        static void Main(string[] args)
        {
            ParseArgs(args);
            Connect();
            var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();
            ResetDB();
            CreateTSCollection();
            BulkLoad(100);
            ReadTest(1_000_000,0);
            //watch.Stop();
            //Console.WriteLine(watch.ElapsedMilliseconds);
           
        }
        static void ParseArgs(string[] args){
            if(args.Contains("-urls")){
                int idx = Array.IndexOf(args, "-urls");
                int no_urls = -1;
                try{
                    no_urls = int.Parse(args[idx+1]);
                }
                catch(IndexOutOfRangeException e){
                    Console.WriteLine(e);
                    Console.WriteLine("Number of urls not specified");
                }
                try{
                    for (int i = 2; i < no_urls+2; i++){
                        contactPoints.Add(args[idx + i]);
                    }
                }
                catch(IndexOutOfRangeException e){
                    Console.WriteLine(e);
                    Console.Write("Number of urls given doesn't match number specified.");
                }
            }
        }
        static void Connect(){
            foreach (var conString in contactPoints){
                clients.Add(new MongoClient("mongodb://root:pass@localhost:27017/"));
            }
        }
        static void ResetDB(){
            var master = clients[0];
            master.DropDatabase("benchmark");
        }
        static void CreateTSCollection(){
            clients[0].GetDatabase("benchmark").CreateCollection("metrics", new CreateCollectionOptions { TimeSeriesOptions = new TimeSeriesOptions("timestamp", "data") });
        }
        static void BulkLoad(int n){
            var data = GenerateTestData(n, DateTime.UtcNow);
            using (var session = clients[0].StartSession()){
                var db = clients[0].GetDatabase("benchmark");
                db.GetCollection<TimeSeries>("metrics").InsertMany(data);
            }
        }
        static List<TimeSeries> GenerateTestData(int count, DateTime start){
            var testData = new List<TimeSeries>();
           
            for (var i = 0; i < count; i++){
               
                var r = new TimeSeries { 
                    timestamp = start.AddSeconds(i).ToUniversalTime(), 
                    data = new Data{
                        field1 = "tcp", 
                        field2 = "192.168.0.1", 
                        field3 = "192.168.0.2"
                        } 
                    };
                testData.Add(r);
            }

            return testData;
        }

        static void ReadTest(int no_reads, int no_threads){
            var watch = new System.Diagnostics.Stopwatch();
            Console.WriteLine($"Starting read test with {no_reads} reads...");
            watch.Start();
            for (int i = 0; i < no_reads; i++){
                var db = clients[0].GetDatabase("benchmark");
                var collection = db.GetCollection<TimeSeries>("metrics");
                var filter = Builders<TimeSeries>.Filter.Eq(x=>x.data.field1, "tcp");
                var doc = collection.Find<TimeSeries>(filter).First();
            }
            watch.Stop();
            Console.WriteLine($"Test finished after {watch.Elapsed.TotalSeconds}seconds.");
            Console.WriteLine($"Average delay: {watch.Elapsed.TotalSeconds/no_reads} seconds per read.");
        }
    
        static void AggregationTest(int n, DateTime start, DateTime end){
            var watch = new System.Diagnostics.Stopwatch();
            Console.WriteLine($"Starting aggregation test with {n} querries ({start} - {end}...)");
            var filter_builder = Builders<TimeSeries>.Filter;
            var range_query = filter_builder.Gte(x => x.timestamp, start) & filter_builder.Lte(x => x.timestamp, end);
            watch.Start();
            for (int i = 0; i < n; i++){
                var db = clients[0].GetDatabase("benchmark");
                var collection = db.GetCollection<TimeSeries>("metrics");
                collection.Aggregate<TimeSeries>().Match(range_query)
                .Group(g => new { Id = 1 }, // Group by a constant value (1) or any specific field
                    g => new { AvgField1 = g.Average(x => x.data.field1) });//g.Average(x => x.Field1) });
            }
            watch.Stop();
            Console.WriteLine($"Test finished after {watch.Elapsed.TotalSeconds}seconds.");
            Console.WriteLine($"Average delay: {watch.Elapsed.TotalSeconds/n} seconds per read.");
        }
    }
}