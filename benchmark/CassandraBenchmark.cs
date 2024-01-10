
using System.Diagnostics;
using Cassandra;
using Cassandra.Data.Linq;

public class CassandraBenchmark : IDatabaseBenchmark
{
    const string KEY_SPACE = "benchmark";
    ISession session;
    
    private DateTime startDate;

    public CassandraBenchmark(List<string> contactPoints){
        Console.WriteLine(contactPoints.Count());
        var cluster = Cluster.Builder()
            .AddContactPoint(contactPoints[0]) // Replace with your Cassandra server's address
            .Build();
        //Create a session (similar to a database connection)
        session = cluster.Connect();
        
    }

    public void BulkLoad(int timePointsCount, int chunkSize)
    {
        Stopwatch watch = new Stopwatch();
        DateTime timestamp = startDate;
        for(int i = 0; i < timePointsCount; i++){
            var batch = GenerateTestData(1,  timestamp, "UEDATA");
            watch.Start();
            session.Execute(batch);
            watch.Stop();   
            timestamp = timestamp.AddSeconds(1);    
        }
        Console.WriteLine($"Write test: {timePointsCount} Whole time taken: {watch.Elapsed.TotalSeconds} seconds");
    }
    public void SequentialReadTest(int readCount)
    {
         var watch = new Stopwatch();
        Random random = new Random();
        watch.Start();
        List<Task> list = new List<Task>();
        for (int i = 0; i < readCount; i++){      
            var statement = new SimpleStatement($"SELECT * FROM UEDATA WHERE timestamp = ? LIMIT 1", startDate.AddSeconds(random.Next(0,10_000)));
            list.Add(session.ExecuteAsync(statement));            
            
        }
        Task.WhenAll(list).Wait();
        watch.Stop();
        Console.WriteLine($"BulkReadTest: Whole {readCount} loops was done in {watch.Elapsed.TotalSeconds} seconds.");
    }
    
    public void ReadAllTest(){
        var watch = new Stopwatch();
        var statement = new SimpleStatement("SELECT * FROM UEDATA");
        watch.Start();
        var res = session.Execute(statement);
        foreach (var row in res){
            var data = row;
        }
        watch.Stop();
        Console.WriteLine($"Read all test, time: {watch.Elapsed.TotalSeconds}");
    }
    public void AggregationTest(int queryCount)
    {
        var watch = new Stopwatch();
        
        var endDate = startDate.AddDays(7);
        Console.WriteLine($"Starting aggregation test with {queryCount} querries ({startDate} - {endDate}...)");

        var ueData = new Table<UEData>(session);

        var ueDataLength = ueData.Count().Execute();
        DateTimeOffset start = new DateTimeOffset(startDate);
        DateTimeOffset end = new DateTimeOffset(endDate);
       
        for (int i = 0; i < queryCount; i++){ 
            //watch = new Stopwatch();          
            watch.Start();
            // IEnumerable<UEData> rows1 = (from record in ueData where (record.timestamp_column <= end && record.timestamp_column >= start) select record).Execute();
            // IEnumerable<BSData> rows2 = (from record in bsData where (record.timestamp_column <= end && record.timestamp_column >= start) select record).Execute();
            
            var query = "SELECT AVG(dl_brate) as dl_brate FROM UEDATA";

            // Execute the query
            var row = session.Execute(query);
            watch.Stop();
            var xd = row.First();
            var x2 = xd.GetValue<double>("system.avg(dl_brate)");
            
            Console.WriteLine($"Average: {x2}");
            
        }
        Console.WriteLine($" ueDataLength: {ueDataLength};");
        Console.WriteLine($"Test finished after {watch.Elapsed.TotalSeconds} seconds.");
        Console.WriteLine($"Average delay: {watch.Elapsed.TotalSeconds/queryCount} seconds per read.");
    }

    public void ResetDB()
    {
        session.Execute("DROP TABLE IF EXISTS RecordCassandra");
        session.Execute("DROP TYPE IF EXISTS RecordCassandra");

        session.Execute("DROP TABLE IF EXISTS UEDATA");
        session.Execute("DROP TYPE IF EXISTS UEDATA");
    }

    public void SetupDB()
    {
        // Create keyspace
        session.Execute($"CREATE KEYSPACE IF NOT EXISTS {KEY_SPACE}" + " WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1}");
        // Use keyspace
        session.Execute($"USE {KEY_SPACE}");
        var table1 = new Table<UEData>(session);
        table1.CreateIfNotExists();
    }

    public BatchStatement GenerateTestData(int count, DateTime timestamp, string UeDataTableName){
        BatchStatement batch = new BatchStatement();
        var random = new Random();
        for (var i = 0; i < count; i++){
            
            for (int bs_id = 0; bs_id < 3; bs_id++){
                
                for (int ue_id = 0; ue_id < 5; ue_id++){
                    UEData uEData = new UEData{
                                timestamp_column = timestamp.ToUniversalTime(), 
                                ue_id = ue_id + bs_id * 5,
                                pci = bs_id 
                    };
                    var simpleStatement =  new SimpleStatement($"INSERT INTO {UeDataTableName} (guid, timestamp_column, ue_id, cc, pci, earfcn, rsrp, pl, cfo, dl_mcs, dl_brate, dl_bler, ul_mcs, ul_brate, ul_bler, dl_snr, ul_buff) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", Guid.NewGuid(), uEData.timestamp_column,
                        uEData.ue_id, uEData.cc, uEData.pci, uEData.earfcn, uEData.rsrp, uEData.pl, uEData.cfo, uEData.dl_mcs, uEData.dl_brate, uEData.dl_bler, uEData.ul_mcs, uEData.ul_brate, uEData.ul_bler, uEData.dl_snr, uEData.ul_buff); 
                    //batch.Add(this.session.ExecuteAsync(simpleStatement));
                    batch.Add(simpleStatement);
                }
            }
            startDate = startDate.AddSeconds(i);
                  
        }
        return batch;
    }
}
