using waves_events.Interfaces;
using waves_events.Models;

namespace waves_events.Handlers;
public class EventDeleted : IDomainEvent {
  public Events EventObj { get; }

  public EventDeleted(Events eventObj) {
    EventObj = eventObj;
  }
}