using System.ComponentModel.DataAnnotations;

namespace waves_events.Models;

public class DiscountCodes {
    public string discountCode { get; set; } = string.Empty;

    [CustomValidation(typeof(DiscountCodes), "ValidateDiscountPercentage")]
    public int discountPercentage { get; set; } = 0;
    
    public static ValidationResult? ValidateDiscountPercentage(object value, ValidationContext context) {
        var percentage = (int)value;
        return percentage is < 1 or > 99 ? new ValidationResult("Discount percentage must be between 1 and 99") : ValidationResult.Success;
    }
}