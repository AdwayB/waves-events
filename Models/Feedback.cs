using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace waves_events.Models;

public class Feedback {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("eventId")]
    [Required]
    public Guid EventId { get; set; } = Guid.Empty;
    
    [BsonElement("userFeedback")]
    [Required]
    public List<UserFeedback> UserFeedback { get; set; } = [];
}