namespace waves_events.Models;

public class EventFeedbackResponse {
  public int NumberOfFeedback { get; set; }
  public int TotalPages { get; set; }
  public int PageNumber { get; set; }
  public int PageSize { get; set; }
  public List<UserFeedback>? Feedbacks { get; set; }

  public EventFeedbackResponse (
    int numberOfFeedback,
    int totalPages,
    int pageNumber,
    int pageSize,
    List<UserFeedback>? feedbacks
  ) {
    NumberOfFeedback = numberOfFeedback;
    TotalPages = totalPages;
    PageNumber = pageNumber;
    PageSize = pageSize;
    Feedbacks = feedbacks;
  }
}