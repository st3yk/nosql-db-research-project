using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization;

namespace Models{
    public class UEData{
        
        [BsonElement("ue_id")]
        public int ue_id {get; set;}
        [BsonElement("cc")]
        public int cc {get; set;} = 0;
        
        [BsonElement("pci")]
        public int pci {get; set;}

        [BsonElement("earfcn")]
        public int earfcn {get; set;} = 3400;

        [BsonElement("rsrp")]
        public int rsrp {get; set;} = new Random().Next(-80, -40);

        [BsonElement("pl")]
        public int pl {get; set;} = new Random().Next(40, 80);

        [BsonElement("cfo")]
        public double cfo {get; set;} = new Random().NextDouble() * -1_000;

        [BsonElement("dlul_mcs")]
        public double dlul_mcs {get; set;} = new Random().NextDouble() * 20;

        [BsonElement("dlul_brate")]
        public double dlul_brate {get; set;} = new Random().NextDouble() * 50_000;

        [BsonElement("dlul_bler")]
        public double dlul_bler {get; set;} = new Random().NextDouble() * 100;

        [BsonElement("dl_snr")]
        public double dl_snr {get; set;} = new Random().NextDouble() * 5 + 10;

        [BsonElement("ul_buff")]
        public double ul_buff {get; set;} = new Random().NextDouble() * 100;
       

    }

    public class BSData{
        [BsonElement("bs_id")]
        public int bs_id {get; set;}

        [BsonElement("nof_ue")]
        public int nof_ue {get; set;} = 5;
        [BsonElement("dlul_brate")]
        public double dlul_brate {get; set;} = new Random().NextDouble() * 1_500_000 + 500_000;
    }

    [BsonIgnoreExtraElements]
    public class Record{

        [BsonIgnoreIfNull]
        [BsonElement("ue_data")]
        public UEData? UEData {get; set;}
        [BsonIgnoreIfNull]
        [BsonElement("bs_data")]
        public BSData? BSData {get; set;}
        public DateTime timestamp {get; set;}
    }
}