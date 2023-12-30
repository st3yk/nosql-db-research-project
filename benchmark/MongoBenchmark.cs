using System.Diagnostics;
using Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

public class MongoBenchmark : IDatabaseBenchmark{

    private MongoClient client;
    private DateTime startTime;
    public MongoBenchmark(string connectionString, DateTime _startTime){
        this.client = new MongoClient(connectionString);
        this.startTime = _startTime;
        this.RegisterDataModels();
    }
    public void SetupDB(){
        Console.WriteLine("Setting up the DB...");
        CreateCollectionOptions options = new CreateCollectionOptions{TimeSeriesOptions = new TimeSeriesOptions("timestamp")};
        this.client.GetDatabase("benchmark").CreateCollection("metrics", options);
        var model = new CreateIndexModel<Record>("{timestamp: 1}");
        var model1 = new CreateIndexModel<Record>("{timestamp: -1}");
        var model2 = new CreateIndexModel<Record>("{\"ue_data.pci\": 1}");
        
        this.client.GetDatabase("benchmark").GetCollection<Record>("metrics").Indexes.CreateMany([model, model2]);
        Console.WriteLine("DB setup finished.\n");
    }
    public void ResetDB(){
        this.client.DropDatabase("benchmark");
    }
    private void RegisterDataModels(){
        if(!BsonClassMap.IsClassMapRegistered(typeof(UEData))){
                Console.WriteLine($"Registering {typeof(UEData)} classmap.");
                BsonClassMap.RegisterClassMap<UEData>();
            }
        if(!BsonClassMap.IsClassMapRegistered(typeof(BSData))){
            Console.WriteLine($"Registering {typeof(BSData)} classmap.");
            BsonClassMap.RegisterClassMap<BSData>();
        }
        if(!BsonClassMap.IsClassMapRegistered(typeof(Record))){
            Console.WriteLine($"Registering {typeof(Record)} classmap.");
            BsonClassMap.RegisterClassMap<Record>();
        }
        Console.WriteLine();
    }
    public void BulkLoad(int timePointsCount, int chunkSize){
        Console.WriteLine($"Bulk loading {timePointsCount*18} records in chunks of {chunkSize*18}...");
        BenchmarkDataGenerator gen = new BenchmarkDataGenerator(chunkSize, timePointsCount, startTime);
        var collection = this.client.GetDatabase("benchmark").GetCollection<Record>("metrics"); 
        var watch1 = new Stopwatch();
        var watch2 = new Stopwatch();
        int chunkNumber = 1;
        watch1.Start();
        foreach (var data in gen.GetDataChunk()){
            Console.WriteLine($"{100.0*chunkNumber/gen.chunkCount:0.00}%");
            watch2.Start();
            collection.InsertMany(data);
            watch2.Stop();
            chunkNumber += 1;
        }
        watch1.Stop();
        Console.WriteLine("Bulk loading finished.\n");
        Console.WriteLine($"Total time including generating data: {watch1.Elapsed.TotalSeconds}");
        Console.WriteLine($"Total time only inserting data: {watch2.Elapsed.TotalSeconds}");
    }
    public void SequentialReadTest(int readCount){
        Console.WriteLine($"Starting sequential read test for {readCount} reads...");
        Stopwatch watch = new System.Diagnostics.Stopwatch();
        IMongoCollection<Record> collection = this.client.GetDatabase("benchmark").GetCollection<Record>("metrics");
        FilterDefinition<Record> filter;
        for (int i = 0; i < readCount; i++){
            filter = Builders<Record>.Filter.Eq(x => x.bs_data.bs_id, new Random().Next(3));
            watch.Start();
            var doc = collection.Find(filter).First();
            watch.Stop();
            if(i%10_000 == 0 && i != 0) Console.WriteLine($"Finished {i} reads...");
        }
        Console.WriteLine("Sequential read test finished.");
        Console.WriteLine($"Total time for {readCount} reads: {watch.Elapsed.TotalSeconds} seconds.");
        Console.WriteLine($"Ops/second: {readCount / watch.Elapsed.TotalSeconds}.\n");

    }
    public void AggregationTest(int queryCount){
        var watch = new System.Diagnostics.Stopwatch();
        var endTimeOneWeek = startTime.AddDays(7);
        var endTimeTwoWeeks = startTime.AddDays(14);
        
        Console.WriteLine($"Starting aggregation test with {queryCount} querries ({startTime} - {endTimeOneWeek}...)");
        var filter_builder = Builders<Record>.Filter;
        var dateRangeFilter = filter_builder.Gte(x => x.timestamp, startTime) & filter_builder.Lte(x => x.timestamp, endTimeOneWeek);
        var existsFilter = filter_builder.Exists(x => x.ue_data);
        
        var collection = this.client.GetDatabase("benchmark").GetCollection<Record>("metrics");
        for (int i = 0; i < queryCount; i++){
            var q = collection.Aggregate()
                .Match(dateRangeFilter)
                .Match(existsFilter)
                .Group(g => g.ue_data.pci,
                 g => new { avg = g.Average(x => x.ue_data.dlul_brate) });
                
            
            //Console.WriteLine(q);
            //var test = collection.Find(dateRangeFilter);
            watch.Start();
            var res = q.ToCursor();
            watch.Stop();
            if(i%1_000 == 0 && i != 0) Console.WriteLine($"Finished {i} queries...");
        }

        Console.WriteLine("Aggregation test finished.");
        Console.WriteLine($"Total time for {queryCount} queries: {watch.Elapsed.TotalSeconds} seconds.");
        Console.WriteLine($"Ops/second: {queryCount / watch.Elapsed.TotalSeconds}.\n");
        Console.WriteLine($"Starting aggregation test with {queryCount} querries ({startTime} - {endTimeTwoWeeks}...)");
        dateRangeFilter = filter_builder.Gte(x => x.timestamp, startTime) & filter_builder.Lte(x => x.timestamp, endTimeTwoWeeks) & filter_builder.Exists(x => x.ue_data);
        watch.Reset();
        for (int i = 0; i < queryCount; i++){
            var q = collection.Aggregate()
                .Match(dateRangeFilter)
                .Match(existsFilter)
                .Group(g => g.ue_data.pci,
                g => new { avg = g.Average(x => x.ue_data.dlul_brate) });
            watch.Start();
            var res = q.ToCursor();
            watch.Stop();
            if(i%1_000 == 0 && i != 0) Console.WriteLine($"Finished {i} queries...");
        }

        Console.WriteLine("Aggregation test finished.");
        Console.WriteLine($"Total time for {queryCount} queries: {watch.Elapsed.TotalSeconds} seconds.");
        Console.WriteLine($"Ops/second: {queryCount / watch.Elapsed.TotalSeconds}.\n");
    }

}