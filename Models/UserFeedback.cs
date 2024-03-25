using System.ComponentModel.DataAnnotations;

namespace waves_events.Models;

public class UserFeedback {
    [Required]
    public string UserId { get; set; } = string.Empty;

    [CustomValidation(typeof(UserFeedback), "ValidateRating")]
    public int Rating { get; set; } = 0;
    
    public string Comment { get; set; } = string.Empty;

    public static ValidationResult? ValidateRating(object value, ValidationContext validationContext) {
        var ratedValue = (int)value;
        return ratedValue is < 1 or > 5 
            ? new ValidationResult("Rating must be between 1 and 5")
            : ValidationResult.Success;
    }
}