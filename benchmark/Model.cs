using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Models{
    public class UEData{
        
        [JsonPropertyName("ue_id")]
        [BsonElement("ue_id")]
        public int ue_id {get; set;}
        [JsonPropertyName("cc")]
        [BsonElement("cc")]
        public int cc {get; set;} = 0;
        [JsonPropertyName("pci")]
        [BsonElement("pci")]
        public int pci {get; set;}

        [JsonPropertyName("earfcn")]
        [BsonElement("earfcn")]
        public int earfcn {get; set;} = 3400;

        [JsonPropertyName("rsrp")]
        [BsonElement("rsrp")]
        public int rsrp {get; set;} = new Random().Next(-80, -40);

        [JsonPropertyName("pl")]
        [BsonElement("pl")]
        public int pl {get; set;} = new Random().Next(40, 80);

        [JsonPropertyName("cfo")]
        [BsonElement("cfo")]
        public double cfo {get; set;} = new Random().NextDouble() * -1_000;

        [JsonPropertyName("dlul_mcs")]
        [BsonElement("dlul_mcs")]
        public double dlul_mcs {get; set;} = new Random().NextDouble() * 20;

        [JsonPropertyName("dlul_brate")]
        [BsonElement("dlul_brate")]
        public double dlul_brate {get; set;} = new Random().NextDouble() * 50_000;

        [JsonPropertyName("clul_bler")]
        [BsonElement("dlul_bler")]
        public double dlul_bler {get; set;} = new Random().NextDouble() * 100;

        [JsonPropertyName("dl_snr")]
        [BsonElement("dl_snr")]
        public double dl_snr {get; set;} = new Random().NextDouble() * 5 + 10;

        [JsonPropertyName("ul_buff")]
        [BsonElement("ul_buff")]
        public double ul_buff {get; set;} = new Random().NextDouble() * 100;

    }

    public class BSData{

        [JsonPropertyName("bs_id")]
        [BsonElement("bs_id")]
        public int bs_id {get; set;}

        [JsonPropertyName("nof_ue")]
        [BsonElement("nof_ue")]
        public int nof_ue {get; set;} = 5;

        [JsonPropertyName("dlul_brate")]
        [BsonElement("dlul_brate")]
        public double dlul_brate {get; set;} = new Random().NextDouble() * 1_500_000 + 500_000;
    }


    [BsonIgnoreExtraElements]
    public class Record{

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("ue_data")]
        [BsonIgnoreIfNull]
        [BsonElement("ue_data")]
        public UEData? ue_data {get; set;}
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("bs_data")]
        [BsonIgnoreIfNull]
        [BsonElement("bs_data")]
        public BSData? bs_data {get; set;}
        [JsonPropertyName("timestamp")]
        public DateTime timestamp {get; set;}
    }
}