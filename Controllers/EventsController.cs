using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
  
  
}