namespace waves_events.Middleware;

public class AuthInterceptor {
  private readonly RequestDelegate _next;

  public AuthInterceptor(RequestDelegate next) {
    _next = next;
  }

  public async Task Invoke(HttpContext context) {
    var jwt = context.Request.Cookies["jwt"];
    if (!string.IsNullOrEmpty(jwt)) {
      context.Request.Headers.Append("Authorization", "Bearer " + jwt);
    }

    await _next(context);
  }
}