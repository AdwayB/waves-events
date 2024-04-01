using waves_events.Interfaces;
using waves_events.Models;

namespace waves_events.Handlers;

public class EventRegistered : IDomainEvent {
  public Events EventObj { get; private set; }
  public string UserEmail { get; private set; }

  public EventRegistered(Events eventObj, string userEmail) {
    EventObj = eventObj;
    UserEmail = userEmail;
  }
}