using MongoDB.Driver;
using waves_events.Models;

namespace waves_events.Interfaces;

public interface IMongoDatabaseContext {
  IMongoCollection<Events> Events { get; }
  IMongoCollection<Feedback> Feedback { get; }
  IMongoCollection<Payments> Payments { get; }
  Task<IClientSessionHandle> StartSessionAsync(CancellationToken cancellationToken = default);
}