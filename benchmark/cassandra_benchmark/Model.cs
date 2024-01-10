
using Cassandra;
using Cassandra.Mapping;
using Cassandra.Mapping.Attributes;

public class Table{}
[Table("UEDATA")]

public class UEData : Table{
    
    [ClusteringKey(1)]
    public DateTimeOffset timestamp_column {get; set;}


    [PartitionKey (0)]


    public int ue_id {get; set;}


    public int cc {get; set;} = 0;
    

    public int pci {get; set;}


    public int earfcn {get; set;} = 3400;


    public int rsrp {get; set;} = new Random().Next(-80, -40);


    public int pl {get; set;} = new Random().Next(40, 80);


    public double cfo {get; set;} = new Random().NextDouble() * -1_000;


    public double dl_mcs {get; set;} = new Random().NextDouble() * 20;
    
    public double ul_mcs {get; set;} = new Random().NextDouble() * 20;


    public double dl_brate {get; set;} = new Random().NextDouble() * 50_000;
    

    public double ul_brate {get; set;} = new Random().NextDouble() * 50_000;

    public double dl_bler {get; set;} = new Random().NextDouble() * 100;
    public double ul_bler {get; set;} = new Random().NextDouble() * 100;

    public double dl_snr {get; set;} = new Random().NextDouble() * 5 + 10;

    public double ul_buff {get; set;} = new Random().NextDouble() * 100;
    

}

[Table("BSDATA")]


public class BSData : Table{

    [ClusteringKey(0)]

    public DateTimeOffset timestamp_column {get; set;}

    [PartitionKey(0)]
    public int bs_id {get; set;}
    public int nof_ue {get; set;} = 5;
    public double dlul_brate {get; set;} = new Random().NextDouble() * 1_500_000 + 500_000;
}
    
