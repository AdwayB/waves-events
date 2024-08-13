using waves_events.Interfaces;

namespace waves_events.Handlers;

public class EventCreatedHandler : IDomainEventHandler<EventCreated> {
  private readonly IMailService _mailService;

  public EventCreatedHandler(IMailService emailService) {
    _mailService = emailService;
  }

  public async Task Handle(EventCreated @event) {
    await _mailService.SendEventUpdatedEmail(@event.EventObj);
  }
}