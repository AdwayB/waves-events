using MongoDB.Driver;
using waves_events.Helpers;
using waves_events.Interfaces;
using waves_events.Models;

namespace waves_events.Services;

public class PaymentService : IPaymentService {
  private readonly MongoDatabaseContext _mongodb;
  private readonly EventService _eventService;

  public PaymentService(MongoDatabaseContext mongodb, EventService eventService) {
    _mongodb = mongodb;
    _eventService = eventService;
  }
  
  // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
  private async Task<Events> ValidateAndFindEventAsync(Guid userId, Guid eventId) {
    if (userId == Guid.Empty || eventId == Guid.Empty)
      throw new ApplicationException("Invalid user or event ID");

    var eventObj = await _mongodb.Events.Find(x => x.EventId == eventId).FirstOrDefaultAsync();
    if (eventObj == null)
      throw new ApplicationException("Event not found");

    return eventObj;
  }
  
  private async Task<(FilterDefinition<Payments>?, Payments?)> FindPaymentObjectAsync(Guid userId, Guid eventId) {
    var paymentObjectFilter = Builders<Payments>.Filter.And(
      Builders<Payments>.Filter.Eq(x => x.UserId, userId),
      Builders<Payments>.Filter.ElemMatch(x => x.PaymentDetails, pd => pd.EventId == eventId));

    return (paymentObjectFilter, await _mongodb.Payments.Find(paymentObjectFilter).FirstOrDefaultAsync());
  }

  public async Task<Payments?> RegisterForEvent(Guid userId, Guid eventId) {
    var eventObj = await ValidateAndFindEventAsync(userId, eventId);
    
    var availableSeats = eventObj.EventTotalSeats - eventObj.EventRegisteredSeats;
    if (availableSeats == 0)
      throw new ApplicationException("No available seats for this event");

    var (paymentObjectFilter, paymentObj) = await FindPaymentObjectAsync(userId, eventId);

    if (paymentObj != null) {
      if (paymentObj.PaymentDetails.Any(x => x.Status == PaymentStatus.Success.ToString()))
        throw new ApplicationException("User has already registered for this event");
      
      if (paymentObj.PaymentDetails.Any(x => x.Status == PaymentStatus.Pending.ToString()))
        throw new ApplicationException("User is in the process of registering for this event");
    }

    using (var session = await _mongodb.StartSessionAsync()) {
      session.StartTransaction();

      try {
        var eventResult = await _eventService.UpdateEvent(
          new Events {
            EventId = eventObj.EventId,
            EventRegisteredSeats = eventObj.EventRegisteredSeats + 1
          });
        if (eventResult == null)
          throw new ApplicationException("Failed to update event.");

        if (paymentObj == null) {
          await _mongodb.Payments.InsertOneAsync(session,
            new Payments { 
              UserId = userId, 
              PaymentDetails = [ new PaymentDetails { 
                  EventId = eventId,
                  PaymentId = Guid.NewGuid(),
                  InvoiceId = Guid.NewGuid(),
                  Amount = 0,
                  Status = PaymentStatus.Success.ToString()
                },
              ]
            });
          await session.CommitTransactionAsync();
          return await _mongodb.Payments.Find(x => x.UserId == userId).FirstOrDefaultAsync();
        }
        
        var update = Builders<Payments>.Update.Set("PaymentDetails.$.Status", PaymentStatus.Success.ToString());
        await _mongodb.Payments.UpdateOneAsync(session, paymentObjectFilter, update);

        await session.CommitTransactionAsync();
        return paymentObj;
      }
      catch (Exception ex) {
        await session.AbortTransactionAsync();
        throw new ApplicationException("Failed to register for event. " + ex.Message);
      }
    }
  }

  public async Task<Payments?> CancelRegistration(Guid userId, Guid eventId) {
    var eventObj = await ValidateAndFindEventAsync(userId, eventId);
    var (paymentObjectFilter, paymentObj) = await FindPaymentObjectAsync(userId, eventId);

    if (paymentObj == null)
      throw new ApplicationException("User has not registered for this event.");

    using (var session = await _mongodb.StartSessionAsync()) {
      session.StartTransaction();

      try {
        var eventResult = await _eventService.UpdateEvent(
          new Events {
            EventId = eventObj.EventId,
            EventRegisteredSeats = eventObj.EventRegisteredSeats - 1
          });
        if (eventResult == null)
          throw new ApplicationException("Failed to update event.");

        var update = Builders<Payments>.Update.Set("PaymentDetails.$.Status", PaymentStatus.Cancelled.ToString());
        await _mongodb.Payments.UpdateOneAsync(session, paymentObjectFilter, update);

        await session.CommitTransactionAsync();
        return paymentObj;
      }
      catch (Exception ex) {
        await session.AbortTransactionAsync();
        throw new ApplicationException("Failed to cancel registration. " + ex.Message);
      }
    }
  }
}
