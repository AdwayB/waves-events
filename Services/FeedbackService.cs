using MongoDB.Driver;
using waves_events.Interfaces;
using waves_events.Models;

namespace waves_events.Services;

public class FeedbackService : IFeedbackService {
  private readonly IMongoDatabaseContext _mongoDb;
  private readonly IEventService _eventService;
  
  public FeedbackService(IMongoDatabaseContext mongoDb, IEventService eventService) {
    _mongoDb = mongoDb;
    _eventService = eventService;
  }
  
  private static UpdateDefinition<Feedback> BuildUpdateDefinition(Feedback feedbackObj) {
    var updateDefinitionBuilder = new List<UpdateDefinition<Feedback>>();
    
    if (feedbackObj.UserFeedback is not { Count: > 0 })
      return Builders<Feedback>.Update.Combine(updateDefinitionBuilder);
    
    var update = Builders<Feedback>.Update.Set(f => f.UserFeedback, feedbackObj.UserFeedback);
    updateDefinitionBuilder.Add(update);

    return Builders<Feedback>.Update.Combine(updateDefinitionBuilder);
  }

  public async Task<Feedback?> GetFeedbackByEventAndUser (Guid eventId, Guid userId) {
    if (eventId == Guid.Empty || userId == Guid.Empty)
      throw new ApplicationException("Event and User ID are required");

    try {
      var result = await _mongoDb.Feedback
        .Find(x => x.EventId == eventId && x.UserFeedback
          .Any(y => y.UserId == userId))
        .FirstOrDefaultAsync();
      return result;
    }
    catch (Exception ex) {
      throw new ApplicationException(
        $"An error occurred while getting feedback for EventId {eventId} and UserID {userId}: {ex.Message}");
    }
  }

  public async Task<Feedback?> GetFeedbackById (Guid feedbackID) {
    if (feedbackID == Guid.Empty)
      throw new ApplicationException("Feedback ID is required.");

    try {
      var result = await _mongoDb.Feedback
        .Find(x => x.UserFeedback
          .Any(y => y.FeedbackId == feedbackID))
        .FirstOrDefaultAsync();
      return result;
    }
    catch (Exception ex) {
      throw new ApplicationException($"An error occurred while getting feedback for FeedbackId {feedbackID}: {ex.Message}");
    }
  }

  public async Task<Feedback?> AddFeedback(Feedback feedbackObj) {
    if (feedbackObj.EventId == Guid.Empty || feedbackObj.UserFeedback.First().UserId == Guid.Empty)
      throw new ApplicationException("User ID and Event ID are required.");

    if (await _eventService.GetEventById(feedbackObj.EventId) == null)
      throw new ApplicationException($"Event with ID {feedbackObj.EventId} not found.");
    
    if (await GetFeedbackByEventAndUser(feedbackObj.EventId, feedbackObj.UserFeedback.First().UserId) != null)
      throw new ApplicationException($"Feedback for EventId {feedbackObj.EventId} already exists.");
    
    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();
      try {
        var feedbackId = Guid.NewGuid();
        var feedback = new Feedback {
          EventId = feedbackObj.EventId,
          UserFeedback = [
            new UserFeedback {
              FeedbackId = feedbackId,
              UserId = feedbackObj.UserFeedback.First().UserId,
              Rating = feedbackObj.UserFeedback.First().Rating,
              Comment = feedbackObj.UserFeedback.First().Comment
            }
          ]
        };

        await _mongoDb.Feedback.InsertOneAsync(session, feedback);
        await session.CommitTransactionAsync();
        return await GetFeedbackById(feedbackId);
      }
      catch (Exception ex) {
        await session.AbortTransactionAsync();
        throw new ApplicationException($"An error occurred while adding feedback for EventId {feedbackObj.EventId}: {ex.Message}");
      }
    }
  }

  public async Task<Feedback?> UpdateFeedback (Feedback feedbackObj) {
    if (feedbackObj.UserFeedback.First().UserId == Guid.Empty || feedbackObj.EventId == Guid.Empty)
      throw new ApplicationException("User ID and Event ID are required.");
    
    if (await GetFeedbackByEventAndUser(feedbackObj.EventId, feedbackObj.UserFeedback.First().UserId) == null)
      throw new ApplicationException($"Feedback for EventId {feedbackObj.EventId} does not exist.");
    
    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();

      try {
        var feedbackUserId = feedbackObj.UserFeedback.First().UserId;
        var filter = Builders<Feedback>.Filter.And(
          Builders<Feedback>.Filter.Eq(x => x.EventId, feedbackObj.EventId),
          Builders<Feedback>.Filter.ElemMatch(x => x.UserFeedback, y => y.UserId == feedbackUserId)
        );
        var updateDefinition = BuildUpdateDefinition(feedbackObj);
        var result = await _mongoDb.Feedback.UpdateOneAsync(session, filter, updateDefinition);
        await session.CommitTransactionAsync();
        return result.MatchedCount > 0 ? feedbackObj : null;
      }
      catch (Exception ex) {
        await session.AbortTransactionAsync();
        throw new ApplicationException(
          $"An error occurred while updating feedback with ID: {feedbackObj.UserFeedback.First().FeedbackId}: {ex.Message}");
      }
    }
  }

  public async Task<Feedback?> DeleteFeedback (Feedback feedbackObj) {
    if (feedbackObj.UserFeedback.First().UserId == Guid.Empty || feedbackObj.EventId == Guid.Empty)
      throw new ApplicationException("User ID and Event ID are required.");
    
    if (await GetFeedbackByEventAndUser(feedbackObj.EventId, feedbackObj.UserFeedback.First().UserId) == null)
      throw new ApplicationException($"Feedback for EventId {feedbackObj.EventId} does not exist.");

    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();

      try {
        var feedbackUserId = feedbackObj.UserFeedback.First().UserId;
        var filter = Builders<Feedback>.Filter.And(
          Builders<Feedback>.Filter.Eq(x => x.EventId, feedbackObj.EventId),
          Builders<Feedback>.Filter.ElemMatch(x => x.UserFeedback, y => y.UserId == feedbackUserId)
        );
        var result = await _mongoDb.Feedback.DeleteOneAsync(session, filter);
        await session.CommitTransactionAsync();
        return result.DeletedCount > 0 ? feedbackObj : null;
      }
      catch (Exception ex) {
        throw new ApplicationException(
          $"An error occurred while deleting feedback with ID: {feedbackObj.UserFeedback.First().FeedbackId}: {ex.Message}");
      }
    }
  }

  public async Task<(List<UserFeedback>?, int)> GetFeedbacksByEventId (Guid eventId, int pageNumber, int pageSize) {
    if (eventId == Guid.Empty)
      throw new ApplicationException("Event ID is required.");

    if (await _eventService.GetEventById(eventId) == null)
      throw new ApplicationException($"Event with ID {eventId} not found.");

    try {
      var feedbackObjects = await _mongoDb.Feedback
        .Find(x => x.EventId == eventId)
        .Skip((pageNumber - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();

      if (feedbackObjects.Count == 0)
        throw new ApplicationException($"No feedback found for EventId {eventId}.");

      var userFeedbacks = feedbackObjects.SelectMany(x => x.UserFeedback).ToList();
      var numberOfUserFeedbacks = await _mongoDb.Feedback.CountDocumentsAsync(x => x.EventId == eventId);
      return (userFeedbacks, (int)numberOfUserFeedbacks);
    }
    catch (Exception ex) {
      throw new ApplicationException($"An error occurred when fetching feedback for Event Id {eventId}: {ex.Message}");
    }
  }

  public async Task<double> GetAverageRating (Guid eventId) {
    if (eventId == Guid.Empty)
      throw new ApplicationException("Event ID is required.");

    if (await _eventService.GetEventById(eventId) == null)
      throw new ApplicationException($"Event with ID {eventId} not found.");

    try {
      var feedbackObjects = await _mongoDb.Feedback
        .Find(x => x.EventId == eventId)
        .ToListAsync();

      if (feedbackObjects.Count == 0)
        throw new ApplicationException($"No feedback found for EventId {eventId}.");

      return Math.Ceiling(feedbackObjects.SelectMany(x => x.UserFeedback.Select(y => y.Rating)).ToList().Average());
    }
    catch (Exception ex) {
      throw new ApplicationException($"An error occurred while getting average rating for event with ID {eventId}: {ex.Message}");
    }
  }
}
