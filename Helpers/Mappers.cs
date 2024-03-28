using waves_events.Models;

namespace waves_events.Helpers;

public class Mappers {
  public static Events UpdateRequestToEventBody(UpdateEventRequest updateRequest, Guid eventId) {
    return new Events {
      EventId = eventId,
      EventName = updateRequest.EventName,
      EventDescription = updateRequest.EventDescription,
      EventBackgroundImage = updateRequest.EventBackgroundImage,
      EventTotalSeats = updateRequest.EventTotalSeats,
      EventRegisteredSeats = updateRequest.EventRegisteredSeats,
      EventTicketPrice = updateRequest.EventTicketPrice,
      EventGenres = updateRequest.EventGenres,
      EventCollab = updateRequest.EventCollab.Select(Guid.Parse).ToList(),
      EventStartDate = updateRequest.EventStartDate,
      EventEndDate = updateRequest.EventEndDate,
      EventLocation = updateRequest.EventLocation,
      EventStatus = updateRequest.EventStatus,
      EventAgeRestriction = updateRequest.EventAgeRestriction,
      EventCountry = updateRequest.EventCountry,
      EventDiscounts = updateRequest.EventDiscounts
    };
  }
}
