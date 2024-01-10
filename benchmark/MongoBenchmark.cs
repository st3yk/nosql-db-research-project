using System.Diagnostics;
using Elastic.Clients.Elasticsearch.Core.GetScriptContext;
using Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

public class ConnectionThrottlingPipeline
{
    private readonly Semaphore openConnectionSemaphore;

    public ConnectionThrottlingPipeline( IMongoClient client )
    {
        //Only grabbing half the available connections to hedge against collisions.
        //If you send every operation through here
        //you should be able to use the entire connection pool.
        openConnectionSemaphore = new Semaphore( client.Settings.MaxConnectionPoolSize / 2,
            client.Settings.MaxConnectionPoolSize / 2 );
    }

    public async Task<T> AddRequest<T>( Func<Task<T>> task )
    {
        openConnectionSemaphore.WaitOne();
        try
        {
            var result = await task();
            return result;
        }
        finally
        {
            openConnectionSemaphore.Release();
        }
    }
}
public class MongoBenchmark : IDatabaseBenchmark{

    private MongoClient client;
    private DateTime startTime;
    private StreamWriter? outSW;
    private bool writeToFile;
    public MongoBenchmark(string connectionString, DateTime _startTime, bool writeToFile){
        var settings = MongoClientSettings.FromConnectionString(connectionString);
	settings.MinConnectionPoolSize = 100;
	settings.MaxConnectionPoolSize = 10000;
	this.client = new MongoClient(connectionString);
        this.startTime = _startTime;
        this.RegisterDataModels();
        this.writeToFile = writeToFile;
        if(writeToFile){
            this.outSW = File.AppendText("mongo_standalone_agg.txt");
            this.outSW.AutoFlush = true;
        }
    }
    ~MongoBenchmark(){
        if(this.writeToFile) outSW.Close();
    }
    public void SetupDB(){
        Console.WriteLine("Setting up the DB...");
        CreateCollectionOptions options = new CreateCollectionOptions{TimeSeriesOptions = new TimeSeriesOptions("timestamp", "ue_data")};
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
    public void SequentialWriteTest(int timePointsCount, int dt){
        
    	Console.WriteLine($"Write test: {timePointsCount*15} records...");
        if(this.writeToFile) outSW.WriteLine($"Write test: {timePointsCount*15} records...");
        BenchmarkDataGenerator gen = new BenchmarkDataGenerator(-1, timePointsCount, startTime, dt);
        var collection = this.client.GetDatabase("benchmark").GetCollection<Record>("metrics"); 
        var watch = new Stopwatch();
        int recordNumber = 1;
        foreach (var chunk in gen.GetTimepointChunk()){
            if(recordNumber%1_000 == 0 && recordNumber != 0) Console.WriteLine($"{100.0*recordNumber/timePointsCount:0.00}%");
            watch.Start();
            collection.InsertMany(chunk);
            watch.Stop();
            recordNumber += 1;
        }
        if(this.writeToFile){
            outSW.WriteLine($"Write test of {timePointsCount*15} records finished.");
            outSW.WriteLine($"Total time only inserting data: {watch.Elapsed.TotalSeconds}\n");
        }
        Console.WriteLine($"Write test finished after {watch.Elapsed.TotalSeconds} seconds");
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
       	Stopwatch watch = new System.Diagnostics.Stopwatch();
        var searchRequests = new List<Task<IAsyncCursor<Record>>>();
        watch.Start();
        IMongoCollection<Record> collection = this.client.GetDatabase("benchmark").GetCollection<Record>("metrics");
        FilterDefinition<Record> filter;
        var pipeline = new ConnectionThrottlingPipeline(this.client);
        for (int i = 0; i < readCount; i++){
            filter = Builders<Record>.Filter.Eq(x => x.timestamp, startTime.AddSeconds(new Random().Next(10_000)));
            searchRequests.Add(pipeline.AddRequest(() => collection.FindAsync(filter, new FindOptions<Record>(){Limit=1})));
            if(i%10000 == 0 && i != 0) Console.WriteLine($"Finished {i} reads...");
        }
        var tasks = Task.WhenAll(searchRequests);
        tasks.Wait();
        watch.Stop();
	
        Console.WriteLine("Sequential read test finished.");
        Console.WriteLine(watch.Elapsed.TotalSeconds);
        if(this.writeToFile){
                outSW.WriteLine($"Sequential read test: {readCount} reads.");
                outSW.WriteLine($"Total time for {readCount} reads: {watch.Elapsed.TotalSeconds} seconds.");
                outSW.WriteLine($"Ops/second: {readCount / watch.Elapsed.TotalSeconds}.\n");
        }

    }
    public void ReadAllTest(){
        var watch = new Stopwatch();
        var collection = this.client.GetDatabase("benchmark").GetCollection<Record>("metrcis");
        
        watch.Start();
        var cursor = collection.Find(_ => true).ToCursor();   
        while(cursor.MoveNext()){
            Console.WriteLine(cursor.Current.Count());
        }
        watch.Stop();
        Console.WriteLine($"Write all test, time: {watch.Elapsed.TotalSeconds} seconds");
    }
    public void AggregationTest(int queryCount){
        var watch = new System.Diagnostics.Stopwatch();
        var endTimeOneWeek = startTime.AddDays(7);
        var endTimeTwoWeeks = startTime.AddDays(14);
        Console.WriteLine($"Aggregation test: {queryCount} queries, one week");
        Console.WriteLine($"Starting aggregation test with {queryCount} queries ({startTime} - {endTimeOneWeek}...)");
        if(this.writeToFile) outSW.WriteLine($"Aggregation test: {queryCount} queries, one week");
        if(this.writeToFile) outSW.WriteLine($"Starting aggregation test with {queryCount} queries ({startTime} - {endTimeOneWeek}...)");

        var filter_builder = Builders<Record>.Filter;
        var dateRangeFilter = filter_builder.Gte(x => x.timestamp, startTime) & filter_builder.Lte(x => x.timestamp, endTimeOneWeek);
        
        var collection = this.client.GetDatabase("benchmark").GetCollection<Record>("metrics");
        for (int i = 0; i < queryCount; i++){
            var q = collection.Aggregate()
                .Match(dateRangeFilter)
                .Group(r => new {},
                 g => new { avg = g.Average(x => x.ue_data.dl_brate) });
                
            
            Console.WriteLine(q);
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
       	if(this.writeToFile){
		    outSW.WriteLine("Aggregation test finished.");
        	outSW.WriteLine($"Total time for {queryCount} queries: {watch.Elapsed.TotalSeconds} seconds.");
        	outSW.WriteLine($"Ops/second: {queryCount / watch.Elapsed.TotalSeconds}.\n");
        	outSW.WriteLine($"Starting aggregation test with {queryCount} querries ({startTime} - {endTimeTwoWeeks}...)");
	    }
        dateRangeFilter = filter_builder.Gte(x => x.timestamp, startTime) & filter_builder.Lte(x => x.timestamp, endTimeTwoWeeks);
        watch.Reset();
        for (int i = 0; i < queryCount; i++){
            var q = collection.Aggregate()
                .Match(dateRangeFilter)
                .Group(g => new {},
                g => new { avg = g.Average(x => x.ue_data.dl_brate) });
            watch.Start();
            var res = q.First();
            watch.Stop();
	        Console.WriteLine(res);
            if(i%1_000 == 0 && i != 0) Console.WriteLine($"Finished {i} queries...");
        }

        Console.WriteLine("Aggregation test finished.");
        Console.WriteLine($"Total time for {queryCount} queries: {watch.Elapsed.TotalSeconds} seconds.");
        Console.WriteLine($"Ops/second: {queryCount / watch.Elapsed.TotalSeconds}.\n");
        if(this.writeToFile){
		    outSW.WriteLine("Aggregation test finished.");
        	outSW.WriteLine($"Total time for {queryCount} queries: {watch.Elapsed.TotalSeconds} seconds.");
        	outSW.WriteLine($"Ops/second: {queryCount / watch.Elapsed.TotalSeconds}.\n");
	    }
    }

}
