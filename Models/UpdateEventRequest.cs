using System.ComponentModel.DataAnnotations;

namespace waves_events.Models;

public class UpdateEventRequest {
    [Required]
    public string EventId { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string EventName { get; set; } = string.Empty;
    
    public string EventDescription { get; set; } = string.Empty;
    
    public string EventBackgroundImage { get; set; } = string.Empty; // Upload a Base64 String
    
    public int EventTotalSeats { get; set; }
    
    public int EventRegisteredSeats { get; set; }
    
    public double EventTicketPrice { get; set; }
    
    public List<string> EventGenres { get; set; } = [];
    
    public List<string> EventCollab { get; set; } = [];
    
    public DateTime EventStartDate { get; set; } = DateTime.UtcNow;
    
    public DateTime EventEndDate { get; set; } = DateTime.UtcNow;
    
    public Location EventLocation { get; set; } = new ();
    
    public string EventStatus { get; set; } = EventStatusEnum.Scheduled.ToString();
    
    public int EventAgeRestriction { get; set; }
    
    public string EventCountry { get; set; } = string.Empty;
    
    public List<DiscountCodes> EventDiscounts { get; set; } = [];
}