using System;
using System.Diagnostics;
using System.Collections.Generic;
using Cassandra;
using Models;
using System.Data;

public class CassandraTester : DataBaseTestingInterface{
    const string KEY_SPACE = "benchmark";
    const string TABLE_NAME = "metrics"; 
    int batchSize = 500;
    ISession session;
    public void AggregationTest(){
        
    }
    public CassandraTester(List<string> contactPoints, List<string> ports){
        var cluster = Cluster.Builder()
            .AddContactPoints(contactPoints[0]) // Replace with your Cassandra server's address
            .Build();
        //Create a session (similar to a database connection)
        session = cluster.Connect();
        // Create keyspace
        session.Execute("CREATE KEYSPACE IF NOT EXISTS mykeyspace WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1}");
        // Use keyspace
        session.Execute("USE mykeyspace");

        // Drop table if exists
        DropTableIfExists(session);
        CreateTypes(session);
        // Create table
        CreateTable(session);
        session.UserDefinedTypes.Define(UdtMap.For<BSData>()
            .Map(v => v.bs_id, "bs_id")
            .Map(v => v.nof_ue, "nof_ue")
            .Map(v => v.dlul_brate, "dlul_brate"));
        session.UserDefinedTypes.Define(UdtMap.For<UEData>()
            .Map(v => v.ue_id, "ue_id")
            .Map(v => v.cc, "cc")
            .Map(v => v.pci, "pci")
            .Map(v => v.earfcn, "earfcn")
            .Map(v => v.rsrp, "rsrp")
            .Map(v => v.pl, "pl")
            .Map(v => v.cfo, "cfo")
            .Map(v => v.dlul_mcs, "dlul_mcs")
            .Map(v => v.dlul_brate, "dlul_brate")
            .Map(v => v.dlul_bler, "dlul_bler")
            .Map(v => v.dl_snr, "dl_snr")
            .Map(v => v.ul_buff, "ul_buff")
            );
        this.BulkLoadTest();
    }
    void DropTableIfExists(ISession session)
    {
        session.Execute("DROP TABLE IF EXISTS example_table");
        session.Execute("DROP TYPE IF EXISTS ADDRESS");
    }

    void CreateTable(ISession session)
    {
        // Define your table schema
        var createTableQuery = "CREATE TABLE example_table (id UUID PRIMARY KEY, timestamp timestamp, bs_data FROZEN <BSData>, ue_data FROZEN <UEData>)";
        session.Execute(createTableQuery);
    }

    void CreateTypes(ISession session)
    {
        // Define your user-defined types
        var createTypeQuery = "CREATE TYPE IF NOT EXISTS mykeyspace.UEData (ue_id int, cc int, pci int, earfcn int, rsrp int, pl int, cfo double, dlul_mcs double"
        + ", dlul_brate double,  dlul_bler double, dl_snr double, ul_buff double)";
        session.Execute(createTypeQuery);

        createTypeQuery = "CREATE TYPE IF NOT EXISTS mykeyspace.BSData (bs_id int, nof_ue int, dlul_brate double)";
        session.Execute(createTypeQuery);

    }

    public void BulkLoadTest(){
        // Create a Stopwatch instance
        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();
        ParallelDataGenerator<BatchStatement> generator = new ParallelDataGenerator<BatchStatement>(6, GenerateTestData);
        for(int i = 0; i < 100; i++){
            var batch = generator.Get();
           
            this.session.Execute(batch);
            
        }
        stopwatch.Stop();
        TimeSpan elapsedTime = stopwatch.Elapsed;
        Console.WriteLine($"Time taken: {elapsedTime.TotalMilliseconds} milliseconds");
    }
    public void AggregationTest(int n, bool two_weeks){
        // var watch = new Stopwatch();
        // var end_date = two_weeks ? start_date.AddDays(14):start_date.AddDays(7);
        // Console.WriteLine($"Starting aggregation test with {n} querries ({start_date} - {end_date}...)");
        // var filter_builder = Builders<Record>.Filter;
        // var query = filter_builder.Gte(x => x.timestamp, start_date) & filter_builder.Lte(x => x.timestamp, end_date) & filter_builder.Exists(x => x.UEData);
        // double time_sum = 0;
        // for (int i = 0; i < n; i++){
        //     var db = clients[0].GetDatabase("benchmark");
        //     var collection = db.GetCollection<Record>("metrics");
        //     watch.Start();
        //     var res = collection.Aggregate<Record>()
        //     .Match(query)
        //     .Group(g => g.UEData.pci, // Group by a constant value (1) or any specific field
        //         g => new { avg = g.Average(x => x.UEData.dlul_brate) }).ToBsonDocument();//g.Average(x => x.Field1) });
        //     watch.Stop();
        //     Console.WriteLine(res);
        //     time_sum += watch.Elapsed.TotalSeconds;
        // }
        // Console.WriteLine($"Test finished after {time_sum}seconds.");
        // Console.WriteLine($"Average delay: {time_sum/n} seconds per read.");
    }
    public void BulkReadTest(){}

    static public BatchStatement GenerateTestData(){
        int count = 10;
        DateTime start = DateTime.Now;
        List<Record> records  = MongoDbTester.GenerateTestData(count, start);
        var batch = new BatchStatement();
        foreach(Record record in records){
            SimpleStatement simpleStatement =  new SimpleStatement("INSERT INTO mykeyspace.example_table (id, timestamp, bs_data, ue_data) VALUES (?,  ?,  ?, ?)", 
                Guid.NewGuid(), record.timestamp, record.BSData, record.UEData);
            batch.Add(simpleStatement);
        }
        return batch;
    }
}