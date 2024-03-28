namespace waves_events.Models;

public class AllEventsResponse {
  public int NumberOfEvents { get; set; }
  public int TotalPages { get; set; }
  public int PageNumber { get; set; }
  public int PageSize { get; set; }
  public List<Events> Events { get; set; }

  public AllEventsResponse (
    int numberOfEvents,
    int totalPages,
    int pageNumber,
    int pageSize,
    List<Events> events
  ) {
    NumberOfEvents = numberOfEvents;
    TotalPages = totalPages;
    PageNumber = pageNumber;
    PageSize = pageSize;
    Events = events;
  }
}