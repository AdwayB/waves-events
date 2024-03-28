using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace waves_events.Models;

public enum EventStatusEnum {
    Scheduled,
    Cancelled,
    Completed
}

public class Events {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("eventId")] 
    [Required]
    public Guid EventId { get; set; } = Guid.Empty;
    
    [BsonElement("eventName")] 
    [Required]
    [MinLength(3)]
    [MaxLength(50)]
    public string EventName { get; set; } = string.Empty;
    
    [BsonElement("eventDescription")] 
    [Required]
    public string EventDescription { get; set; } = string.Empty;
    
    [BsonElement("eventBackgroundImage")] 
    [Required]
    public string EventBackgroundImage { get; set; } = string.Empty; // Upload a Base64 String
    
    [BsonElement("eventTotalSeats")] 
    [Required]
    public int EventTotalSeats { get; set; }
    
    [BsonElement("eventRegisteredSeats")] 
    [Required]
    [CustomValidation(typeof(Events), "ValidateEventRegisteredSeats")]
    public int EventRegisteredSeats { get; set; }
    
    [BsonElement("eventTicketPrice")] 
    [Required]
    public double EventTicketPrice { get; set; }
    
    [BsonElement("eventGenres")] 
    [Required]
    public List<string> EventGenres { get; set; } = [];
    
    [BsonElement("eventCollab")] 
    [Required]
    public List<Guid> EventCollab { get; set; } = [];
    
    [BsonElement("eventStartDate")] 
    [Required]
    public DateTime EventStartDate { get; set; } = DateTime.UtcNow;
    
    [BsonElement("eventEndDate")] 
    [Required]
    public DateTime EventEndDate { get; set; } = DateTime.UtcNow;
    
    [BsonElement("eventLocation")] 
    [Required]
    public Location EventLocation { get; set; } = new ();
    
    [BsonElement("eventStatus")] 
    [Required]
    public string EventStatus { get; set; } = EventStatusEnum.Scheduled.ToString();
    
    [BsonElement("eventCreatedBy")] 
    [Required]
    public Guid EventCreatedBy { get; set; } = Guid.Empty;
    
    [BsonElement("eventAgeRestriction")] 
    [Required]
    public int EventAgeRestriction { get; set; }
    
    [BsonElement("eventCountry")] 
    [Required]
    public string EventCountry { get; set; } = string.Empty;
    
    [BsonElement("eventDiscounts")] 
    [Required]
    public List<DiscountCodes> EventDiscounts { get; set; } = [];
    
    public static ValidationResult? ValidateEventRegisteredSeats(object value, ValidationContext context) {
        if (context.ObjectInstance is not Events eventModel) {
            throw new InvalidOperationException("Validation context is not of type Events.");
        }

        var totalSeats = eventModel.EventTotalSeats;
        var registeredSeats = (int)value;
        
        return registeredSeats < 0 || registeredSeats > totalSeats
            ? new ValidationResult("Event registered seats must be between 0 and the total number of seats.")
            : ValidationResult.Success;
    }
}