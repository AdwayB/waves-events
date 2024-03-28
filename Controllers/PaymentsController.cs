using Microsoft.AspNetCore.Mvc;
using waves_events.Helpers;
using waves_events.Interfaces;
using waves_events.Models;

namespace waves_events.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : ControllerBase {
  private readonly IPaymentService _paymentService;
  
  public PaymentsController(IPaymentService paymentService) {
    _paymentService = paymentService;
  }
  
  private UserType ValidateAndGetUserType () {
    var userDetails = this.GetUserDetailsFromContext();

    if (!userDetails.HasValue) 
      throw new UnauthorizedAccessException("User details not found. Check if user exists.");

    var (userId, userType) = userDetails.Value;

    if (userId != Guid.Empty)
      return userType;
    
    throw new UnauthorizedAccessException("User Id cannot be empty.");
  }
}