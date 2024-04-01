using waves_events.Interfaces;

namespace waves_events.Handlers;

public class EventRegistrationCancelledHandler : IDomainEventHandler<EventRegistrationCancelled> {
  private readonly IMailService _mailService;

  public EventRegistrationCancelledHandler(IMailService emailService) {
    _mailService = emailService;
  }

  public async Task Handle(EventRegistrationCancelled @event) {
    await _mailService.SendEventCancellationEmail(@event.EventObj, @event.UserEmail);
  }
}