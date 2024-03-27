using System.ComponentModel.DataAnnotations;

namespace waves_events.Models;

public class EventFeedbacksResponse {
  [Required]
  public Guid EventId { get; set; } = Guid.Empty;
  
  [Required]
  public List<UserFeedback> UserFeedback { get; set; } = [];
}