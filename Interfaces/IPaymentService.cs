using waves_events.Models;

namespace waves_events.Interfaces;

public interface IPaymentService {
  Task<Payments?> RegisterForEvent(Guid userId, Guid eventId);
  Task<Payments?> CancelRegistration(Guid userId, Guid paymentId);
  Task<(List<Guid>, int)> GetRegistrationsForEvent(Guid userId, Guid eventId, int pageNumber, int pageSize);
  Task<(List<Events>?, int)> GetRegistrationsByUser(Guid userId, int pageNumber, int pageSize);
  Task<(List<Events>?, int)> GetCancelledRegistrationsByUser(Guid userId, int pageNumber, int pageSize);
  // Task<Payments?> ProcessPayment (Guid userId, Guid eventId, double amount, string paymentMethod);
  // Task<bool> RefundPayment (Guid paymentId);
  // Task<double> GetTotalCollectedForEvent (Guid eventId);
  // Task<bool> UpdatePaymentStatus (Guid paymentId, string newStatus);
  // Task<Payments?> GetPaymentById (Guid paymentId);
}
