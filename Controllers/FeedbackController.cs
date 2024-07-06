using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using waves_events.Helpers;
using waves_events.Interfaces;
using waves_events.Models;

namespace waves_events.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FeedbackController : ControllerBase {
  private readonly IFeedbackService _feedbackService;
  
  public FeedbackController (IFeedbackService feedbackService) {
    _feedbackService = feedbackService;
  }
  
  private void ValidateAndGetUserType () {
    var userDetails = this.GetUserDetailsFromContext();

    if (!userDetails.HasValue) 
      throw new UnauthorizedAccessException("User details not found. Check if user exists.");

    var userId = userDetails.Value;

    if (userId.Item1 == Guid.Empty)
      throw new UnauthorizedAccessException("User Id cannot be empty.");
  }

  [Authorize]
  [HttpGet("get-feedback-by-eventId/{eventId}/{pageNumber:int}/{pageSize:int}")]
  public async Task<IActionResult> GetFeedbackByEventId (string eventId, int pageNumber, int pageSize) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    
    try {
      if (!Guid.TryParse(eventId, out var guidId)) 
        return BadRequest("The provided ID is not in a valid format.");
      
      var (feedbacks, numberOfFeedback) = await _feedbackService.GetFeedbacksByEventId(guidId, pageNumber, pageSize);
      var totalPages = (int)Math.Ceiling(numberOfFeedback / (double)pageSize);
      return Ok(new EventFeedbackResponse(numberOfFeedback, totalPages, pageNumber, pageSize, feedbacks));
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting feedbacks for event {eventId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpGet("get-feedback-by-id/{feedbackId}")]
  public async Task<IActionResult> GetFeedbackById (string feedbackId) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    
    try {
      if (!Guid.TryParse(feedbackId, out var guidId)) 
        return BadRequest("The provided ID is not in a valid format.");
      
      var response = await _feedbackService.GetFeedbackById(guidId);
      return response != null ? Ok(response) : BadRequest("Feedback not found. Please check the ID provided.");
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting feedback with ID {feedbackId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpGet("get-feedback-by-event-and-user/{eventID}/{userId}")]
  public async Task<IActionResult> GetFeedbackByEventAndUser (string eventId, string userId) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    
    try {
      if (!Guid.TryParse(eventId, out var eventGuid) || eventGuid == Guid.Empty) 
        return BadRequest("The provided event ID is not in a valid format.");
      if (!Guid.TryParse(userId, out var userGuid) || userGuid == Guid.Empty) 
        return BadRequest("The provided user ID is not in a valid format.");
      
      var response = await _feedbackService.GetFeedbackByEventAndUser(eventGuid, userGuid);
      return Ok(response);
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting feedback by user {userId} for event {eventId}" +
                             $": {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpGet("get-average-rating/{eventID}")]
  public async Task<IActionResult> GetAverageRating (string eventId) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    
    try {
      if (!Guid.TryParse(eventId, out var eventGuid) || eventGuid == Guid.Empty) 
        return BadRequest("The provided event ID is not in a valid format.");
      
      var response = await _feedbackService.GetAverageRating(eventGuid);
      return response == null ? BadRequest("Unable to calculate average rating.") : Ok(response);
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting average rating for event {eventId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpPost("add-feedback")]
  public async Task<IActionResult> AddFeedback ([FromBody] AddFeedbackRequest request) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    
    try {
      if (!Guid.TryParse(request.EventId, out var eventGuid) || eventGuid == Guid.Empty) 
        return BadRequest("The provided event ID is not in a valid format.");
      if (!Guid.TryParse(request.UserId, out var userGuid) || userGuid == Guid.Empty) 
        return BadRequest("The provided user ID is not in a valid format.");
      
      var response = await _feedbackService.AddFeedback(request);
      return response != null ? Ok(response) : BadRequest("User has already submitted feedback for this event.");
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while adding feedback for {request.EventId} by user {request.UserId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpPatch("update-feedback")]
  public async Task<IActionResult> UpdateFeedback ([FromBody] UpdateFeedbackRequest request) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    
    try {
      if (!Guid.TryParse(request.EventId, out var eventGuid) || eventGuid == Guid.Empty) 
        return BadRequest("The provided event ID is not in a valid format.");
      if (!Guid.TryParse(request.UserId, out var userGuid) || userGuid == Guid.Empty) 
        return BadRequest("The provided user ID is not in a valid format.");
      if (!Guid.TryParse(request.FeedbackId, out var feedbackGuid) || feedbackGuid == Guid.Empty) 
        return BadRequest("The provided feedback ID is not in a valid format.");
      
      var response = await _feedbackService.UpdateFeedback(request);
      return response != null ? Ok(response) : NotFound("User has not submitted feedback for this event yet.");
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while updating feedback for {request.EventId} by user {request.UserId}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpDelete("delete-feedback")]
  public async Task<IActionResult> DeleteFeedback ([FromBody] UpdateFeedbackRequest request) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    
    try {
      if (!Guid.TryParse(request.EventId, out var eventGuid) || eventGuid == Guid.Empty) 
        return BadRequest("The provided event ID is not in a valid format.");
      if (!Guid.TryParse(request.UserId, out var userGuid) || userGuid == Guid.Empty) 
        return BadRequest("The provided user ID is not in a valid format.");
      if (!Guid.TryParse(request.FeedbackId, out var feedbackGuid) || feedbackGuid == Guid.Empty) 
        return BadRequest("The provided feedback ID is not in a valid format.");
      
      var response = await _feedbackService.DeleteFeedback(request);
      return response != null ? Ok(response) : NotFound("User has not submitted feedback for this event yet.");
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while deleting feedback for {request.EventId} by user {request.UserId}: {ex.Message}");
    }
  }
}