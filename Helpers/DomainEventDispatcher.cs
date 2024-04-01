using waves_events.Interfaces;

namespace waves_events.Helpers;

public class DomainEventDispatcher : IDomainEventDispatcher {
  private readonly IServiceProvider _serviceProvider;

  public DomainEventDispatcher(IServiceProvider serviceProvider) {
    _serviceProvider = serviceProvider;
  }

  public async Task Dispatch<TEvent>(TEvent @event) where TEvent : IDomainEvent {
    var handlers = _serviceProvider.GetServices<IDomainEventHandler<TEvent>>();
    foreach (var handler in handlers) {
      await handler.Handle(@event);
    }
  }
}