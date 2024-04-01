using waves_events.Interfaces;

namespace waves_events.Handlers;

public class EventRegisteredHandler : IDomainEventHandler<EventRegistered> {
  private readonly IMailService _mailService;

  public EventRegisteredHandler(IMailService emailService) {
    _mailService = emailService;
  }

  public async Task Handle(EventRegistered @event) {
    await _mailService.SendEventRegistrationEmail(@event.EventObj, @event.UserEmail);
  }
}