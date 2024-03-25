using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace waves_events.Models;

public class Payments {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("userId")] 
    [Required]
    public Guid UserId { get; set; } = Guid.Empty;
    
    [BsonElement("paymentDetails")]
    [Required]
    public List<PaymentDetails> PaymentDetails { get; set; } = [];
}