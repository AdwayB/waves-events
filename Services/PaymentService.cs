using MongoDB.Driver;
using waves_events.Helpers;
using waves_events.Interfaces;
using waves_events.Models;
using ApplicationException = System.ApplicationException;

namespace waves_events.Services;

public class PaymentService : IPaymentService {
  private readonly MongoDatabaseContext _mongoDb;
  private readonly EventService _eventService;

  public PaymentService(MongoDatabaseContext mongoDb, EventService eventService) {
    _mongoDb = mongoDb;
    _eventService = eventService;
  }

  // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
  private async Task<Events> ValidateAndFindEventAsync(Guid? userId, Guid eventId) {
    if (userId == Guid.Empty || eventId == Guid.Empty)
      throw new ApplicationException("Invalid user or event ID");

    var eventObj = await _eventService.GetEventById(eventId);
    if (eventObj == null)
      throw new ApplicationException("Event not found");

    return eventObj;
  }

  private async Task<(FilterDefinition<Payments>?, Payments?)> FindPaymentObjectAsync(Guid userId, Guid eventId) {
    var paymentObjectFilter = Builders<Payments>.Filter.And(
      Builders<Payments>.Filter.Eq(x => x.UserId, userId),
      Builders<Payments>.Filter.ElemMatch(x => x.PaymentDetails, pd => pd.EventId == eventId)
    );

    return (paymentObjectFilter, await _mongoDb.Payments.Find(paymentObjectFilter).FirstOrDefaultAsync());
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

    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();

      try {
        var eventResult = await _eventService.UpdateEvent(
          new Events { EventId = eventObj.EventId, EventRegisteredSeats = eventObj.EventRegisteredSeats + 1 }
        );
        if (eventResult == null)
          throw new ApplicationException("Failed to update event.");

        if (paymentObj == null) {
          await _mongoDb.Payments.InsertOneAsync(
            session,
            new Payments
            {
              UserId = userId,
              PaymentDetails =
              [
                new PaymentDetails
                {
                  EventId = eventId,
                  PaymentId = Guid.NewGuid(),
                  InvoiceId = Guid.NewGuid(),
                  Amount = 0,
                  Status = PaymentStatus.Success.ToString()
                }
              ]
            }
          );
          await session.CommitTransactionAsync();
          return await _mongoDb.Payments.Find(x => x.UserId == userId).FirstOrDefaultAsync();
        }

        var update = Builders<Payments>.Update.Set("PaymentDetails.$.Status", PaymentStatus.Success.ToString());
        await _mongoDb.Payments.UpdateOneAsync(session, paymentObjectFilter, update);

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

    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();

      try {
        var eventResult = await _eventService.UpdateEvent(
          new Events { EventId = eventObj.EventId, EventRegisteredSeats = eventObj.EventRegisteredSeats - 1 }
        );

        if (eventResult == null)
          throw new ApplicationException("Failed to update event.");

        var update = Builders<Payments>.Update.Set("PaymentDetails.$.Status", PaymentStatus.Cancelled.ToString());
        await _mongoDb.Payments.UpdateOneAsync(session, paymentObjectFilter, update);

        await session.CommitTransactionAsync();
        return paymentObj;
      }
      catch (Exception ex) {
        await session.AbortTransactionAsync();
        throw new ApplicationException("Failed to cancel registration. " + ex.Message);
      }
    }
  }

  public async Task<(List<Guid>, int)> GetRegistrationsForEvent(Guid userId, Guid eventId, int pageNumber, int pageSize) {
    await ValidateAndFindEventAsync(userId, eventId);

    try {
      var numberOfRegisteredUsers = await _mongoDb.Payments.CountDocumentsAsync(
        x => x.PaymentDetails.Any(pd => pd.EventId == eventId)
      );

      var paymentObjects = await _mongoDb
        .Payments.Find(x => x.PaymentDetails.Any(pd => pd.EventId == eventId))
        .Skip((pageNumber - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();

      if (paymentObjects.Count == 0)
        throw new ApplicationException($"No registered userIds found for event: {eventId}.");

      var userIds = paymentObjects.Select(x => x.UserId).ToList();

      return (userIds, (int)numberOfRegisteredUsers);
    }
    catch (Exception ex) {
      throw new ApplicationException(
        $"An error occurred while getting registrations for event: {eventId}. {ex.Message}"
      );
    }
  }

  public async Task<(List<Events>, int)> GetRegistrationsByUser(Guid userId, int pageNumber, int pageSize) {
    if (userId == Guid.Empty)
      throw new ApplicationException("Invalid User Id");

    try {
      var numberOfRegisteredEvents = await _mongoDb.Payments.CountDocumentsAsync(x => x.UserId == userId);

      var paymentObjects = await _mongoDb
        .Payments.Find(x => x.UserId == userId)
        .Skip((pageNumber - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();

      if (paymentObjects.Count == 0)
        throw new ApplicationException($"No registered eventIds found for user: {userId}.");

      var eventIds = paymentObjects.SelectMany(x => x.PaymentDetails.Select(y => y.EventId)).ToList();
      var events = await _eventService.GetEventByIdList(eventIds);

      return (events, (int)numberOfRegisteredEvents);
    }
    catch (Exception ex) {
      throw new ApplicationException($"An error occurred while getting registrations for user: {userId}. {ex.Message}");
    }
  }

  public async Task<(List<Events>, int)> GetCancelledRegistrationsByUser(Guid userId, int pageNumber, int pageSize) {
    if (userId == Guid.Empty)
      throw new ApplicationException("Invalid User Id");

    try {
      var paymentObjects = await _mongoDb
        .Payments.Find(
          x => x.UserId == userId && x.PaymentDetails.Any(y => y.Status == PaymentStatus.Cancelled.ToString())
        )
        .Skip((pageNumber - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();

      if (paymentObjects.Count == 0)
        throw new ApplicationException($"No cancelled eventIds found for user: {userId}.");

      var eventIds = paymentObjects
        .SelectMany(x => x.PaymentDetails)
        .Where(y => y.Status == PaymentStatus.Cancelled.ToString())
        .Select(y => y.EventId)
        .Distinct()
        .ToList();
      var events = await _eventService.GetEventByIdList(eventIds);

      return (events, eventIds.Count);
    }
    catch (Exception ex) {
      throw new ApplicationException(
        $"An error occurred while getting cancelled registrations for user: {userId}. {ex.Message}"
      );
    }
  }
}
