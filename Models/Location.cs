using MongoDB.Bson.Serialization.Attributes;

namespace waves_events.Models;

public class Location {
    [BsonElement("type")]
    public string Type { get; set; } = "Point";

    [BsonElement("coordinates")] 
    public double[] Coordinates { get; set; } = [];
}