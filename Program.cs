using System.Reflection;
using System.Text;
using dotenv.net;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using waves_events.Handlers;
using waves_events.Helpers;
using waves_events.Interfaces;
using waves_events.Middleware;
using waves_events.Models;
using waves_events.Services;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder
    .Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<EmailProviderConfig>(builder.Configuration.GetSection("BrevoConfig"));
builder.Services.AddSingleton<IMongoDatabaseContext, MongoDatabaseContext>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddTransient<IMailService, MailService>();
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

builder.Services.AddScoped<IDomainEventHandler<EventUpdated>, EventUpdatedHandler>();
builder.Services.AddScoped<IDomainEventHandler<EventDeleted>, EventDeletedHandler>();
builder.Services.AddScoped<IDomainEventHandler<EventRegistered>, EventRegisteredHandler>();
builder.Services.AddScoped<IDomainEventHandler<EventRegistrationCancelled>, EventRegistrationCancelledHandler>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters() {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "type"
        };
        options.Events = new JwtBearerEvents {
            OnTokenValidated = context => {  
                var userId = context.Principal?.FindFirst("userId")?.Value;
                var userType = context.Principal?.FindFirst("type")?.Value;
                if (userId == null) return Task.CompletedTask;
                context.HttpContext.Items["UserID"] = userId;
                context.HttpContext.Items["UserType"] = userType;
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context => {
                Console.WriteLine($"Authentication failed: Exception: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "Waves Events API", Version = "v1" });
  var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
  var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
  c.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options => {
  options.AddPolicy("AllowLocalhost3000", policyBuilder => {
    policyBuilder.WithOrigins("http://localhost:3000")
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials();
  });
});

builder.Services.AddSingleton<MongoDatabaseContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI(c =>
  {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Waves Events API V1");
    c.RoutePrefix = string.Empty;
  });
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalhost3000");
app.UseMiddleware<AuthInterceptor>();
app.UseAuthentication(); // Use authentication
app.UseAuthorization();

app.MapControllers();

try {
    var mongoContext = app.Services.GetRequiredService<MongoDatabaseContext>();
    await mongoContext.EnsureIndexesCreatedAsync();
    await mongoContext.SeedDataAsync();
}
catch (Exception ex) {
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while setting up MongoDB.");
}

app.Run();
