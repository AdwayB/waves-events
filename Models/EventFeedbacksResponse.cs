using System.ComponentModel.DataAnnotations;

namespace waves_events.Models;

public class EventFeedbacksResponse {
  [Required]
  public Guid EventId { get; set; }
  
  [Required]
  public List<UserFeedback> UserFeedback { get; set; }
  
  public EventFeedbacksResponse (Guid eventId, List<UserFeedback> userFeedback) {
    EventId = eventId;
    UserFeedback = userFeedback;
  }
}