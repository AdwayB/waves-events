namespace waves_events.Interfaces;

public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent {
  Task Handle(TEvent @event);
}