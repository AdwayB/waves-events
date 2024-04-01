using waves_events.Interfaces;

namespace waves_events.Handlers;

public class EventUpdatedHandler : IDomainEventHandler<EventUpdated> {
  private readonly IMailService _mailService;

  public EventUpdatedHandler(IMailService emailService) {
    _mailService = emailService;
  }

  public async Task Handle(EventUpdated @event) {
    await _mailService.SendEventUpdatedEmail(@event.EventObj);
  }
}