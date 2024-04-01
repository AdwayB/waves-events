using waves_events.Interfaces;

namespace waves_events.Handlers;

public class EventDeletedHandler : IDomainEventHandler<EventDeleted>{
    private readonly IMailService _mailService;

    public EventDeletedHandler(IMailService emailService) {
      _mailService = emailService;
    }

    public async Task Handle(EventDeleted @event) {
      await _mailService.SendEventDeletedEmail(@event.EventObj);
    }
}