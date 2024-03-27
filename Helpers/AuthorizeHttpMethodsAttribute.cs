using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using waves_events.Models;
using HttpMethods = waves_events.Models.HttpMethods;

namespace waves_events.Helpers;

[AttributeUsage(AttributeTargets.Method)]
public class AuthorizeHttpMethodsAttribute : Attribute, IAuthorizeData, IActionHttpMethodProvider {
  public string? Policy { get; set; }
  public string? Roles { get; set; }
  public string? AuthenticationSchemes { get; set; }
  public string Template { get; private set; }

  public AuthorizeHttpMethodsAttribute(AuthorizeUserEnum roles, HttpMethods httpMethod, string template) {
    Roles = roles == AuthorizeUserEnum.None ? null : string.Join(",", 
      Enum.GetValues(typeof(AuthorizeUserEnum))
        .Cast<AuthorizeUserEnum>()
        .Where(role => roles.HasFlag(role) && role != AuthorizeUserEnum.None)
        .Select(r => r.ToString()));

    HttpMethods = new[] { httpMethod.ToString() };
    Template = template;
  }

  public IEnumerable<string> HttpMethods { get; }
  public int? Order => null;
  public string Name => null;
}