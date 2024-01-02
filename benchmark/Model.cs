using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using Nest;

namespace Models{
        
        [ElasticsearchType(Name = "ue_data")]
    public class UEData{
        [Number(NumberType.Integer, Name = "ue_id")]
        [JsonPropertyName("ue_id")]
        [BsonElement("ue_id")]
        public int ue_id {get; set;}

        [Number(NumberType.Integer, Name = "cc")]
        [JsonPropertyName("cc")]
        [BsonElement("cc")]
        public int cc {get; set;} = 0;
        [Number(NumberType.Integer, Name = "pci")]
        [JsonPropertyName("pci")]
        [BsonElement("pci")]
        public int pci {get; set;}

        [Number(NumberType.Integer, Name = "earfcn")]
        [JsonPropertyName("earfcn")]
        [BsonElement("earfcn")]
        public int earfcn {get; set;} = 3400;

        [Number(NumberType.Integer, Name = "rsrp")]
        [JsonPropertyName("rsrp")]
        [BsonElement("rsrp")]
        public int rsrp {get; set;} = new Random().Next(-80, -40);

        [Number(NumberType.Integer, Name = "pl")]
        [JsonPropertyName("pl")]
        [BsonElement("pl")]
        public int pl {get; set;} = new Random().Next(40, 80);

        [Number(NumberType.Double, Name = "cfo")]
        [JsonPropertyName("cfo")]
        [BsonElement("cfo")]
        public double cfo {get; set;} = new Random().NextDouble() * -1_000;

        [Number(NumberType.Double, Name = "dlul_mcs")]
        [JsonPropertyName("dlul_mcs")]
        [BsonElement("dlul_mcs")]
        public double dlul_mcs {get; set;} = new Random().NextDouble() * 20;

        [Number(NumberType.Double, Name = "dlul_brate")]
        [JsonPropertyName("dlul_brate")]
        [BsonElement("dlul_brate")]
        public double dlul_brate {get; set;} = new Random().NextDouble() * 50_000;

        [Number(NumberType.Double, Name = "dlul_bler")]
        [JsonPropertyName("dlul_bler")]
        [BsonElement("dlul_bler")]
        public double dlul_bler {get; set;} = new Random().NextDouble() * 100;

        [Number(NumberType.Double, Name = "dl_snr")]
        [JsonPropertyName("dl_snr")]
        [BsonElement("dl_snr")]
        public double dl_snr {get; set;} = new Random().NextDouble() * 5 + 10;

        [Number(NumberType.Double, Name = "ul_buff")]
        [JsonPropertyName("ul_buff")]
        [BsonElement("ul_buff")]
        public double ul_buff {get; set;} = new Random().NextDouble() * 100;

    }

    public class BSData{

        [Number(NumberType.Integer, Name = "bs_id")]
        [JsonPropertyName("bs_id")]
        [BsonElement("bs_id")]
        public int bs_id {get; set;}

        [Number(NumberType.Integer, Name = "nof_ue")]
        [JsonPropertyName("nof_ue")]
        [BsonElement("nof_ue")]
        public int nof_ue {get; set;} = 5;

        [Number(NumberType.Double, Name = "dlul_brate")]
        [JsonPropertyName("dlul_brate")]
        [BsonElement("dlul_brate")]
        public double dlul_brate {get; set;} = new Random().NextDouble() * 1_500_000 + 500_000;
    }


    [ElasticsearchType(Name = "record")]
    [BsonIgnoreExtraElements]
    public class Record{

        [Object(Name = "ue_data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("ue_data")]
        [BsonIgnoreIfNull]
        [BsonElement("ue_data")]
        public UEData? ue_data {get; set;}
        
        [Object(Name = "bs_data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("bs_data")]
        [BsonIgnoreIfNull]
        [BsonElement("bs_data")]
        public BSData? bs_data {get; set;}

        [Date(Name = "timestamp", Index = true)]
        [JsonPropertyName("timestamp")]
        public DateTime timestamp {get; set;}
    }
}