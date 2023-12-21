using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization;

namespace Models{
    public class Data{
        [BsonElement("field1")]
        public string field1 {get; set;}
        [BsonElement("field2")]
        public string field2 {get; set;}
        [BsonElement("field3")]
        public string field3 {get; set;}

    }

    [BsonIgnoreExtraElements]
    public class TimeSeries{
        [BsonElement("data")]
        public Data data {get; set;}
        public DateTime timestamp {get; set;}
    }
}