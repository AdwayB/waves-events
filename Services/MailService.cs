using Microsoft.Extensions.Options;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using waves_events.Helpers;
using waves_events.Interfaces;
using waves_events.Models;
using Task = System.Threading.Tasks.Task;

namespace waves_events.Services;

public class MailService : IMailService {
  private readonly EmailProviderConfig _emailConfig;
  private readonly IPaymentService _paymentService;

  public MailService (IOptions<EmailProviderConfig> emailConfig, IPaymentService paymentService) {
    _emailConfig = emailConfig.Value;
    _paymentService = paymentService;
  }

  private static List<SendSmtpEmailTo> GetReceivers (List<string> userEmails) {
    return userEmails.Select(userEmail => new SendSmtpEmailTo(userEmail)).ToList();
  }
  
  private async Task<string?> SendEmail (List<string> to, string subject, string htmlContent) {
    Configuration.Default.AddApiKey("api-key", _emailConfig.Key);
    var provider = new TransactionalEmailsApi();
    
    var emailSender = new SendSmtpEmailSender("Waves EMS", _emailConfig.SenderEmail);
    var emailReceivers = GetReceivers(to);
    var tryAttempts = 3;

    while (tryAttempts > 0) {
      try {
        var senderInstance = new SendSmtpEmail(emailSender, emailReceivers, null, null, htmlContent, null, subject);
        var result = await provider.SendTransacEmailAsync(senderInstance);
        return result.MessageId ?? throw new Exception();
      }
      catch (Exception) {
        tryAttempts--;
        if (tryAttempts == 0)
          return null;
        await Task.Delay(2000);
      }
    }
    return null;
  }

  public async Task SendEventRegistrationEmail (Events eventObj, string userEmail) {
    var heading = EmailTemplate.GetHTMLHeading(eventObj, EmailType.Registered);
    var content = EmailTemplate.GetHTMLContent(eventObj, EmailType.Registered);
    var htmlContent = EmailTemplate.GetHTMLTemplate(heading, content);

    await SendEmail([userEmail], heading, htmlContent);
  }
  
  public async Task SendEventCancellationEmail (Events eventObj, string userEmail) {
    var heading = EmailTemplate.GetHTMLHeading(eventObj, EmailType.RegistrationCancelled);
    var content = EmailTemplate.GetHTMLContent(eventObj, EmailType.RegistrationCancelled);
    var htmlContent = EmailTemplate.GetHTMLTemplate(heading, content);

    await SendEmail([userEmail], heading, htmlContent);
  }

  public async Task SendEventUpdatedEmail (Events eventObj) {
    var heading = EmailTemplate.GetHTMLHeading(eventObj, EmailType.EventUpdated);
    var content = EmailTemplate.GetHTMLContent(eventObj, EmailType.EventUpdated);
    var htmlContent = EmailTemplate.GetHTMLTemplate(heading, content);
    var userEmails = await _paymentService.GetRegisteredEmailsForEvent(eventObj.EventId);

    await SendEmail(userEmails, heading, htmlContent);
  }
  
  public async Task SendEventDeletedEmail (Events eventObj) {
    var heading = EmailTemplate.GetHTMLHeading(eventObj, EmailType.EventDeleted);
    var content = EmailTemplate.GetHTMLContent(eventObj, EmailType.EventDeleted);
    var htmlContent = EmailTemplate.GetHTMLTemplate(heading, content);
    var userEmails = await _paymentService.GetRegisteredEmailsForEvent(eventObj.EventId);

    await SendEmail(userEmails, heading, htmlContent);
  }
}