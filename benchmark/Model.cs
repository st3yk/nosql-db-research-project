using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization;

namespace Models{
    public class Data{
        
        [BsonElement("cc")]
        public int cc {get; set;}
        
        [BsonElement("pci")]
        public int pci {get; set;}

        [BsonElement("earfcn")]
        public int earfcn {get; set;}

        [BsonElement("rsrp")]
        public double rsrp {get; set;}

        [BsonElement("pl")]
        public double pl {get; set;}

        [BsonElement("cfo")]
        public double cfo {get; set;}

        [BsonElement("dl_mcs")]
        public int dl_mcs {get; set;}

        [BsonElement("dl_brate")]
        public int dl_brate {get; set;}

        [BsonElement("dl_bler")]
        public double dl_bler {get; set;}

        [BsonElement("dl_snr")]
        public float dl_snr {get; set;}

        [BsonElement("ul_buff")]
        public int ul_buff {get; set;}

        [BsonElement("nof_ue")]
        public int nof_ue {get; set;}


    }

    [BsonIgnoreExtraElements]
    public class TimeSeries{
        [BsonElement("data")]
        public Data data {get; set;}
        public DateTime timestamp {get; set;}
    }
}