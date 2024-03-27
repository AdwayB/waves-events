namespace waves_events.Models;

[Flags]
public enum AuthorizeUserEnum {
  None = 0,
  Admin = 1,
  User = 2,
  Both = Admin | User
}