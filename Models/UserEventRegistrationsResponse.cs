namespace waves_events.Models;

public class UserEventRegistrationsResponse {
  public bool Cancelled { get; set; }
  public int NumberOfRegistrations { get; set; }
  public int TotalPages { get; set; }
  public int PageNumber { get; set; }
  public int PageSize { get; set; }
  public List<Events>? RegisteredEventIds { get; set; }

  public UserEventRegistrationsResponse (
    bool cancelled,
    int numberOfRegistrations,
    int totalPages,
    int pageNumber,
    int pageSize,
    List<Events>? registeredEventIds
  ) {
    Cancelled = cancelled;
    NumberOfRegistrations = numberOfRegistrations;
    TotalPages = totalPages;
    PageNumber = pageNumber;
    PageSize = pageSize;
    RegisteredEventIds = registeredEventIds;
  }
}
