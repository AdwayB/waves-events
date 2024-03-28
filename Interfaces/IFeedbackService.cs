using waves_events.Models;

namespace waves_events.Interfaces;

public interface IFeedbackService {
  Task<Feedback?> GetFeedbackByEventAndUser(Guid eventId, Guid userId);
  Task<Feedback?> GetFeedbackById(Guid feedbackID);
  Task<Feedback?> AddFeedback(UpdateFeedbackRequest feedbackObj);
  Task<Feedback?> UpdateFeedback(UpdateFeedbackRequest feedbackObj);
  Task<Guid?> DeleteFeedback(UpdateFeedbackRequest feedbackObj);
  Task<(List<UserFeedback>?, int)> GetFeedbacksByEventId(Guid eventId, int pageNumber, int pageSize);
  Task<double?> GetAverageRating(Guid eventId);
}
