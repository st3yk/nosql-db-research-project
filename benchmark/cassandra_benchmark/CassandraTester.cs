using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualBasic;

using System.Data;
using Cassandra;
using Cassandra.Mapping;
using Cassandra.Data.Linq;
using System.CodeDom;
using System.Threading.Tasks;

public class CassandraTester{
  
    const string KEY_SPACE = "benchmark";
    ISession session;
    public CassandraTester(List<string> contactPoints, List<string> ports){
        Console.WriteLine(contactPoints.Count());
        var cluster = Cluster.Builder()
            .AddContactPoints(contactPoints)
            .WithQueryOptions(new QueryOptions().SetConsistencyLevel(ConsistencyLevel.One))
            .WithSocketOptions(new SocketOptions().SetReadTimeoutMillis(60000))
            .Build();
        

        session = cluster.Connect();
        session.Execute($"CREATE KEYSPACE IF NOT EXISTS {KEY_SPACE}" + " WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 3}");
        session.Execute($"USE {KEY_SPACE}");
        CreateTable();
        
    }
    public void DropTableIfExists()
    {
        session.Execute($"DROP TABLE IF EXISTS RecordCassandra");
        session.Execute("DROP TYPE IF EXISTS RecordCassandra");

        session.Execute("DROP TABLE IF EXISTS BSData");
        session.Execute("DROP TABLE IF EXISTS UEData");
        session.Execute("DROP TYPE IF EXISTS BSData");
        session.Execute("DROP TYPE IF EXISTS BSData");
    }

    public void CreateTable()
    {
        var table1 = new Table<UEData>(session);
        table1.CreateIfNotExists();
        var table2 = new Table<BSData>(session);
        table2.CreateIfNotExists();
    }


    public void BulkLoadTest(int wholeSize, int batchSize, int step = 1){
        Stopwatch stopwatch1 = new Stopwatch();
        Stopwatch stopwatch2 = new Stopwatch();
        TaskScheduler taskScheduler = TaskScheduler.Default;
        
        List<Task> list = new List<Task>();
        DateTime start = DateTime.ParseExact("20230501T00:00:00Z", "yyyyMMddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime();
        stopwatch1.Start();
        for(int i = 0; i < wholeSize/batchSize; i++){
            stopwatch2.Start();
            var batch = GenerateTestData(batchSize, start.AddSeconds(i), "UEDATA", step);
            var xd = session.Execute(batch);
            stopwatch2.Stop();
        }
        stopwatch1.Stop();
        var ueData = new Table<UEData>(session);
        
        
        Console.WriteLine($"Bulk write {wholeSize} Whole time taken: {stopwatch1.Elapsed.TotalSeconds} seconds; Insert only time {stopwatch2.Elapsed.TotalSeconds}s");
    }

     public BatchStatement GenerateTestData(int count, DateTime start, string UeDataTableName, int step = 1){
        BatchStatement batch = new BatchStatement();
        var random = new Random();
        for (var i = 0; i < count; i++){
            
            for (int bs_id = 0; bs_id < 3; bs_id++){

                for (int ue_id = 0; ue_id < 5; ue_id++){
                    UEData uEData = new UEData{
                                timestamp_column = start.AddSeconds(step).ToUniversalTime(), 
                                ue_id = ue_id + bs_id * 5,
                                pci = bs_id 
                    };
                    SimpleStatement simpleStatement =  new SimpleStatement($"INSERT INTO {UeDataTableName} ( timestamp_column, ue_id, cc , pci, earfcn, rsrp, pl, cfo, dl_mcs,ul_mcs, dl_brate, ul_brate, dl_bler, ul_bler, dl_snr , ul_buff) VALUES ( ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                     uEData.timestamp_column , uEData.ue_id, uEData.cc , uEData.pci, uEData.earfcn, uEData.rsrp, uEData.pl, uEData.cfo, uEData.dl_mcs,uEData.ul_mcs, uEData.dl_brate, uEData.ul_brate, uEData.dl_bler, uEData.ul_bler, uEData.dl_snr , uEData.ul_buff);
                    batch.Add(simpleStatement);
                }
            }
                  
        }

        return batch;
    }
    public void AggregationTest(int n = 1, bool two_weeks = true){
        var watch = new Stopwatch();
        DateTime start_date = DateTime.ParseExact("20230501T00:00:00Z", "yyyyMMddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime();

        var end_date = two_weeks ? start_date.AddDays(14):start_date.AddDays(7);
        Console.WriteLine($"Starting aggregation test with {n} querries ({start_date} - {end_date}...)");

        var ueData = new Table<UEData>(session);
        var bsData = new Table<BSData>(session);

        var ueDataLength = ueData.Count().Execute();
        var bsDataLength = bsData.Count().Execute();
        Console.WriteLine($"{ueDataLength}, {bsDataLength}");
        DateTimeOffset start = new DateTimeOffset(start_date);
        DateTimeOffset end = new DateTimeOffset(end_date);
       
        for (int i = 0; i < n; i++){          
            watch.Start();
            
            var query = new SimpleStatement("SELECT AVG(dl_brate) as average FROM UEDATA where ue_id = 0 and timestamp_column >= ? and timestamp_column <= ? ", start, end);

            var rows = session.Execute(query);
            watch.Stop();
            var row = rows.First();
            var x2 = row.GetValue<double>("average");
            
            //Console.WriteLine($"Average: {average}");
            
        }
        Console.WriteLine($"AggregationTest: Whole {n} loops with 'two_weeks' set to {two_weeks} was done in {watch.Elapsed.TotalSeconds} seconds.");


    }
    public async void BulkReadTest(int n = 100){
        var watch = new Stopwatch();
        Random random = new Random();
        watch.Start();
        List<Task> list = new List<Task>();
        for (int i = 0; i < n; i++){      
            var statement = new SimpleStatement($"SELECT dl_brate  FROM UEDATA WHERE  ue_id = ?  LIMIT 1", random.Next(0,15));
            list.Add(session.ExecuteAsync(statement));            
            
        }
        await Task.WhenAll(list);
        watch.Stop();
        Console.WriteLine($"BulkReadTest: Whole {n} loops was done in {watch.Elapsed.TotalSeconds} seconds.");
    }

     
}