using waves_events.Models;

namespace waves_events.Interfaces;

public interface IMailService {
  Task SendEventRegistrationEmail(Events eventObj, string userEmail);
  Task SendEventCancellationEmail(Events eventObj, string userEmail);
  Task SendEventUpdatedEmail (Events eventObj);
  Task SendEventDeletedEmail (Events eventObj);
}