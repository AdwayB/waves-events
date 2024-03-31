using Microsoft.AspNetCore.Authorization;
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
  
  private void ValidateAndGetUserType () {
    var userDetails = this.GetUserDetailsFromContext();

    if (!userDetails.HasValue) 
      throw new UnauthorizedAccessException("User details not found. Check if user exists.");
    
    if (userDetails.Value.Item1 == Guid.Empty)
      throw new UnauthorizedAccessException("User Id cannot be empty.");
  }

  [Authorize]
  [HttpGet("get-registrations-for-event/{eventId}/{pageNumber:int}/{pageSize:int}")]
  public async Task<IActionResult> GetRegistrationsForEvent (string eventId, int pageNumber, int pageSize) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }

    try {
      if (!Guid.TryParse(eventId, out var guidId)) 
        return BadRequest("The provided ID is not in a valid format.");

      var (registrations, numberOfRegistrations) = await _paymentService.GetRegistrationsForEvent(guidId, pageNumber, pageSize);
      var totalPages = (int)Math.Ceiling(numberOfRegistrations / (double)pageSize);
      return Ok(new EventRegistrationsResponse(numberOfRegistrations, totalPages, pageNumber, pageSize, registrations));
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting registrations for event {eventId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpGet("get-registrations-by-user/{userId}/{pageNumber:int}/{pageSize:int}")]
  public async Task<IActionResult> GetRegistrationsByUser (string userId, int pageNumber, int pageSize) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }

    try {
      if (!Guid.TryParse(userId, out var guidId)) 
        return BadRequest("The provided ID is not in a valid format.");
      
      var totalPages = 0;
      
      var (registrations, numberOfRegistrations) = await _paymentService.GetRegistrationsByUser(guidId, pageNumber, pageSize);
      
      if (numberOfRegistrations != null)
        totalPages = (int)Math.Ceiling((int)numberOfRegistrations / (double)pageSize);
      
      return Ok(new UserEventRegistrationsResponse(false, numberOfRegistrations ?? 0, totalPages, pageNumber, pageSize, registrations));
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting registrations by user {userId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpGet("get-cancelled-registrations-by-user/{userId}/{pageNumber:int}/{pageSize:int}")]
  public async Task<IActionResult> GetCancelledRegistrationsByUser (string userId, int pageNumber, int pageSize) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }

    try {
      if (!Guid.TryParse(userId, out var guidId)) 
        return BadRequest("The provided ID is not in a valid format.");

      var (registrations, numberOfRegistrations) = await _paymentService.GetCancelledRegistrationsByUser(guidId, pageNumber, pageSize);
      var totalPages = (int)Math.Ceiling(numberOfRegistrations / (double)pageSize);
      return Ok(new UserEventRegistrationsResponse(true, numberOfRegistrations, totalPages, pageNumber, pageSize, registrations));
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting cancelled registrations by user {userId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpGet("get-registrations-by-user-and-eventId/{userId}/{eventId}")]
  public async Task<IActionResult> GetRegistrationsByUserAndEventID (string userId, string eventId) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }

    try {
      if (!Guid.TryParse(userId, out var userGuid)) 
        return BadRequest("The provided User ID is not in a valid format.");
      if (!Guid.TryParse(eventId, out var eventGuid)) 
        return BadRequest("The provided Event ID is not in a valid format.");

      var response = await _paymentService.GetRegistrationsByUserAndEventId(userGuid, eventGuid);
      return Ok(response);
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting registrations by user {userId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpPost("register-for-event")]
  public async Task<IActionResult> RegisterForEvent ([FromBody] PaymentRequest request) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }

    try {
      if (!Guid.TryParse(request.UserId, out var userGuid)) 
        return BadRequest("The provided User ID is not in a valid format.");
      if (!Guid.TryParse(request.EventId, out var eventGuid)) 
        return BadRequest("The provided Event ID is not in a valid format.");

      var response = await _paymentService.RegisterForEvent(userGuid, request.UserEmail, eventGuid);
      return response != null ? Ok(response) : StatusCode(500, "An error occurred while registering for the event.");
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while registering for event {request.EventId} by user {request.UserId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpDelete("cancel-registration/{userId}/{eventId}")]
  public async Task<IActionResult> CancelEventRegistration (string userId, string eventId) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }

    try {
      if (!Guid.TryParse(userId, out var userGuid)) 
        return BadRequest("The provided User ID is not in a valid format.");
      if (!Guid.TryParse(eventId, out var eventGuid)) 
        return BadRequest("The provided Event ID is not in a valid format.");

      var response = await _paymentService.CancelRegistration(userGuid, eventGuid);
      return response != null ? Ok(response) : StatusCode(500, "An error occurred while cancelling registration for the event.");
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while cancelling registration by user {userId} for event {eventId}: {ex.Message}");
    }
  }
}