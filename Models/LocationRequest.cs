using System.ComponentModel.DataAnnotations;

namespace waves_events.Models;

public class LocationRequest {
  [Required]
  public double[] Location { get; set; } = [];
  
  [Required]
  public double Radius { get; set; }
}