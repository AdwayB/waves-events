using waves_events.Models;

namespace waves_events.Helpers;

public enum EmailType {
  Registered, 
  RegistrationCancelled,
  EventUpdated,
  EventDeleted
}

public class EmailTemplate {
  public static string GetHTMLTemplate(string heading, string content) {
    var HTMLTemplate =
      """
      <!DOCTYPE html>
      <html lang="en">
        <head>
          <meta charset="UTF-8" />
          <meta http-equiv="X-UA-Compatible" content="IE=edge" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0" />
          <title>Email Template</title>
          <style>
            body {
              font-family: "Work Sans", sans-serif;
              margin: 0;
              padding: 0;
              background-color: #010320;
              color: #c99fef;
            }
            pre {
              font-family: unset;
            }
            h1,
            h2,
            h3,
            h4,
            h5,
            h6 {
              color: #e8e6e4;
            }
            .container {
              display: flex;
              flex-direction: column;
              align-items: center;
              justify-content: center;
              padding: 20px;
              background-image: url("https://dim.mcusercontent.com/cs/4024b0f190264c150133796ae/images/58c2d062-ba76-e717-4479-30b219fb97b3.png?w=1200");
              background-position: center center;
              background-repeat: no-repeat;
              background-size: cover;
            }
            .header,
            .content,
            .footer {
              display: flex;
              flex-direction: column;
              align-items: center;
              justify-content: center;
            }
            .email-body {
              padding: 20px;
              background-color: #010320a1;
              border-radius: 32px;
            }
            .header img {
              max-width: 100px;
            }
            .content {
              padding: 20px;
            }
            .button {
              background-color: #9d5ad7;
              color: #22055a;
              padding: 10px 20px;
              text-decoration: none;
              border-radius: 50px;
              margin-top: 20px;
            }
            @media (max-width: 480px) {
              .content h1,
              .content p,
              .content pre {
                text-align: center;
              }
            }
          </style>
        </head>
        <body>
          <div class="container">
            <div class="email-body">
              <header class="header">
                <img
                  src="https://mcusercontent.com/4024b0f190264c150133796ae/images/d7726037-e05c-ab20-a65c-5bbaefe83bf5.png"
                  alt="Logo"
                />
              </header>
              <div class="content">
                <h1>{{heading}}</h1>
                <pre>{{content}}</pre>
              </div>
            </div>
          </div>
        </body>
      </html>
      """;
    
    HTMLTemplate = HTMLTemplate.Replace("{{heading}}", heading).Replace("{{content}}", content);
    return HTMLTemplate;
  }

  public static string GetHTMLContent (Events eventObj, EmailType emailType) {
    var startDate = $"{eventObj.EventStartDate.Day}/{eventObj.EventStartDate.Month}/{eventObj.EventStartDate.Year}";
    var endDate = $"{eventObj.EventEndDate.Day}/{eventObj.EventEndDate.Month}/{eventObj.EventEndDate.Year}";

    return emailType switch {
      EmailType.Registered =>
        $"Congratulations! You have successfully registered for the event {eventObj.EventName}, scheduled for {startDate}. Please check the event details for more information.",
      EmailType.RegistrationCancelled => $"Confirmed! Your registration for the event {eventObj.EventName} has been cancelled.",
      EmailType.EventUpdated =>
        $"The event {eventObj.EventName} has been updated. Following are the essentials, in case they have been updated:\n Event Start Date: {startDate}\n Event End Date: {endDate}\n Venue/Event Age Restriction: {eventObj.EventAgeRestriction}.\n Please check the event details for more information.",
      EmailType.EventDeleted => $"The event {eventObj.EventName} has been deleted. The event was scheduled to happen between {startDate} and {endDate}",
      _ => "Error in template selection. Please contact the administrator."
    };
  }
  
  public static string GetHTMLHeading (Events eventObj, EmailType emailType) {
    return emailType switch {
      EmailType.Registered => $"Waves: Successfully Registered for {eventObj.EventName}!",
      EmailType.RegistrationCancelled => $"Waves: Registration Cancelled for {eventObj.EventName}!",
      EmailType.EventUpdated => $"Waves: Event Details Updated for {eventObj.EventName}!",
      EmailType.EventDeleted => $"Waves: Event {eventObj.EventName} Deleted",
      _ => ""
    };
  }
}
