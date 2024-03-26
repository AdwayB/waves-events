namespace waves_events.Models;

public class EventRegistrationsResponse {
  public int NumberOfRegistrations { get; set; }
  public int TotalPages { get; set; }
  public int PageNumber { get; set; }
  public int PageSize { get; set; }
  public List<Guid> RegisteredUserIds { get; set; }

  public EventRegistrationsResponse (
    int numberOfRegistrations,
    int totalPages,
    int pageNumber,
    int pageSize,
    List<Guid> registeredUserIds
  ) {
    NumberOfRegistrations = numberOfRegistrations;
    TotalPages = totalPages;
    PageNumber = pageNumber;
    PageSize = pageSize;
    RegisteredUserIds = registeredUserIds;
  }
}
