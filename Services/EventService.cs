using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using waves_events.Handlers;
using waves_events.Interfaces;
using waves_events.Models;

namespace waves_events.Services;

public class EventService : IEventService {
  private readonly IMongoDatabaseContext _mongoDb;
  private readonly IDomainEventDispatcher _eventDispatcher;

  public EventService(IMongoDatabaseContext mongoDb, IDomainEventDispatcher eventDispatcher) {
    _mongoDb = mongoDb;
    _eventDispatcher = eventDispatcher;
  }
  
  private UpdateDefinition<Events> BuildUpdateDefinition(Events existingEvent, UpdateEventRequest updateEventRequest) {
    var updateDefinitionBuilder = new List<UpdateDefinition<Events>>();
    var requestProperties = typeof(UpdateEventRequest).GetProperties();

    foreach (var property in requestProperties) {
      var requestValue = property.GetValue(updateEventRequest);
      var eventProperty = typeof(Events).GetProperty(property.Name);
        
      if (eventProperty == null)
        continue;

      var eventValue = eventProperty.GetValue(existingEvent);
      var defaultValue = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;

      if (requestValue == null || requestValue.Equals(defaultValue))
        continue;

      if (requestValue.Equals(eventValue))
        continue;
      
      switch (property.Name) {
        case "EventLocation" when requestValue is Location { Coordinates: not { Length: 2 } }:
        case "EventCollab" when requestValue is List<Guid> { Count: 0 }:
        case "EventDiscounts" when requestValue is List<DiscountCodes> { Count: 0 }:
          continue;
        default: {
          var update = Builders<Events>.Update.Set(eventProperty.Name, requestValue);
          updateDefinitionBuilder.Add(update);
          break;
        }
      }
    }
    return Builders<Events>.Update.Combine(updateDefinitionBuilder);
  }

  public async Task<Events?> GetEventById(Guid id) {
    if (id == Guid.Empty)
      throw new ApplicationException("Event ID cannot be empty.");
    
    try {
      return await _mongoDb.Events.Find(x => x.EventId == id).FirstOrDefaultAsync();
    }
    catch (Exception ex) {
      throw new ApplicationException("An error occurred while fetching events: " + ex.Message);
    }
  }

  public async Task<List<Events>?> GetEventByIdList (List<Guid> ids) {
    if (ids.Contains(Guid.Empty) || ids.Count == 0)
      throw new ApplicationException("Event IDs cannot be empty.");

    try {
      var eventsFilter = Builders<Events>.Filter.In(e => e.EventId, ids);
      var events = await _mongoDb.Events.Find(eventsFilter).ToListAsync();

      if (events == null || events.Count == 0)
        throw new ApplicationException("No events found with the provided IDs.");
      return events;
    }
    catch (Exception ex) {
      throw new ApplicationException($"An error occurred while fetching events: {ex.Message}");
    }
  }

  public async Task<(List<Events>, int)> GetAllEvents(int pageNumber, int pageSize) {
    try {
      var numberOfRecords = await _mongoDb.Events.CountDocumentsAsync(_ => true);
      var response = await _mongoDb
        .Events.Find(_ => true)
        .Skip((pageNumber - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();
      
      return (response, (int)numberOfRecords);
    }
    catch (Exception ex) {
          throw new ApplicationException("An error occurred while fetching events: " + ex.Message);
    }
  }

  public async Task<(List<Events>, int)> GetEventsWithGenre(string genre, int pageNumber, int pageSize) {
    if (genre.Length == 0)
      throw new ApplicationException("Genre cannot be empty.");
    
    try {
      var numberOfRecords = await _mongoDb.Events.Find(x => x.EventGenres.Contains(genre)).CountDocumentsAsync();
      var response = await _mongoDb
        .Events.Find(x => x.EventGenres.Contains(genre))
        .Skip((pageNumber - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();

      return (response, (int)numberOfRecords);
    }
    catch (Exception ex) {
      throw new ApplicationException("An error occurred while fetching events: " + ex.Message);
    }
  }

  public async Task<(List<Events>, int)> GetEventsByArtist(Guid artistId, int pageNumber, int pageSize) {
    if (artistId == Guid.Empty)
      throw new ApplicationException("Artist ID cannot be empty.");
    
    try {
      var numberOfRecords = await _mongoDb.Events.Find(x => x.EventCreatedBy.Equals(artistId)).CountDocumentsAsync();
      var response = await _mongoDb
        .Events.Find(x => x.EventCreatedBy.Equals(artistId))
        .Skip((pageNumber - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();

      return (response, (int)numberOfRecords);
    }
    catch (Exception ex) {
      throw new ApplicationException("An error occurred while fetching events: " + ex.Message);
    }
  }
  
  public async Task<(List<Events>, int)> GetEventsByArtistCollab(Guid artistId, int pageNumber, int pageSize) {
    try {
      var numberOfRecords = await _mongoDb.Events.Find(x => x.EventCollab.Contains(artistId)).CountDocumentsAsync();
      var response = await _mongoDb
        .Events.Find(x => x.EventCollab.Contains(artistId))
        .Skip((pageNumber - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();

      return (response, (int)numberOfRecords);
    }
    catch (Exception ex) {
      throw new ApplicationException("An error occurred while fetching events: " + ex.Message);
    }
  }
  
  public async Task<(List<Events>, int)> GetEventsWithLocation(double[] location, double radius, int pageNumber, int pageSize) {
    if (location.Length != 2)
      throw new ApplicationException("Location array must be of the form [longitude, latitude].");
    if (radius <= 0)
      throw new ApplicationException("Radius must be greater than 0.");

    try {
      var radiusInRadians = radius / 6371;
      var locationPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
        new GeoJson2DGeographicCoordinates(location[0], location[1]));
        
      var filter = Builders<Events>.Filter.GeoWithinCenterSphere(
        field: e => e.EventLocation.Coordinates,
        x: locationPoint.Coordinates.Longitude,
        y: locationPoint.Coordinates.Latitude,
        radius: radiusInRadians
      );

      var response = await _mongoDb.Events.Find(filter)
        .Skip((pageNumber - 1) * pageSize).Limit(pageSize).ToListAsync();
      var totalItems = await _mongoDb.Events.Find(filter).CountDocumentsAsync();

      return (response, (int)totalItems);
    }
    catch (FormatException ex) {
      throw new ApplicationException("Error processing location coordinates: " + ex.Message, ex);
    }
    catch (Exception ex) {
      throw new ApplicationException("An error occurred while fetching events by location: " + ex.Message, ex);
    }
  }

  public async Task<(List<Events>, int)> GetEventsWithDateRange(DateTime startDate, DateTime endDate, int pageNumber, int pageSize) {
    if (startDate > endDate)
      throw new ApplicationException("Start date cannot be after end date.");

    try {
      var dateFilter = Builders<Events>.Filter.And(
        Builders<Events>.Filter.Gte(e => e.EventStartDate, startDate),
        Builders<Events>.Filter.Lte(e => e.EventEndDate, endDate)
      );
      
      var numberOfRecords = await _mongoDb.Events.Find(dateFilter).CountDocumentsAsync();
      var response = await _mongoDb
        .Events.Find(dateFilter)
        .Skip((pageNumber - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();

      return (response, (int)numberOfRecords);
    }
    catch (Exception ex) {
      throw new ApplicationException("An error occurred while fetching events: " + ex.Message);
    }
  }

  public async Task<Events?> CreateEvent(Events eventObj) {
    if (eventObj.EventId != Guid.Empty) {
      throw new ApplicationException($"EventId cannot be set by client.");
    }
    eventObj.EventId = Guid.NewGuid();

    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();
      try {
        var obj = await _mongoDb.Events
          .Find(session, e => e.EventId == eventObj.EventId)
          .FirstOrDefaultAsync();

        if (obj != null) 
          return null;
        
        await _mongoDb.Events.InsertOneAsync(session, eventObj);
        await session.CommitTransactionAsync();
        return eventObj;
      }
      catch (MongoException ex) {
        await session.AbortTransactionAsync();
        throw new ApplicationException($"An error occurred while creating Event: {ex.Message}\n" +
                                       $"{ex.StackTrace}");
      }
    }
  }
  
  public async Task<Events?> UpdateEvent(UpdateEventRequest eventRequest, bool sendMail) {
    if (!Guid.TryParse(eventRequest.EventId, out var eventGuid)) 
      throw new ApplicationException($"No eventId provided.");

    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();
      try {
        var obj = await _mongoDb.Events
          .Find(session, e => e.EventId == eventGuid)
          .FirstOrDefaultAsync();

        if (obj == null) 
          return null;
        
        var filter = Builders<Events>.Filter.Eq(e => e.EventId, eventGuid);
        var updateDefinition = BuildUpdateDefinition(obj, eventRequest);
        
        var result = await _mongoDb.Events.UpdateOneAsync(session, filter, updateDefinition);
        await session.CommitTransactionAsync();

        var resultObj = await GetEventById(eventGuid);
        if (sendMail)
            await _eventDispatcher.Dispatch(new EventUpdated(resultObj ?? new Events()));
        return result.MatchedCount == 0 ? null : resultObj;
      }
      catch (MongoException ex) {
        await session.AbortTransactionAsync();
        throw new ApplicationException($"An error occurred while updating Event: {ex.Message}\n" +
                                       $"{ex.StackTrace}");
      }
    }
  }

  public async Task<Events?> UpdateEventCollab (UpdateCollabRequest collabObj) {
    if(!Guid.TryParse(collabObj.EventId, out var eventId))
      throw new ApplicationException($"Invalid eventId provided.");
    
    if (eventId == Guid.Empty)
      throw new ApplicationException($"No eventId provided.");

    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();

      try {
        var obj = await _mongoDb.Events.Find(session, x => x.EventId == eventId).FirstOrDefaultAsync();

        if (obj == null)
          return null;

        var collabGuids = new List<Guid>();
        foreach (var id in collabObj.EventCollab) {
            if (Guid.TryParse(id, out var collabGuid))
              collabGuids.Add(collabGuid);
        }
        
        var result = await _mongoDb.Events.UpdateOneAsync(session, 
          x => x.EventId == eventId, 
          Builders<Events>.Update.Set(x => x.EventCollab, collabGuids)
          );
        await session.CommitTransactionAsync();

        return result.MatchedCount == 0 ? null : await _mongoDb.Events.Find(x => x.EventId == eventId).FirstOrDefaultAsync();
      }
      catch (MongoException ex) {
        await session.AbortTransactionAsync();
        throw new ApplicationException($"An error occurred while updating collab for Event {eventId} : {ex.Message}\n" +
                                       $"{ex.StackTrace}");
      }
    }
  }
  
  public async Task<Events?> UpdateEventDiscounts (UpdateDiscountsRequest discountsObj) {
    if(!Guid.TryParse(discountsObj.EventId, out var eventId))
      throw new ApplicationException($"Invalid eventId provided.");
    
    if (eventId == Guid.Empty)
      throw new ApplicationException($"No eventId provided.");

    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();

      try {
        var obj = await _mongoDb.Events.Find(session, x => x.EventId == eventId).FirstOrDefaultAsync();

        if (obj == null)
          return null;
        
        var result = await _mongoDb.Events.UpdateOneAsync(session, 
          x => x.EventId == eventId, 
          Builders<Events>.Update.Set(x => x.EventDiscounts, discountsObj.EventDiscounts)
        );
        await session.CommitTransactionAsync();
        
        return result.MatchedCount == 0 ? null : await _mongoDb.Events.Find(x => x.EventId == eventId).FirstOrDefaultAsync();
      }
      catch (MongoException ex) {
        await session.AbortTransactionAsync();
        throw new ApplicationException($"An error occurred while updating discounts for Event {eventId} : {ex.Message}\n" +
                                       $"{ex.StackTrace}");
      }
    }
  }
  
  public async Task<Guid?> DeleteEvent(Guid eventId) {
    if (eventId == Guid.Empty)
      throw new ApplicationException($"No eventId provided.");

    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();
      try {
        var obj = await _mongoDb.Events
          .Find(session, e => e.EventId == eventId)
          .FirstOrDefaultAsync();

        if (obj == null) return null;
        var result = await _mongoDb.Events.DeleteOneAsync(session, x => x.EventId == eventId);
        await _eventDispatcher.Dispatch(new EventDeleted(obj ?? new Events()));
        await session.CommitTransactionAsync();
        return result.DeletedCount == 0 ? null : eventId;
      }
      catch (MongoException ex) {
        await session.AbortTransactionAsync();
        throw new ApplicationException($"An error occurred while deleting Event: {ex.Message}\n" +
                                       $"{ex.StackTrace}");
      }
    }
  }
}
