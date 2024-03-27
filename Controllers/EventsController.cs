using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using waves_events.Helpers;
using waves_events.Interfaces;
using waves_events.Models;
using HttpMethods = waves_events.Models.HttpMethods;

namespace waves_events.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase {
  private readonly IEventService _eventService;
  
  public EventsController (IEventService eventService) {
    _eventService = eventService;
  }
  
  [AuthorizeHttpMethods(AuthorizeUserEnum.Both, HttpMethods.GET, "")]
  public async Task<IActionResult> GetEventById
  
}