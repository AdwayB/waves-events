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
          <style>
              body {
                  font-family: "Google Sans", Roboto, RobotoDraft, Helvetica, Arial, sans-serif;
                  margin: 0;
                  padding: 0;
                  background-color: #010320;
                  color: #c99fef;
              }
              pre {
                  font-family: unset;
                  font-size: 18px;
                  color: #e8e6e4;
              }
              h1 {
                  font-size: 36px;
                  color: #e8e6e4;
              }
              .container {
                  width: 100%;
                  margin: 0 auto; 
                  padding: 20px;
                  background-image: url("https://dim.mcusercontent.com/cs/4024b0f190264c150133796ae/images/58c2d062-ba76-e717-4479-30b219fb97b3.png?w=1200");
                  background-position: center center;
                  background-repeat: no-repeat;
                  background-size: cover;
              }
              .header img {
                  max-width: 100px;
              }
              .email-body {
                  width: 100%;
                  max-width: 600px; 
                  background-color: #010320a1;
                  border-radius: 32px;
                  padding: 20px;
              }
              .content {
                  width: 100%;
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
          </style>
      </head>
      <body>
          <table class="container" cellpadding="0" cellspacing="0" border="0">
              <tr>
                  <td align="center">
                      <table class="email-body" cellpadding="0" cellspacing="0" border="0">
                          <tr>
                              <td align="center">
                                  <header class="header">
                                      <img src="https://mcusercontent.com/4024b0f190264c150133796ae/images/d7726037-e05c-ab20-a65c-5bbaefe83bf5.png" alt="Logo" />
                                  </header>
                              </td>
                          </tr>
                          <tr>
                              <td class="content" align="center">
                                  <h1>{0}</h1>
                                  <pre>{1}</pre>
                              </td>
                          </tr>
                      </table>
                  </td>
              </tr>
          </table>
      </body>
      </html>
      """;
    
    return HTMLTemplate.Replace("{0}", heading).Replace("{1}", content);
  }

  public static string GetHTMLContent (Events eventObj, EmailType emailType) {
    var startDate = $"{eventObj.EventStartDate.Day}/{eventObj.EventStartDate.Month}/{eventObj.EventStartDate.Year}";
    var endDate = $"{eventObj.EventEndDate.Day}/{eventObj.EventEndDate.Month}/{eventObj.EventEndDate.Year}";

    return emailType switch {
      EmailType.Registered =>
        $"Congratulations! You have successfully registered for the event {eventObj.EventName} with ID {eventObj.EventId}, scheduled for {startDate}. Please check the event details for more information.",
      EmailType.RegistrationCancelled => $"Confirmed! Your registration for the event {eventObj.EventName} with ID {eventObj.EventId} has been cancelled.",
      EmailType.EventUpdated =>
        $"The event {eventObj.EventName} with ID {eventObj.EventId} has been updated. Following are the essentials, in case they have been updated:\n Event Start Date: {startDate}\n Event End Date: {endDate}\n Venue/Event Age Restriction: {eventObj.EventAgeRestriction}.\n Please check the event details for more information.",
      EmailType.EventDeleted => $"The event {eventObj.EventName} with ID {eventObj.EventId} has been deleted. The event was scheduled to happen between {startDate} and {endDate}",
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
