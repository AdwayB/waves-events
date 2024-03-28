using System.ComponentModel.DataAnnotations;

namespace waves_events.Models;

public class UpdateDiscountsRequest {
  [Required]
  public string EventId { get; set; } = string.Empty;
  
  [Required]
  public List<DiscountCodes> EventDiscounts { get; set; } = [];
}