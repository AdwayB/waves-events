using waves_events.Models;

namespace waves_events.Interfaces;

public interface IFeedbackService {
    Task<Feedback?> AddFeedback (Feedback feedbackObj);
    Task<Feedback> DeleteFeedback (Guid feedbackID);
    Task<Feedback> UpdateFeedback (Guid feedbackID);
    Task<(List<Feedback?>, int)> GetFeedbacksByEventId (Guid eventId, int pageNumber, int pageSize);
    Task<(List<Feedback?>, int)> GetAllFeedbacks (int pageNumber, int pageSize);
    Task<double> GetAverageRating (Guid eventId);
}