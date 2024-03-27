using Microsoft.AspNetCore.Mvc;
using waves_events.Models;

namespace waves_events.Helpers;

public static class UserDetailsControllerExtension {
  public static (Guid, UserType)? GetUserDetailsFromContext (this ControllerBase controller) {
    if (!controller.HttpContext.Items.TryGetValue("UserID", out var userId) ||
        userId is not string id ||
        !controller.HttpContext.Items.TryGetValue("UserType", out var userType) ||
        userType is not string type
       ) {
      return null;
    }
    
    if (!Guid.TryParse(id, out var parsedUserID))
      return null;

    if (Enum.TryParse<UserType>(type, out var parsedUserType) && Enum.IsDefined(typeof(UserType), parsedUserType))
      return (parsedUserID, parsedUserType);
    
    return null;
  }
}