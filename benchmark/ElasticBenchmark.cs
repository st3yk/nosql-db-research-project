using System.Diagnostics;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using Models;

public class ElasticBenchmark : IDatabaseBenchmark{

    private ElasticsearchClient client;
    private DateTime startTime;
    private StreamWriter outSW;
    public ElasticBenchmark(Uri clientUri, DateTime _startTime){
        this.startTime = _startTime;
        this.client = new ElasticsearchClient(clientUri);
        outSW = File.AppendText("elastic.txt");
        outSW.AutoFlush = true;
    }

    public ElasticBenchmark(IEnumerable<Uri> nodeUris, DateTime _startTime){
        this.startTime = _startTime;
        var pool = new StaticNodePool(nodeUris);
        var clientSettings = new ElasticsearchClientSettings(pool);
        client = new ElasticsearchClient(clientSettings);
        outSW = File.AppendText("elastic.txt");
        outSW.AutoFlush = true;
    }
   
    public void SetupDB(){
        this.client.Indices.Create("metrics");
    }

    public void ResetDB(){
        this.client.Indices.Delete("metrics");
    }
    public void BulkLoad(int timePointsCount, int chunkSize){
        Console.WriteLine($"Bulk loading {timePointsCount*18} records in chunks of {chunkSize*18}...");
        BenchmarkDataGenerator gen = new BenchmarkDataGenerator(chunkSize, timePointsCount, startTime);
        int chunkNumber = 1;
        Stopwatch watch1 = new Stopwatch();
        Stopwatch watch2 = new Stopwatch();
        watch1.Start();
        foreach (var data in gen.GetDataChunk()){
            Console.WriteLine($"{100.0*chunkNumber/gen.chunkCount:0.00}%");
            watch2.Start();
            this.client.IndexMany(data, "metrics");
            watch2.Stop();
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
            watch.Start();
            var doc = this.client.Get<Record>(1, idx => idx.Index("metrics"));
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
        outSW.WriteLine($"Aggregation test: {queryCount} queries, one week");
        Console.WriteLine($"Starting aggregation test with {queryCount} queries ({startTime} - {endTimeOneWeek}...)");
        for (int i = 0; i < queryCount; i++){
            var request = new SearchRequest<Record>("metrics"){
                Query = new DateRangeQuery(Infer.Field<Record>(r => r.timestamp)){
                        Gte = startTime,
                        Lte = endTimeOneWeek
                    } & new ExistsQuery(){
                        Field = Infer.Field<Record>(r => r.ue_data)
                    },
                Aggregations = new AverageAggregation("avg", Infer.Field<Record>(r => r.ue_data.pci))
            
            };
    
            watch.Start();
            var res = this.client.Search<Record>(request);
            watch.Stop();
            if(i%1_000 == 0 && i != 0) Console.WriteLine($"Finished {i} queries...");
        }

        Console.WriteLine("Aggregation test finished.");
        outSW.WriteLine($"Total time for {queryCount} queries: {watch.Elapsed.TotalSeconds} seconds.");
        outSW.WriteLine($"Ops/second: {queryCount / watch.Elapsed.TotalSeconds}.\n");
        Console.WriteLine($"Starting aggregation test with {queryCount} querries ({startTime} - {endTimeTwoWeeks}...)");
        outSW.WriteLine($"Aggregation test: {queryCount} queries, one week");
        for (int i = 0; i < queryCount; i++){
            var request = new SearchRequest<Record>("metrics"){
                Query = new DateRangeQuery(Infer.Field<Record>(r => r.timestamp)){
                        Gte = startTime,
                        Lte = endTimeTwoWeeks
                    } & new ExistsQuery(){
                        Field = Infer.Field<Record>(r => r.ue_data)
                    },
                Aggregations = new AverageAggregation("avg", Infer.Field<Record>(r => r.ue_data.pci))
            
            };
    
            watch.Start();
            var res = this.client.Search<Record>(request);
            watch.Stop();
            if(i%1_000 == 0 && i != 0) Console.WriteLine($"Finished {i} queries...");
        }
    }

}