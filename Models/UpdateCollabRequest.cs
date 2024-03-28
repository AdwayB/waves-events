using System.ComponentModel.DataAnnotations;

namespace waves_events.Models;

public class UpdateCollabRequest {
  [Required]
  public string EventId { get; set; } = string.Empty;
  
  [Required]
  public List<string> EventCollab { get; set; } = [];
}