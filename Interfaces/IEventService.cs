using waves_events.Models;

namespace waves_events.Interfaces;

public interface IEventService {
    Task<Events?> GetEventById (Guid id);
    Task<List<Events>?> GetEventByIdList (List<Guid> ids);
    Task<(List<Events>, int)> GetAllEvents (int pageNumber, int pageSize);
    Task<(List<Events>, int)> GetEventsWithGenre (string genre, int pageNumber, int pageSize);
    Task<(List<Events>, int)> GetEventsByArtist (Guid artistId, int pageNumber, int pageSize);
    Task<(List<Events>, int)> GetEventsByArtistCollab (Guid artistId, int pageNumber, int pageSize);
    Task<(List<Events>, int)> GetEventsWithLocation(double[] location, double radius, int pageNumber, int pageSize);
    Task<(List<Events>, int)> GetEventsWithDateRange (DateTime startDate, DateTime endDate, int pageNumber, int pageSize);
    Task<Events?> CreateEvent (Events eventObj);
    Task<Events?> UpdateEvent (UpdateEventRequest eventObj);
    Task<Events?> UpdateEventCollab (UpdateCollabRequest collabObj);
    Task<Events?> UpdateEventDiscounts (UpdateDiscountsRequest discountsObj);
    Task<Guid?> DeleteEvent (Guid eventId);
}