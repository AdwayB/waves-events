using waves_events.Interfaces;

namespace waves_events.Interfaces;
public interface IDomainEventDispatcher {
  Task Dispatch<TEvent>(TEvent eventInstance) where TEvent : IDomainEvent;
}