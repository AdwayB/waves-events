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

  public async Task<UserFeedback?> GetFeedbackByEventAndUser (Guid eventId, Guid userId) {
    if (eventId == Guid.Empty || userId == Guid.Empty)
      throw new ApplicationException("Event and User ID are required");

    try {
      var result = await _mongoDb.Feedback
        .Find(x => x.EventId == eventId && x.UserFeedback
          .Any(y => y.UserId == userId))
        .FirstOrDefaultAsync();

      return result?.UserFeedback.Find(x => x.UserId == userId);
    }
    catch (Exception ex) {
      throw new ApplicationException(
        $"An error occurred while getting feedback for EventId {eventId} and UserID {userId}: {ex.Message}");
    }
  }

  public async Task<UserFeedback?> GetFeedbackById (Guid feedbackID) {
    if (feedbackID == Guid.Empty)
      throw new ApplicationException("Feedback ID is required.");

    try {
      var result = await _mongoDb.Feedback
        .Find(x => x.UserFeedback
          .Any(y => y.FeedbackId == feedbackID))
        .FirstOrDefaultAsync();
      return result?.UserFeedback.Find(x => x.FeedbackId == feedbackID);
    }
    catch (Exception ex) {
      throw new ApplicationException($"An error occurred while getting feedback for FeedbackId {feedbackID}: {ex.Message}");
    }
  }

  public async Task<UserFeedback?> AddFeedback(AddFeedbackRequest feedbackObj) {
    if (!Guid.TryParse(feedbackObj.UserId, out var userId) || 
        !Guid.TryParse(feedbackObj.EventId, out var eventId) ||
        userId == Guid.Empty || eventId == Guid.Empty)
      throw new ApplicationException("User ID and Event ID are required.");

    var obj = await GetFeedbackByEventAndUser(eventId, userId);
    
    if (obj != null)
      return null;
    
    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();
      try {
        var feedbackId = Guid.NewGuid();
        
        var feedback = new Feedback {
          EventId = eventId,
          UserFeedback = [
            new UserFeedback {
              FeedbackId = feedbackId,
              UserId = userId,
              Rating = feedbackObj.Rating,
              Comment = feedbackObj.Comment
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

  public async Task<UserFeedback?> UpdateFeedback (UpdateFeedbackRequest feedbackObj) {
    if (!Guid.TryParse(feedbackObj.UserId, out var userId) || 
        !Guid.TryParse(feedbackObj.EventId, out var eventId) ||
        userId == Guid.Empty || eventId == Guid.Empty)
      throw new ApplicationException("User ID and Event ID are required.");
    
    var obj = await GetFeedbackByEventAndUser(eventId, userId);

    if (obj == null)
      return null;
    
    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();

      try {
        var feedbackId = obj.FeedbackId;
        
        var filter = Builders<Feedback>.Filter.And(
          Builders<Feedback>.Filter.Eq(x => x.EventId, eventId),
          Builders<Feedback>.Filter.ElemMatch(x => x.UserFeedback, y => y.UserId == userId)
        );
        var updateDefinition = Builders<Feedback>.Update.Combine(
          Builders<Feedback>.Update.Set(x => x.UserFeedback.First().Rating, feedbackObj.Rating),
          Builders<Feedback>.Update.Set(x => x.UserFeedback.First().Comment, feedbackObj.Comment)
          );
        
        var result = await _mongoDb.Feedback.UpdateOneAsync(session, filter, updateDefinition);
        await session.CommitTransactionAsync();
        return result.MatchedCount > 0 ? await GetFeedbackById(feedbackId) : null;
      }
      catch (Exception ex) {
        await session.AbortTransactionAsync();
        throw new ApplicationException(
          $"An error occurred while updating feedback with ID: {feedbackObj.FeedbackId}: {ex.Message}");
      }
    }
  }

  public async Task<Guid?> DeleteFeedback (UpdateFeedbackRequest feedbackObj) {
    if (!Guid.TryParse(feedbackObj.UserId, out var userId) || 
        !Guid.TryParse(feedbackObj.EventId, out var eventId) ||
        userId == Guid.Empty || eventId == Guid.Empty)
      throw new ApplicationException("User ID and Event ID are required.");
    
    var obj = await GetFeedbackByEventAndUser(eventId, userId);

    if (obj == null)
      return null;

    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();

      try {
        var feedbackId = obj.FeedbackId;
        
        var filter = Builders<Feedback>.Filter.ElemMatch(x => x.UserFeedback, y => y.FeedbackId == feedbackId);
        var result = await _mongoDb.Feedback.DeleteOneAsync(session, filter);
        await session.CommitTransactionAsync();
        return result.DeletedCount > 0 ? feedbackId : null;
      }
      catch (Exception ex) {
        throw new ApplicationException(
          $"An error occurred while deleting feedback with ID: {feedbackObj.FeedbackId}: {ex.Message}");
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
        return ([], 0);

      var userFeedbacks = feedbackObjects.SelectMany(x => x.UserFeedback).ToList();
      var numberOfUserFeedbacks = userFeedbacks.Count;
      return (userFeedbacks, numberOfUserFeedbacks);
    }
    catch (Exception ex) {
      throw new ApplicationException($"An error occurred when fetching feedback for Event Id {eventId}: {ex.Message}");
    }
  }

  public async Task<double?> GetAverageRating (Guid eventId) {
    if (eventId == Guid.Empty)
      throw new ApplicationException("Event ID is required.");

    if (await _eventService.GetEventById(eventId) == null)
      throw new ApplicationException($"Event with ID {eventId} not found.");

    try {
      var feedbackObjects = await _mongoDb.Feedback
        .Find(x => x.EventId == eventId)
        .ToListAsync();

      return feedbackObjects.Count == 0 
      ? 0
      : feedbackObjects.SelectMany(x => x.UserFeedback.Select(y => y.Rating)).ToList().Average();
    }
    catch (Exception ex) {
      throw new ApplicationException($"An error occurred while getting average rating for event with ID {eventId}: {ex.Message}");
    }
  }
}
