﻿using System.ComponentModel.DataAnnotations;

namespace waves_events.Models;

public class AddFeedbackRequest {
  [Required]
  public string EventId { get; set; } = string.Empty;

  [Required]
  public string UserId { get; set; } = string.Empty;

  [CustomValidation(typeof(AddFeedbackRequest), "ValidateRating")]
  public int Rating { get; set; }

  public string Comment { get; set; } = string.Empty;

  public static ValidationResult? ValidateRating(object value, ValidationContext validationContext) {
    var ratedValue = (int)value;
    return ratedValue is < 1 or > 5 ? new ValidationResult("Rating must be between 1 and 5") : ValidationResult.Success;
  }
}