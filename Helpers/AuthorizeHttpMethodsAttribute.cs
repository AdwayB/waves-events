using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;

namespace waves_events.Helpers;

[AttributeUsage(AttributeTargets.Method)]
public class AuthorizeHttpMethodsAttribute : Attribute, IAuthorizeData, IActionHttpMethodProvider {
  public string? Policy { get; set; }
  public string? Roles { get; set; }
  public string? AuthenticationSchemes { get; set; }
  private readonly string[] _httpMethods;

  public AuthorizeHttpMethodsAttribute(string roles, params string[] httpMethods) {
    Roles = roles;
    _httpMethods = httpMethods.Select(x => x.ToUpper()).ToArray();
  }

  public IEnumerable<string> HttpMethods => _httpMethods;
}