using System.Diagnostics;
using System.Text.Json;
using System.Xml;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using Elastic.Transport.Extensions;
using Models;
using MongoDB.Bson;
using MongoDB.Bson.IO;

public class ElasticBenchmark : IDatabaseBenchmark{

    private ElasticsearchClient client;
    private DateTime startTime;
    private StreamWriter outSW;
    public ElasticBenchmark(Uri clientUri, DateTime _startTime){
        this.startTime = _startTime;
        this.client = new ElasticsearchClient(clientUri);
        outSW = File.AppendText("elastic_standalone.txt");
        outSW.AutoFlush = true;
    }

    public ElasticBenchmark(IEnumerable<Uri> nodeUris, DateTime _startTime){
        this.startTime = _startTime;
        var pool = new StaticNodePool(nodeUris);
        var clientSettings = new ElasticsearchClientSettings(pool);
        client = new ElasticsearchClient(clientSettings);
        outSW = File.AppendText("elastic2.txt");
        outSW.AutoFlush = true;
        var request = new SearchRequest<Record>("metrics"){
                Query = new DateRangeQuery(Infer.Field<Record>(r => r.timestamp)){
                        Gte = startTime,
                        Lte = _startTime.AddDays(7)
                    } & new ExistsQuery(){
                        Field = Infer.Field<Record>(r => r.ue_data)
                    },
                Aggregations = new AverageAggregation("avg", Infer.Field<Record>(r => r.ue_data.dlul_brate))
            
            };
        Console.WriteLine(this.client.RequestResponseSerializer.SerializeToString(request));
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
            var request = new SearchRequest<Record>("metrics"){
                Query = new MatchQuery(Infer.Field<Record>(r => r.bs_data.bs_id)){
                    Query = new Random().Next(3).ToString()
                }
            };
        
            watch.Start();
            var doc = this.client.Search<Record>(request);
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
        for (int i = 0; i < queryCount; i++){
            // var request = new SearchRequest<Record>("metrics"){
            //     Query = new DateRangeQuery(Infer.Field<Record>(r => r.timestamp)){
            //             Gte = startTime.ToUniversalTime(),
            //             Lte = endTimeOneWeek.ToUniversalTime()
            //         } & new ExistsQuery(){
            //             Field = Infer.Field<Record>(r => r.ue_data)
            //         },
            //     Aggregations = new AverageAggregation("avg", Infer.Field<Record>(r => r.ue_data.dlul_brate))
            
            // };
            var query = new SearchRequest("metrics"){
                Aggregations = new DateRangeAggregation("date_ranges"){
                    Field = Infer.Field<Record>(r => r.timestamp),
                    Ranges = new List<DateRangeExpression>{
                        new DateRangeExpression(){From = new FieldDateMath(DateMath.Anchored(startTime)), To = new FieldDateMath(DateMath.Anchored(endTimeOneWeek))}
                    },
                    Aggregations = new AverageAggregation("avg", Infer.Field<Record>(r => r.ue_data.dlul_brate))
                },
                Query  = new ExistsQuery(){Field = Infer.Field<Record>(r => r.ue_data)},
                
            };
            this.client.RequestResponseSerializer.SerializeToString(query);
            watch.Start();
            var res = this.client.Search<Record>(query);
            Console.WriteLine(res.Aggregations.GetDateRange("date_ranges").Meta);
            watch.Stop();
            if(i%1_000 == 0 && i != 0) Console.WriteLine($"Finished {i} queries...");
        }

        Console.WriteLine("Aggregation test finished.");
        Console.WriteLine($"Total time for {queryCount} queries: {watch.Elapsed.TotalSeconds} seconds.");
        Console.WriteLine($"Ops/second: {queryCount / watch.Elapsed.TotalSeconds}.\n");
        Console.WriteLine($"Starting aggregation test with {queryCount} querries ({startTime} - {endTimeTwoWeeks}...)");
        Console.WriteLine($"Aggregation test: {queryCount} queries, one week");
        watch.Reset();
        for (int i = 0; i < queryCount; i++){
            var request = new SearchRequest<Record>("metrics"){
                Query = new DateRangeQuery(Infer.Field<Record>(r => r.timestamp)){
                        Gte = startTime.ToUniversalTime(),
                        Lte = endTimeTwoWeeks.ToUniversalTime()
                    } & new ExistsQuery(){
                        Field = Infer.Field<Record>(r => r.ue_data)
                    },
                Aggregations = new AverageAggregation("avg", Infer.Field<Record>(r => r.ue_data.dlul_brate)){

                }
            
            };
    
            watch.Start();
            var res = this.client.Search<Record>(request);
            watch.Stop();
            if(i%1_000 == 0 && i != 0) Console.WriteLine($"Finished {i} queries...");
        }

        Console.WriteLine("Aggregation test finished.");
        Console.WriteLine($"Total time for {queryCount} queries: {watch.Elapsed.TotalSeconds} seconds.");
        Console.WriteLine($"Ops/second: {queryCount / watch.Elapsed.TotalSeconds}.\n");
        

    }

}