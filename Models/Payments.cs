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
    
    [BsonElement("userEmail")]
    [Required]
    [MinLength(4)]
    [MaxLength(30)]
    [EmailAddress]
    [RegularExpression(@"^[a-zA-Z0-9.+_%$#&-]+@gmail\.com$", ErrorMessage = "Email address must be a valid Gmail address with allowed symbols (_%$#&-).")]
    public string UserEmail { get; set; } = string.Empty;
    
    [BsonElement("paymentDetails")]
    [Required]
    public List<PaymentDetails> PaymentDetails { get; set; } = [];
}