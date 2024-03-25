namespace waves_events.Models;

public class AppSettings {
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string SigningAlgorithm { get; set; } = string.Empty;
}