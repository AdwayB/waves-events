using System.ComponentModel.DataAnnotations;

namespace waves_events.Models;

public class DateRangeRequest {
  [Required]
  public DateTime StartTime { get; set; }

  [Required]
  public DateTime EndTime { get; set; }
}