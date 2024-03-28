using waves_events.Models;

namespace waves_events.Interfaces;

public interface IFeedbackService {
  Task<UserFeedback?> GetFeedbackByEventAndUser(Guid eventId, Guid userId);
  Task<UserFeedback?> GetFeedbackById(Guid feedbackID);
  Task<UserFeedback?> AddFeedback(AddFeedbackRequest feedbackObj);
  Task<UserFeedback?> UpdateFeedback(UpdateFeedbackRequest feedbackObj);
  Task<Guid?> DeleteFeedback(UpdateFeedbackRequest feedbackObj);
  Task<(List<UserFeedback>?, int)> GetFeedbacksByEventId(Guid eventId, int pageNumber, int pageSize);
  Task<double?> GetAverageRating(Guid eventId);
}
