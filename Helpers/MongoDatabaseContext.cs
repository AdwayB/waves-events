using MongoDB.Driver;
using waves_events.Interfaces;
using waves_events.Models;

namespace waves_events.Helpers;

public class MongoDatabaseContext : IMongoDatabaseContext {
  private readonly IMongoDatabase _database;
  private readonly MongoClient _client;
  private static readonly List<string> SampleEventGuids = [
    "71eef77e-b15e-4015-8cfa-119473558f94",
    "e65ccc81-877a-4e0b-bb57-8afc00ee852f",
    "1c3a351d-d387-402e-9a2a-0a2d4bcdf5a8",
    "d0aee170-f7f8-45df-bc33-6db97e69b972",
    "44d96359-2ca8-4c41-a686-0548b4d24225",
    "afddc640-c620-44d4-b656-3a6a8dbd6287",
    "c4e080d2-5540-4c19-be11-e4d6b4d19158",
    "9e69d341-904b-468b-86ed-507131a26fad",
    "38cc9080-e997-46af-83e0-f978f6b90577",
    "d1ad432f-2c36-4e09-b99b-0b00d3fc3450",
    "2f53fe56-0727-4943-bfb1-9339922bdf66",
    "3749cd1b-f74c-4374-958c-bd3b9a08f51a",
    "c97e223b-adff-4462-a453-f8c4bb09d794",
    "eab4ec7f-2235-49fb-97ab-851229611522",
    "346b04d7-4b86-4d14-8a52-2c19e56816f5",
    "50af95da-60c3-47f1-84f7-18fb82b434ff",
    "00d63a30-ecca-409b-9d50-5c7b7d47d8c4",
    "44fff549-d81f-41a9-9e28-a7b74c7d6e02",
    "ab229897-05ea-47ed-8d6b-0742bd194e53",
    "74370b59-e575-41aa-bce5-5e7206d3cd16"
  ];

  private static readonly List<string> SampleUserIds = [
    "3b7b909d-2363-46e6-b4d8-5de3b9b2a7ee",
    "9acdbcc1-383b-49fb-959e-64417fdae795",
    "b264e735-ae0e-4e5b-8578-25407d4bcf72",
    "b2e48f0a-ba7e-4e09-a72f-60ab5dab5dfe",
    "66dd802d-d91a-4d73-a944-23d10e9e3a6a",
    "07d16a43-8585-4437-acf1-e404856859c5",
    "b823a0c0-d3bc-4012-b261-80b45053326f",
    "1f1b4106-1d0d-4ae5-850f-824254d18f3c",
    "b4a21caf-f9c9-4201-8412-662ecc06ea30",
    "f2859f6a-f78a-4384-a8e4-f38d2fd76a52",
    "f6b270e2-b926-41e6-a28b-c7683449cd31"
  ];
  
  private static readonly List<string> SampleAdminUserIds = [
    "3b7b909d-2363-46e6-b4d8-5de3b9b2a7ee",
    "9acdbcc1-383b-49fb-959e-64417fdae795",
    "b264e735-ae0e-4e5b-8578-25407d4bcf72",
    "b2e48f0a-ba7e-4e09-a72f-60ab5dab5dfe",
    "66dd802d-d91a-4d73-a944-23d10e9e3a6a",
    "07d16a43-8585-4437-acf1-e404856859c5",
    "b823a0c0-d3bc-4012-b261-80b45053326f",
    "1f1b4106-1d0d-4ae5-850f-824254d18f3c",
    "b4a21caf-f9c9-4201-8412-662ecc06ea30",
    "f2859f6a-f78a-4384-a8e4-f38d2fd76a52",
    "f6b270e2-b926-41e6-a28b-c7683449cd31"
  ];

  private static readonly List<string> SamplePaymentGuids = [
    "b813d22c-7284-4cc5-8053-9d0d6c3d675c",
    "7d4f4398-50a9-4f81-8f14-9c107ad3bbe4",
    "fb8ca586-6ee5-4010-af2f-8483af15c28e",
    "70295537-62f8-464a-9977-440cc3e9e3c2",
    "0798b561-c85c-472a-9ba1-04b60fdb7702",
    "a1e1e7c1-f336-46f9-9239-ac368530fa1b",
    "e51d20c1-adb4-4d84-90a6-27a96f852a61",
    "afe4d3dc-6f92-40cb-a07b-a1dea4177368",
    "0a84c9b8-8bc1-4edb-b2fb-93a83190b2e2",
    "9787d44a-6f42-4312-b62b-1e04d296673c"
  ];

  private static readonly List<string> SampleInvoiceGuids = [
    "00b42c01-4263-4f65-9ace-d742a0df1905",
    "42359259-2056-4fd3-9499-11d6a2ff37ee",
    "75b94627-c946-4ff2-898f-228f7a875673",
    "95583f39-7186-4072-883c-2e5884b216c0",
    "448bad79-5214-4ea3-92df-2484a3a2b925",
    "e6d09363-a07d-4bfd-b28b-41a09880ed8a",
    "713dfb8b-801e-4945-9516-d7fce403b342",
    "ccf649ad-8bb9-42f6-8afb-9a8e909ee3b7",
    "7efd962f-1e6b-49d7-b424-3ea3c48ed76c",
    "b35f2b55-ff5c-4e3f-bf15-947bef2f0309"
  ];
  
  private static readonly List<double[]> SampleCoordinates = [
    [-73.935242, 40.730610], // New York
    [-0.127758, 51.507351], // London
    [2.352222, 48.856613], // Paris
    [139.650311, 35.676192], // Tokyo
    [151.209900, -33.865143] // Sydney
  ]; 

  public MongoDatabaseContext(IConfiguration configuration) {
    _client = new MongoClient(configuration.GetConnectionString("MongoConnection"));
    _database = _client.GetDatabase("waves-events");
  }

  public IMongoCollection<Events> Events => _database.GetCollection<Events>("Events");
  public IMongoCollection<Feedback> Feedback => _database.GetCollection<Feedback>("Feedback");
  public IMongoCollection<Payments> Payments => _database.GetCollection<Payments>("Payments");

  public Task<IClientSessionHandle> StartSessionAsync(CancellationToken cancellationToken = default) {
    return _client.StartSessionAsync(cancellationToken: cancellationToken);
  }

  public async Task EnsureIndexesCreatedAsync() {
    await Events.Indexes.CreateManyAsync(
      [
        new CreateIndexModel<Events>(
          Builders<Events>.IndexKeys.Ascending(e => e.EventId),
          new CreateIndexOptions { Unique = false }
        ),
        new CreateIndexModel<Events>(
          Builders<Events>.IndexKeys.Geo2DSphere(e => e.EventLocation)
          )
      ]
    );

    await Feedback.Indexes.CreateManyAsync(
      [
        new CreateIndexModel<Feedback>(
          Builders<Feedback>.IndexKeys.Ascending(e => e.EventId),
          new CreateIndexOptions { Unique = false }
        ),
        new CreateIndexModel<Feedback>(
          Builders<Feedback>.IndexKeys.Ascending("UserFeedback.FeedbackId")
        ),
        new CreateIndexModel<Feedback>(
          Builders<Feedback>.IndexKeys.Ascending("UserFeedback.UserId")
        )
      ]
    );

    await Payments.Indexes.CreateManyAsync(
      [
        new CreateIndexModel<Payments>(
          Builders<Payments>.IndexKeys.Ascending(e => e.UserId),
          new CreateIndexOptions { Unique = false }
        ),
        new CreateIndexModel<Payments>(
          Builders<Payments>.IndexKeys.Ascending("PaymentDetails.EventId")
        )
      ]
    );
  }

  public async Task SeedDataAsync() {
    var emptyEvents = await _database.GetCollection<Events>("Events").CountDocumentsAsync(_ => true) == 0;
    var emptyPayments = await _database.GetCollection<Payments>("Payments").CountDocumentsAsync(_ => true) == 0;
    var emptyFeedbacks = await _database.GetCollection<Feedback>("Feedback").CountDocumentsAsync(_ => true) == 0;
    
    if (emptyEvents) {
      var events = CreateSampleEvents();
      await _database.GetCollection<Events>("Events").InsertManyAsync(events);
    }

    if (emptyPayments) {
      var payments = CreateSamplePayments();
      await _database.GetCollection<Payments>("Payments").InsertManyAsync(payments);
    }

    if (emptyFeedbacks) {
      var feedbacks = CreateSampleFeedbacks();
      await _database.GetCollection<Feedback>("Feedback").InsertManyAsync(feedbacks);
    }
  }
  
  private static List<Events> CreateSampleEvents() {
    var rnd = new Random();
    return SampleEventGuids.Select((guid, index) => {
      var randomStartOffset = rnd.Next(2, 20);
      var randomEndOffset = randomStartOffset + rnd.Next(1, 4);

      return new Events {
        EventId = Guid.Parse(guid),
        EventName = $"Event Name for {guid[..8]}",
        EventDescription = "Description of the event.",
        EventBackgroundImage = $"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/"
                               + $"w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==",
        EventTotalSeats = 100,
        EventRegisteredSeats = 0,
        EventTicketPrice = 49.99,
        EventGenres = ["Music", "Dance"],
        EventCollab = SampleAdminUserIds.OrderBy(_ => Guid.NewGuid())
          .Take(2).Where(x => x != guid).Select(Guid.Parse)
          .ToList(),
        EventStartDate = DateTime.UtcNow.AddDays(randomStartOffset),
        EventEndDate = DateTime.UtcNow.AddDays(randomEndOffset),
        EventLocation = new Location {
          Type = "Point",
          Coordinates = SampleCoordinates[index % SampleCoordinates.Count]
        },
        EventStatus = "Scheduled",
        EventCreatedBy = Guid.Parse(SampleAdminUserIds[rnd.Next(SampleAdminUserIds.Count)]),
        EventAgeRestriction = 18,
        EventCountry = "Sample Country",
        EventDiscounts = [new DiscountCodes { discountCode = "SAVE10", discountPercentage = 10 }]
      };
    }).ToList();
  }
  
  private static List<Payments> CreateSamplePayments() {
    var rnd = new Random();
    var paymentStatuses = Enum.GetValues(typeof(PaymentStatus))
      .Cast<PaymentStatus>().Select(s => s.ToString()).ToList();
    var payments = SampleUserIds.Take(5).Select(userId => new Payments {
      UserId = Guid.Parse(userId),
      PaymentDetails = SamplePaymentGuids.Select(paymentGuid => new PaymentDetails {
        EventId = Guid.Parse(SampleEventGuids[rnd.Next(SampleEventGuids.Count)]),
        PaymentId = Guid.Parse(paymentGuid),
        InvoiceId = Guid.Parse(SampleInvoiceGuids[rnd.Next(SampleInvoiceGuids.Count)]),
        Amount = rnd.NextDouble() * (500 - 50) + 50,
        Status = paymentStatuses[rnd.Next(paymentStatuses.Count)]
      }).ToList()
    }).ToList();
    return payments;
  }
  
  private static List<Feedback> CreateSampleFeedbacks() {
    var rnd = new Random();
    var feedbacks = SampleEventGuids.Take(3).Select(eventGuid => new Feedback {
      EventId = Guid.Parse(eventGuid),
      UserFeedback = SampleUserIds.Take(5).Select(userId => new UserFeedback {
        FeedbackId = Guid.NewGuid(),
        UserId = Guid.Parse(userId),
        Rating = rnd.Next(1, 6),
        Comment = "This is an example comment."
      }).ToList()
    }).ToList();
    return feedbacks;
  }
}
