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

        [Number(NumberType.Double, Name = "dl_mcs")]
        [JsonPropertyName("dl_mcs")]
        [BsonElement("dl_mcs")]
        public double dl_mcs {get; set;} = new Random().NextDouble() * 20;

        [Number(NumberType.Double, Name = "dl_brate")]
        [JsonPropertyName("dl_brate")]
        [BsonElement("dl_brate")]
        public double dl_brate {get; set;} = new Random().NextDouble() * 50_000;

        [Number(NumberType.Double, Name = "dl_bler")]
        [JsonPropertyName("dl_bler")]
        [BsonElement("dl_bler")]
        public double dl_bler {get; set;} = new Random().NextDouble() * 100;

        [Number(NumberType.Double, Name = "ul_mcs")]
        [JsonPropertyName("ul_mcs")]
        [BsonElement("ul_mcs")]
        public double ul_mcs {get; set;} = new Random().NextDouble() * 20;

        [Number(NumberType.Double, Name = "ul_brate")]
        [JsonPropertyName("ul_brate")]
        [BsonElement("ul_brate")]
        public double ul_brate {get; set;} = new Random().NextDouble() * 50_000;

        [Number(NumberType.Double, Name = "ul_bler")]
        [JsonPropertyName("ul_bler")]
        [BsonElement("ul_bler")]
        public double ul_bler {get; set;} = new Random().NextDouble() * 100;

        [Number(NumberType.Double, Name = "dl_snr")]
        [JsonPropertyName("dl_snr")]
        [BsonElement("dl_snr")]
        public double dl_snr {get; set;} = new Random().NextDouble() * 5 + 10;

        [Number(NumberType.Double, Name = "ul_buff")]
        [JsonPropertyName("ul_buff")]
        [BsonElement("ul_buff")]
        public double ul_buff {get; set;} = new Random().NextDouble() * 100;

    }

    [ElasticsearchType(Name = "record")]
    [BsonIgnoreExtraElements]
    public class Record{

        [Object(Name = "ue_data")]
        [JsonPropertyName("ue_data")]
        [BsonElement("ue_data")]
        public UEData ue_data {get; set;}
       
        [Date(Name = "timestamp", Index = true)]
        [JsonPropertyName("timestamp")]
        public DateTime timestamp {get; set;}
    }
}
