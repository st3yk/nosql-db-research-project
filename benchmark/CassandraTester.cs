using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualBasic;

using System.Data;
using Cassandra;
using Cassandra.Mapping;
using Cassandra.Data.Linq;
using System.CodeDom;

public class CassandraTester{
  
    const string KEY_SPACE = "benchmark";
    ISession session;
    public CassandraTester(List<string> contactPoints, List<string> ports){
        Console.WriteLine(contactPoints.Count());
        var cluster = Cluster.Builder()
            .AddContactPoint(contactPoints[0]) // Replace with your Cassandra server's address
            .Build();
        //Create a session (similar to a database connection)
        session = cluster.Connect();
        // Create keyspace
        session.Execute($"CREATE KEYSPACE IF NOT EXISTS {KEY_SPACE}" + " WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1}");
        // Use keyspace
        session.Execute($"USE {KEY_SPACE}");

        // Drop table if exists
        DropTableIfExists();
        // Create table
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

    public async void BulkLoadTest(int wholeSize, int batchsize){
        Stopwatch stopwatch1 = new Stopwatch();
        Stopwatch stopwatch2 = new Stopwatch();

       
        int batchsize = 10;
        DateTime start = DateTime.ParseExact("20230501T00:00:00Z", "yyyyMMddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime();
        stopwatch1.Start();
        for(int i = 0; i < wholeSize/batchsize; i++){
            stopwatch2.Start();
            var batch = GenerateTestData(batchsize, start.AddSeconds(i), "UEDATA", "BSDATA");
            //await Task.WhenAll(batch);
            session.Execute(batch);
            stopwatch2.Stop();       
        }
        stopwatch1.Stop();
        Console.WriteLine($"Bulk write {count * batchsize} Whole time taken: {stopwatch1.Elapsed.TotalSeconds} seconds; Insert only time {stopwatch2.Elapsed.TotalSeconds}s");
    }

     public BatchStatement GenerateTestData(int count, DateTime start, string UeDataTableName, string BsDataTableName){
        BatchStatement batch = new BatchStatement();
        var random = new Random();
        for (var i = 0; i < count; i++){
            
            for (int bs_id = 0; bs_id < 3; bs_id++){
                BSData bSData = new BSData{
                        timestamp_column = start.AddSeconds(i).ToUniversalTime(), 
                        bs_id = bs_id
                };
                SimpleStatement simpleStatement =  new SimpleStatement($"INSERT INTO {BsDataTableName} (guid, timestamp_column, bs_id, nof_ue, dlul_brate) VALUES (?, ?,  ?,  ?, ?)", Guid.NewGuid(), bSData.timestamp_column ,bSData.bs_id, bSData.nof_ue, bSData.dlul_brate); 
                //batch.Add(this.session.ExecuteAsync(simpleStatement));
                batch.Add(simpleStatement);

                for (int ue_id = 0; ue_id < 5; ue_id++){
                    UEData uEData = new UEData{
                                timestamp_column = start.AddSeconds(i).ToUniversalTime(), 
                                ue_id = ue_id + bs_id * 3,
                                pci = bs_id 
                    };
                    simpleStatement =  new SimpleStatement($"INSERT INTO {UeDataTableName} (guid, timestamp_column, ue_id, cc, pci, earfcn, rsrp, pl, cfo, dlul_mcs, " +
                        " dlul_brate, dlul_bler, dl_snr, ul_buff) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", Guid.NewGuid(), uEData.timestamp_column,
                        uEData.ue_id, uEData.cc, uEData.pci, uEData.earfcn, uEData.rsrp, uEData.pl, uEData.cfo, uEData.dlul_mcs, uEData.dlul_brate, uEData.dlul_bler, uEData.dl_snr, uEData.ul_buff); 
                    //batch.Add(this.session.ExecuteAsync(simpleStatement));
                    batch.Add(simpleStatement);
                }
            }
                  
        }

        return batch;
    }
    public void AggregationTest(int n = 10, bool two_weeks = true){
        var watch = new Stopwatch();
        DateTime start_date = DateTime.ParseExact("20230501T00:00:00Z", "yyyyMMddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime();

        var end_date = two_weeks ? start_date.AddDays(14):start_date.AddDays(7);
        Console.WriteLine($"Starting aggregation test with {n} querries ({start_date} - {end_date}...)");

        var ueData = new Table<UEData>(session);
        var bsData = new Table<BSData>(session);

        var ueDataLength = ueData.Count().Execute();
        var bsDataLength = bsData.Count().Execute();
        DateTimeOffset start = new DateTimeOffset(start_date);
        DateTimeOffset end = new DateTimeOffset(end_date);
       
        for (int i = 0; i < n; i++){ 
            //watch = new Stopwatch();          
            watch.Start();
            // IEnumerable<UEData> rows1 = (from record in ueData where (record.timestamp_column <= end && record.timestamp_column >= start) select record).Execute();
            // IEnumerable<BSData> rows2 = (from record in bsData where (record.timestamp_column <= end && record.timestamp_column >= start) select record).Execute();
            
            var query = "SELECT AVG(dl_snr) as dl_snr FROM UEDATA";

            // Execute the query
            var row = session.Execute(query);
            watch.Stop();
            var xd = row.First();
            var x2 = xd.GetValue<double>("system.avg(dl_snr)");
            
            //Console.WriteLine($"Average: {average}");
            
        }
        Console.WriteLine($" ueDataLength: {ueDataLength};  bsDataLength: {bsDataLength}");
        Console.WriteLine($"Test finished after {watch.Elapsed.TotalSeconds} seconds.");
        Console.WriteLine($"Average delay: {watch.Elapsed.TotalSeconds/n} seconds per read.");
    }
    public void BulkReadTest(int n = 100){
        var watch = new Stopwatch();
        Random random = new Random();
        for (int i = 0; i < n; i++){      
            var statement = new SimpleStatement($"SELECT dlul_mcs  FROM UEDATA WHERE ue_id = ?", random.Next(0,15));
            watch.Start();
            var row = session.Execute(statement);

            
            watch.Stop();
        }
        Console.WriteLine($"Test finished after {watch.Elapsed.TotalSeconds} seconds.");
        Console.WriteLine($"Average delay: {watch.Elapsed.TotalSeconds/n} seconds per read.");
    }

     
}