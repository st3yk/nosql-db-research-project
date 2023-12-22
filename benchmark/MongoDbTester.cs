using Microsoft.VisualBasic;
using MongoDB.Driver;
using MongoDB.Bson;
using Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.IO;

public class MongoDbTester : DataBaseTestingInterface  {
    private List<string> contactPoints = [];
    private List<MongoClient> clients = [];
    private List<IMongoCollection<Record>> collections = [];
    private DateTime start_date;

    public void BulkLoadTest(){
        this.BulkLoad(1_000);
    }
    public void AggregationTest(){
        this.AggregationTest(1, false);
    }
    public void BulkReadTest(){
        this.ReadTest(1_000_000);
    } 
    
    MongoDbTester(List<string> contactPoints, List<string> ports){
        this.contactPoints = contactPoints;
        
    }

    void Old_Main(string[] args)
    {      
        start_date = DateTime.ParseExact("20230501T00:00:00Z", "yyyyMMddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime();

        Connect();
        //ResetDB();
        //CreateTSCollection();
        //BulkLoad(1_000);
        //ReadTest(100);
        
        // ReadTest(1_000_000,0);
        //watch.Stop();
        //Console.WriteLine(watch.ElapsedMilliseconds);
        
    }
   
    void Connect(){
        foreach (var conString in contactPoints){
            this.clients.Add(new MongoClient("mongodb://root:pass@localhost:27017/"));
        }
    }
    void ResetDB(){
        var master = this.clients[0];
        master.DropDatabase("benchmark");
    }
    void CreateTSCollection(){
        this.clients[0].GetDatabase("benchmark").CreateCollection("metrics", new CreateCollectionOptions { TimeSeriesOptions = new TimeSeriesOptions("timestamp", "data") });
    }
    void BulkLoad(int n){
        var data = GenerateTestData(n, start_date);
        using (var session = clients[0].StartSession()){
            var db = clients[0].GetDatabase("benchmark");
            db.GetCollection<Record>("metrics").InsertMany(data);
        }
    }
    public static List<Record> GenerateTestData(int count, DateTime start){
        var testData = new List<Record>();
        var random = new Random();
        Record r;
        for (var i = 0; i < count; i++){
            
            for (int bs_id = 0; bs_id < 3; bs_id++){
                    r = new Record { 
                    timestamp = start.AddSeconds(i).ToUniversalTime(), 
                    BSData = new BSData{
                            bs_id = bs_id
                        } 
                    };
                testData.Add(r);
                for (int ue_id = 0; ue_id < 5; ue_id++){
                        r = new Record { 
                        timestamp = start.AddSeconds(i).ToUniversalTime(), 
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

    void ReadTest(int no_reads){
        // var watch = new System.Diagnostics.Stopwatch();
        // Console.WriteLine($"Starting read test with {no_reads} reads...");
        // double time_sum = 0;
        // for (int i = 0; i < no_reads; i++){
        //     var db = clients[0].GetDatabase("benchmark");
        //     var collection = db.GetCollection<Record>("metrics");
        //     var filter = Builders<Record>.Filter.Eq(x=>x.BSData.bs_id, new Random().Next(3));
        //     watch.Start();
        //     var doc = collection.Find<Record>(filter).First();
        //     watch.Stop();
        //     time_sum += watch.Elapsed.TotalSeconds;
        // }
        // Console.WriteLine($"Test finished after {time_sum}seconds.");
        // Console.WriteLine($"Average ops/s: {no_reads/time_sum} seconds per read.");
    }
    void AggregationTest(int n, bool two_weeks){
        var watch = new System.Diagnostics.Stopwatch();
        var end_date = two_weeks ? start_date.AddDays(14):start_date.AddDays(7);
        Console.WriteLine($"Starting aggregation test with {n} querries ({start_date} - {end_date}...)");
        var filter_builder = Builders<Record>.Filter;
        var query = filter_builder.Gte(x => x.timestamp, start_date) & filter_builder.Lte(x => x.timestamp, end_date) & filter_builder.Exists(x => x.UEData);
        double time_sum = 0;
        for (int i = 0; i < n; i++){
            var db = clients[0].GetDatabase("benchmark");
            var collection = db.GetCollection<Record>("metrics");
            watch.Start();
            var res = collection.Aggregate<Record>()
            .Match(query)
            .Group(g => g.UEData.pci, // Group by a constant value (1) or any specific field
                g => new { avg = g.Average(x => x.UEData.dlul_brate) }).ToBsonDocument();//g.Average(x => x.Field1) });
            watch.Stop();
            Console.WriteLine(res);
            time_sum += watch.Elapsed.TotalSeconds;
        }
        Console.WriteLine($"Test finished after {time_sum}seconds.");
        Console.WriteLine($"Average delay: {time_sum/n} seconds per read.");
    }

}
