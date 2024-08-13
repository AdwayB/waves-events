using waves_events.Interfaces;
using waves_events.Models;

namespace waves_events.Handlers;

public class EventCreated : IDomainEvent {
  public Events EventObj { get; private set; }

  public EventCreated(Events eventObj) {
    EventObj = eventObj;
  }
}