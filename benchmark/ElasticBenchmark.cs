using System.Diagnostics;
using Elasticsearch.Net;
using Models;
using Nest;

public class ElasticBenchmark : IDatabaseBenchmark{

    private ElasticClient client;
    private DateTime startTime;
    private StreamWriter outSW;
    private bool writeToFile;
    public ElasticBenchmark(Uri clientUri, DateTime _startTime, bool writeToFile = false){
        this.startTime = _startTime;
	    var clientSettings = new ConnectionSettings(clientUri).DefaultIndex("metrics");
        this.client = new ElasticClient(clientSettings);
        this.writeToFile = writeToFile;
        if(writeToFile){
            outSW = File.AppendText("elastic_standalone.txt");
            outSW.AutoFlush = true;
        }
    }

    public ElasticBenchmark(IEnumerable<Uri> nodeUris, DateTime _startTime){
        this.startTime = _startTime;
        var pool = new StaticConnectionPool(nodeUris);
        var clientSettings = new ConnectionSettings(pool).DefaultIndex("metrics");
        
        client = new ElasticClient(clientSettings);
        outSW = File.AppendText("elastic_cluster.txt");
        outSW.AutoFlush = true;
    }
    ~ElasticBenchmark(){
    	outSW.Close();
    } 
    public void SetupDB(){
        this.client.Indices.Create("metrics", 
            c => 
                c.Map<UEData>(m => m.AutoMap())
                .Map<Record>(m => m.AutoMap()));
    }
    public void ResetDB(){
        this.client.Indices.Delete("metrics");
    }
   
    public async void SequentialWriteTest(int timePointsCount, int dt){
    	Console.WriteLine($"Write test: {timePointsCount*18} records...");
        if(writeToFile) outSW.WriteLine($"Write test: {timePointsCount*18} records...");
        BenchmarkDataGenerator gen = new BenchmarkDataGenerator(-1, timePointsCount, startTime, dt);
        var watch = new Stopwatch();
        int recordNumber = 1;
        var searchTasks = new List<Task<BulkResponse>>();
        foreach (var chunk in gen.GetTimepointChunk()){
            if(recordNumber%1_000 == 0 && recordNumber != 0) Console.WriteLine($"{100.0*recordNumber/timePointsCount:0.00}%");
            //watch.Start();
            searchTasks.Add(this.client.IndexManyAsync(chunk));
            //watch.Stop();
            recordNumber += 1;
        }
        var res = await Task.WhenAll(searchTasks);	
        foreach (var bulkResponse in res)
        {
            if (bulkResponse.IsValid)
            {
                Console.WriteLine("IndexMany request successful.");
                // Process the bulk response if needed
            }
            else
            {
                Console.WriteLine($"Error indexing documents: {bulkResponse.DebugInformation}");
            }
        }
        outSW.WriteLine($"Write test of {timePointsCount*18} records finished.");
        outSW.WriteLine($"Total time only inserting data: {watch.Elapsed.TotalSeconds}\n");
        Console.WriteLine($"Write test finished after {watch.Elapsed.TotalSeconds} seconds");
    }
    public void BulkLoad(int timePointsCount, int chunkSize){
        Console.WriteLine($"Bulk loading {timePointsCount*18} records in chunks of {chunkSize*18}...");
        BenchmarkDataGenerator gen = new BenchmarkDataGenerator(chunkSize, timePointsCount, startTime,1);
        int chunkNumber = 1;
        Stopwatch watch1 = new Stopwatch();
        Stopwatch watch2 = new Stopwatch();
        watch1.Start();
        foreach (var data in gen.GetDataChunk()){
            Console.WriteLine($"{100.0*chunkNumber/gen.chunkCount:0.00}%");
            foreach(var r in data){
                watch2.Start();
                var t = this.client.IndexDocument(r);
                watch2.Stop();
            }
            chunkNumber += 1;
        }
        watch1.Stop();
        outSW.WriteLine($"Bulk load {timePointsCount*18} records, chunks of {chunkSize*18}\n");
        outSW.WriteLine($"Total time including generating data: {watch1.Elapsed.TotalSeconds}");
        outSW.WriteLine($"Total time only inserting data: {watch2.Elapsed.TotalSeconds}\n");
        Console.WriteLine("Bulk loading finished.\n");
    }
    public void SequentialReadTest(int readCount){
        Console.WriteLine($"Starting sequential read test for {readCount} reads...");
        Stopwatch watch = new System.Diagnostics.Stopwatch();
        for (int i = 0; i < readCount; i++){
            var request = new SearchRequest<Record>(){
                Query = new TermQuery(){
                    Field = Infer.Field<Record>(r => r.ue_data.pci),
                    Value = new Random().Next(3)
                },
                From = 0,
		Size = 1
            };
        
            watch.Start();
            var res = this.client.Search<Record>(request);
            watch.Stop();
            if(i%10_000 == 0 && i != 0) Console.WriteLine($"Finished {i} reads...");
        }
        Console.WriteLine("Sequential read test finished.");
        outSW.WriteLine($"Sequential read test: {readCount} reads.");
        outSW.WriteLine($"Total time for {readCount} reads: {watch.Elapsed.TotalSeconds} seconds.");
        outSW.WriteLine($"Ops/second: {readCount / watch.Elapsed.TotalSeconds}.\n");

    }
    public void AggregationTest(int queryCount){
        var watch = new System.Diagnostics.Stopwatch();
        var endTimeOneWeek = startTime.AddDays(7);
        var endTimeTwoWeeks = startTime.AddDays(14);
        Console.WriteLine($"Aggregation test: {queryCount} queries, one week");
        Console.WriteLine($"Starting aggregation test with {queryCount} queries ({startTime} - {endTimeOneWeek}...)");
        outSW.WriteLine($"Aggregation test: {queryCount} queries, one week");
        outSW.WriteLine($"Starting aggregation test with {queryCount} queries ({startTime} - {endTimeOneWeek}...)");
        
	for (int i = 0; i < queryCount; i++){
            var query = new SearchRequest<Record>(){
                Query = new DateRangeQuery{
                    Field = Infer.Field<Record>(r => r.timestamp),
                    GreaterThanOrEqualTo = startTime.ToUniversalTime(),
                    LessThanOrEqualTo = endTimeOneWeek.ToUniversalTime()
                } & new ExistsQuery{
                    Field = Infer.Field<Record>(r => r.ue_data)
               	},
		 Aggregations = new AverageAggregation("avg", Infer.Field<Record>(r => r.ue_data.dlul_brate)),
		 Size = 0
		
            };
            
            watch.Start();
            var res = this.client.Search<Record>(query);
            watch.Stop();
            Console.WriteLine(res.Aggregations.Average("avg").Value);
	    if(i%1_000 == 0 && i != 0) Console.WriteLine($"Finished {i} queries...");
        }

        Console.WriteLine("Aggregation test finished.");
        Console.WriteLine($"Total time for {queryCount} queries: {watch.Elapsed.TotalSeconds} seconds.");
        Console.WriteLine($"Ops/second: {queryCount / watch.Elapsed.TotalSeconds}.\n");
        Console.WriteLine($"Starting aggregation test with {queryCount} querries ({startTime} - {endTimeTwoWeeks}...)");
       	outSW.WriteLine("Aggregation test finished.");
        outSW.WriteLine($"Total time for {queryCount} queries: {watch.Elapsed.TotalSeconds} seconds.");
        outSW.WriteLine($"Ops/second: {queryCount / watch.Elapsed.TotalSeconds}.\n");
        outSW.WriteLine($"Starting aggregation test with {queryCount} querries ({startTime} - {endTimeTwoWeeks}...)");
	
	watch.Reset();
        for (int i = 0; i < queryCount; i++){
            var query = new SearchRequest<Record>(){
                Query = new DateRangeQuery{
                    Field = Infer.Field<Record>(r => r.timestamp),
                    GreaterThanOrEqualTo = startTime.ToUniversalTime(),
                    LessThanOrEqualTo = endTimeTwoWeeks.ToUniversalTime()
                } & new ExistsQuery{
                    Field = Infer.Field<Record>(r => r.ue_data)
               	},
		 Aggregations = new AverageAggregation("avg", Infer.Field<Record>(r => r.ue_data.dlul_brate)),
		 Size = 0
		
            };
            
            watch.Start();
            var res = this.client.Search<Record>(query);
            watch.Stop();
            Console.WriteLine(res.Aggregations.Average("avg").Value);
	    if(i%1_000 == 0 && i != 0) Console.WriteLine($"Finished {i} queries...");
        }           
        Console.WriteLine("Aggregation test finished.");
        Console.WriteLine($"Total time for {queryCount} queries: {watch.Elapsed.TotalSeconds} seconds.");
        Console.WriteLine($"Ops/second: {queryCount / watch.Elapsed.TotalSeconds}.\n");
        outSW.WriteLine("Aggregation test finished.");
        outSW.WriteLine($"Total time for {queryCount} queries: {watch.Elapsed.TotalSeconds} seconds.");
        outSW.WriteLine($"Ops/second: {queryCount / watch.Elapsed.TotalSeconds}.\n");
        

    }

}
