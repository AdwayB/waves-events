using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using waves_events.Helpers;
using waves_events.Interfaces;
using waves_events.Models;

namespace waves_events.Services;

public class EventService : IEventService {
  private readonly MongoDatabaseContext _mongoDb;

  public EventService(MongoDatabaseContext mongoDb) {
    _mongoDb = mongoDb;
  }
  
  private UpdateDefinition<Events> BuildUpdateDefinition(Events eventObj) {
    var updateDefinitionBuilder = new List<UpdateDefinition<Events>>();
    var properties = typeof(Events).GetProperties();

    foreach (var property in properties) {
      var value = property.GetValue(eventObj);
      var defaultValue = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;

      if (value == null || value.Equals(defaultValue)) continue;
      var update = Builders<Events>.Update.Set(property.Name, value);
      updateDefinitionBuilder.Add(update);
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

  public async Task<List<Events>> GetEventByIdList (List<Guid> ids) {
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
      var locationPoint = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(
        new GeoJson2DGeographicCoordinates(location[0], location[1]));
      var radiusToMeters = radius * 1000;

      var filter = Builders<Events>.Filter.NearSphere(
        field: e => e.EventLocation,
        point: locationPoint,
        maxDistance: radiusToMeters
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
    if (eventObj.EventId == Guid.Empty) {
      throw new ApplicationException($"No eventId provided.");
    }

    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();
      try {
        var obj = await _mongoDb.Events
          .Find(session, e => e.EventId == eventObj.EventId)
          .FirstOrDefaultAsync();

        if (obj != null) return null;
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
  
  public async Task<Events?> UpdateEvent(Events eventObj) {
    if (eventObj.EventId == Guid.Empty) {
      throw new ApplicationException($"No eventId provided.");
    }

    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();
      try {
        var obj = await _mongoDb.Events
          .Find(session, e => e.EventId == eventObj.EventId)
          .FirstOrDefaultAsync();

        if (obj == null) return null;
        var filter = Builders<Events>.Filter.Eq(e => e.EventId, eventObj.EventId);
        var updateDefinition = BuildUpdateDefinition(eventObj);
        
        var result = await _mongoDb.Events.UpdateOneAsync(session, filter, updateDefinition);
        await session.CommitTransactionAsync();
        return result.MatchedCount == 0 ? null : eventObj;
      }
      catch (MongoException ex) {
        await session.AbortTransactionAsync();
        throw new ApplicationException($"An error occurred while updating Event: {ex.Message}\n" +
                                       $"{ex.StackTrace}");
      }
    }
  }
  
  public async Task<Guid?> DeleteEvent(Guid eventId) {
    if (eventId == Guid.Empty) {
      throw new ApplicationException($"No eventId provided.");
    }

    using (var session = await _mongoDb.StartSessionAsync()) {
      session.StartTransaction();
      try {
        var obj = await _mongoDb.Events
          .Find(session, e => e.EventId == eventId)
          .FirstOrDefaultAsync();

        if (obj == null) return null;
        var result = await _mongoDb.Events.DeleteOneAsync(session, x => x.EventId == eventId);
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
