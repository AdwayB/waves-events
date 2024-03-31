using System.ComponentModel.DataAnnotations;

namespace waves_events.Models;

public class PaymentRequest {
  [Required]
  public string UserId { get; set; } = string.Empty;
  
  [Required]
  public string EventId { get; set; } = string.Empty;
    
  [Required]
  [MinLength(4)]
  [MaxLength(30)]
  [EmailAddress]
  [RegularExpression(@"^[a-zA-Z0-9.+_%$#&-]+@gmail\.com$", ErrorMessage = "Email address must be a valid Gmail address with allowed symbols (_%$#&-).")]
  public string UserEmail { get; set; } = string.Empty;
}