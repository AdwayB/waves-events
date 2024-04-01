using waves_events.Interfaces;
using waves_events.Models;

namespace waves_events.Handlers;
public class EventUpdated : IDomainEvent {
  public Events EventObj { get; private set; }

  public EventUpdated(Events eventObj) {
    EventObj = eventObj;
  }
}