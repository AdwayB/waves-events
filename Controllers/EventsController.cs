using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using waves_events.Helpers;
using waves_events.Interfaces;
using waves_events.Models;

namespace waves_events.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase {
  private readonly IEventService _eventService;
  
  public EventsController (IEventService eventService) {
    _eventService = eventService;
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

  [Authorize]
  [HttpGet("get-event-by-id/{id}")]
  public async Task<IActionResult> GetEventById (string id) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }

    try {
      if (!Guid.TryParse(id, out var guidId)) 
        return BadRequest("The provided ID is not in a valid format.");
      
      var response = await _eventService.GetEventById(guidId);
      return response == null ? BadRequest("Event does not exist.") : Ok(response);
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting event {id}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpGet("get-event-by-id-list")]
  public async Task<IActionResult> GetEventByIdList ([FromBody] List<string> ids) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }

    try {
      var guidIds = new List<Guid>();
      foreach (var id in ids) {
        if (!Guid.TryParse(id, out var guidId)) {
          return BadRequest($"The provided ID {id} is not in a valid format.");
        }
        guidIds.Add(guidId);
      }

      if (guidIds.Contains(Guid.Empty) || guidIds.Count == 0)
        return BadRequest("Invalid list IDs provided.");
      
      var response = await _eventService.GetEventByIdList(guidIds);
      return response == null ? BadRequest("One or more queried events do not exist.") : Ok(response);
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting events with ID list: {ex.Message}");
    }
  }

  [Authorize]
  [HttpGet("get-all-events/{pageNumber:int}/{pageSize:int}")]
  public async Task<IActionResult> GetAllEvents (int pageNumber, int pageSize) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    
    if (pageNumber < 1 || pageSize < 1) 
      return BadRequest("Page Number and Size must be greater than 0.");

    try {
      var (events, numberOfEvents) = await _eventService.GetAllEvents(pageNumber, pageSize);
      var totalPages = (int)Math.Ceiling(numberOfEvents / (double)pageSize);
      return Ok(new AllEventsResponse(numberOfEvents, totalPages, pageNumber, pageSize, events));
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting all events: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpGet("get-events-with-genre/{genre}/{pageNumber:int}/{pageSize:int}")]
  public async Task<IActionResult> GetEventsWithGenre (string genre, int pageNumber, int pageSize) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    
    if (pageNumber < 1 || pageSize < 1) 
      return BadRequest("Page Number and Size must be greater than 0.");

    try {
      var (events, numberOfEvents) = await _eventService.GetEventsWithGenre(genre, pageNumber, pageSize);
      var totalPages = (int)Math.Ceiling(numberOfEvents / (double)pageSize);
      return Ok(new AllEventsResponse(numberOfEvents, totalPages, pageNumber, pageSize, events));
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting events with genre {genre}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpGet("get-events-by-artist/{artistID}/{pageNumber:int}/{pageSize:int}")]
  public async Task<IActionResult> GetEventsByArtist (string artistID, int pageNumber, int pageSize) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    
    if (pageNumber < 1 || pageSize < 1) 
      return BadRequest("Page Number and Size must be greater than 0.");
    
    if (!Guid.TryParse(artistID, out var id)) 
      return BadRequest("The provided ID is not in a valid format.");

    try {
      var (events, numberOfEvents) = await _eventService.GetEventsByArtist(id, pageNumber, pageSize);
      var totalPages = (int)Math.Ceiling(numberOfEvents / (double)pageSize);
      return Ok(new AllEventsResponse(numberOfEvents, totalPages, pageNumber, pageSize, events));
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting events by artist {artistID}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpGet("get-events-with-artist-collab/{artistID}/{pageNumber:int}/{pageSize:int}")]
  public async Task<IActionResult> GetEventsWithArtistCollab (string artistID, int pageNumber, int pageSize) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    
    if (pageNumber < 1 || pageSize < 1) 
      return BadRequest("Page Number and Size must be greater than 0.");
    
    if (!Guid.TryParse(artistID, out var id)) 
      return BadRequest("The provided ID is not in a valid format.");

    try {
      var (events, numberOfEvents) = await _eventService.GetEventsByArtistCollab(id, pageNumber, pageSize);
      var totalPages = (int)Math.Ceiling(numberOfEvents / (double)pageSize);
      return Ok(new AllEventsResponse(numberOfEvents, totalPages, pageNumber, pageSize, events));
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting events with artist collab {artistID}: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpGet("get-events-by-location/{pageNumber:int}/{pageSize:int}")]
  public async Task<IActionResult> GetEventsByLocation ([FromBody] LocationRequest request, int pageNumber, int pageSize) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    
    if (pageNumber < 1 || pageSize < 1) 
      return BadRequest("Page Number and Size must be greater than 0.");
    
    if (request.Location.Length is 0 or > 2 || request.Radius > 500) 
      return BadRequest("The provided location request is not valid.");

    try {
      var (events, numberOfEvents) = await _eventService.GetEventsWithLocation(request.Location, request.Radius, pageNumber, pageSize);                        
      var totalPages = (int)Math.Ceiling(numberOfEvents / (double)pageSize);
      return Ok(new AllEventsResponse(numberOfEvents, totalPages, pageNumber, pageSize, events));
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting events by location: {ex.Message}");
    }
  }
  
  [Authorize]
  [HttpGet("get-events-by-date-range/{pageNumber:int}/{pageSize:int}")]
  public async Task<IActionResult> GetEventsByDateRange ([FromBody] DateRangeRequest request, int pageNumber, int pageSize) {
    try {
      ValidateAndGetUserType();
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    
    if (pageNumber < 1 || pageSize < 1) 
      return BadRequest("Page Number and Size must be greater than 0.");
    
    if (request.StartTime > request.EndTime) 
      return BadRequest("The provided date range is not valid.");

    try {
      var (events, numberOfEvents) = await _eventService.GetEventsWithDateRange(request.StartTime, request.EndTime, pageNumber, pageSize);                        
      var totalPages = (int)Math.Ceiling(numberOfEvents / (double)pageSize);
      return Ok(new AllEventsResponse(numberOfEvents, totalPages, pageNumber, pageSize, events));
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while getting events by location: {ex.Message}");
    }
  }
  
  [Authorize(Roles = "Admin")]
  [HttpPost("create-event")]
  public async Task<IActionResult> CreateEvent ([FromBody] Events request) {
    try {
      var userType = ValidateAndGetUserType();

      if (userType is not UserType.Admin)
        return Unauthorized("Only admins can create events.");

      var response = await _eventService.CreateEvent(request);
      return response == null ? BadRequest("Event already exists.") : Ok(response);
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while creating event: {ex.Message}");
    }
  }
  
  [Authorize(Roles = "Admin")]
  [HttpPatch("update-event")]
  public async Task<IActionResult> UpdateOrModifyEvent ([FromBody] UpdateEventRequest request) {
    try {
      var userType = ValidateAndGetUserType();

      if (userType is not UserType.Admin)
        return Unauthorized("Only admins can update events.");
      
      var response = await _eventService.UpdateEvent(request);
      return response == null ? BadRequest("Event not found.") : Ok(response);
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while updating event: {ex.Message}");
    }
  }
  
  [Authorize(Roles = "Admin")]
  [HttpPatch("update-event-collab")]
  public async Task<IActionResult> UpdateEventCollab ([FromBody] UpdateCollabRequest request) {
    try {
      var userType = ValidateAndGetUserType();

      if (userType is not UserType.Admin)
        return Unauthorized("Only admins can update events.");
      
      var response = await _eventService.UpdateEventCollab(request);
      return response == null ? BadRequest("Event not found.") : Ok(response);
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while updating event: {ex.Message}");
    }
  }
  
  [Authorize(Roles = "Admin")]
  [HttpPatch("update-event-discounts")]
  public async Task<IActionResult> UpdateEventDiscounts ([FromBody] UpdateDiscountsRequest request) {
    try {
      var userType = ValidateAndGetUserType();

      if (userType is not UserType.Admin)
        return Unauthorized("Only admins can update events.");
      
      var response = await _eventService.UpdateEventDiscounts(request);
      return response == null ? BadRequest("Event not found.") : Ok(response);
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while updating event: {ex.Message}");
    }
  }
  
  [Authorize(Roles = "Admin")]
  [HttpDelete("delete-event/{id}")]
  public async Task<IActionResult> DeleteEvent (string id) {
    try {
      var userType = ValidateAndGetUserType();

      if (userType is not UserType.Admin)
        return Unauthorized("Only admins can delete events.");
      
      if (!Guid.TryParse(id, out var eventId)) 
        return BadRequest("The provided ID is not in a valid format.");

      var response = await _eventService.DeleteEvent(eventId);
      return response == null ? BadRequest("Event not found.") : Ok(response);
    }
    catch (UnauthorizedAccessException) {
      return Unauthorized("User action unauthorized.");
    }
    catch (Exception ex) {
      return StatusCode(500, $"An error occurred while deleting event: {ex.Message}");
    }
  }
}