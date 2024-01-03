using System.Diagnostics;
using Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

public class MongoBenchmark : IDatabaseBenchmark{

    private MongoClient client;
    private DateTime startTime;
    private StreamWriter outSW;
    public MongoBenchmark(string connectionString, DateTime _startTime){
        this.client = new MongoClient(connectionString);
        this.startTime = _startTime;
        this.RegisterDataModels();
        this.outSW = File.AppendText("mongo_standalone.txt");
        this.outSW.AutoFlush = true;
    }
    ~MongoBenchmark(){
        outSW.Close();
    }
    public void SetupDB(){
        Console.WriteLine("Setting up the DB...");
        CreateCollectionOptions options = new CreateCollectionOptions{TimeSeriesOptions = new TimeSeriesOptions("timestamp")};
        this.client.GetDatabase("benchmark").CreateCollection("metrics", options);
        var model = new CreateIndexModel<Record>("{timestamp: 1}");
        
        this.client.GetDatabase("benchmark").GetCollection<Record>("metrics").Indexes.CreateOne(model);
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
       
        if(!BsonClassMap.IsClassMapRegistered(typeof(Record))){
            Console.WriteLine($"Registering {typeof(Record)} classmap.");
            BsonClassMap.RegisterClassMap<Record>();
        }
        Console.WriteLine();
    }
    public void SequentionalWriteTest(int timePointsCount){
        Console.WriteLine($"Bulk loading {timePointsCount*18} records");
        outSW.WriteLine($"Bulk loading {timePointsCount*18} records");
        BenchmarkDataGenerator gen = new BenchmarkDataGenerator(-1, timePointsCount, startTime, 1);
        
        var collection = this.client.GetDatabase("benchmark").GetCollection<Record>("metrics"); 
        var watch1 = new Stopwatch();
        var watch2 = new Stopwatch();
        int recordNumber = 1;
        watch1.Start();
        foreach (var chunk in gen.GetTimepointChunk()){
            if(recordNumber%1_000 == 0 && recordNumber != 0) Console.WriteLine($"{100.0*recordNumber/timePointsCount:0.00}%");
            watch2.Start();
            collection.InsertMany(chunk);
            watch2.Stop();
            recordNumber += 1;
        }
        watch1.Stop();
        Console.WriteLine("Bulk loading finished.\n");
        Console.WriteLine($"Total time including generating data: {watch1.Elapsed.TotalSeconds}");
        Console.WriteLine($"Total time only inserting data: {watch2.Elapsed.TotalSeconds}");
        
        outSW.WriteLine("Bulk loading finished.");
        //Console.WriteLine($"Total time including generating data: {watch1.Elapsed.TotalSeconds}");
        outSW.WriteLine($"Total time only inserting data: {watch2.Elapsed.TotalSeconds}\n");    
    }
    public void BulkLoad(int timePointsCount, int chunkSize){
    	Console.WriteLine($"Bulk loading {timePointsCount*18} records");
	BenchmarkDataGenerator gen = new BenchmarkDataGenerator(chunkSize, timePointsCount, startTime, 1);
	var collection = this.client.GetDatabase("benchmark").GetCollection<Record>("metrics");
	int chunkNumber = 1;
	foreach (var chunk in gen.GetDataChunk()){
		Console.WriteLine($"{100.0*chunkNumber/gen.chunkCount}");
		collection.InsertMany(chunk);
		chunkNumber += 1;	
	}
	Console.WriteLine("Finished\n");
    
    }
    public void SequentialReadTest(int readCount){
        Console.WriteLine($"Starting sequential read test for {readCount} reads...");
        outSW.WriteLine($"Starting sequential read test for {readCount} reads...");
        Stopwatch watch = new System.Diagnostics.Stopwatch();
        IMongoCollection<Record> collection = this.client.GetDatabase("benchmark").GetCollection<Record>("metrics");
        FilterDefinition<Record> filter;
        for (int i = 0; i < readCount; i++){
            filter = Builders<Record>.Filter.Eq(x => x.ue_data.pci, new Random().Next(3));
            var req = collection.Find(filter).Limit(1);
            watch.Start();
            var doc = req.First();
            watch.Stop();
            if(i%1000 == 0 && i != 0) Console.WriteLine($"Finished {i} reads...");
        }
        Console.WriteLine("Sequential read test finished.");
        Console.WriteLine($"Total time for {readCount} reads: {watch.Elapsed.TotalSeconds} seconds.");
        Console.WriteLine($"Ops/second: {readCount / watch.Elapsed.TotalSeconds}.\n");
        outSW.WriteLine($"Sequential read test: {readCount} reads.");
        outSW.WriteLine($"Total time for {readCount} reads: {watch.Elapsed.TotalSeconds} seconds.");
        outSW.WriteLine($"Ops/second: {readCount / watch.Elapsed.TotalSeconds}.\n");

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
                .Group(r => new {},
                 g => new { avg = g.Average(x => x.ue_data.dlul_brate) });
                
            
            Console.WriteLine(q);
            //var test = collection.Find(dateRangeFilter);
            watch.Start();
            var res = q.First();
            watch.Stop();
	    Console.WriteLine(res);
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
                .Group(g => new {},
                g => new { avg = g.Average(x => x.ue_data.dlul_brate) });
            watch.Start();
            var res = q.First();
            watch.Stop();
	    Console.WriteLine(res);
            if(i%1_000 == 0 && i != 0) Console.WriteLine($"Finished {i} queries...");
        }

        Console.WriteLine("Aggregation test finished.");
        Console.WriteLine($"Total time for {queryCount} queries: {watch.Elapsed.TotalSeconds} seconds.");
        Console.WriteLine($"Ops/second: {queryCount / watch.Elapsed.TotalSeconds}.\n");
    }

}
