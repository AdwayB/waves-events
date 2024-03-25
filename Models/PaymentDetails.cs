using System.ComponentModel.DataAnnotations;

namespace waves_events.Models;

public enum PaymentStatus {
    Pending,
    Success,
    Failed
};

public class PaymentDetails {
    [Required]
    public Guid EventId { get; set; } = Guid.Empty;
    
    [Required]
    public Guid PaymentId { get; set; } = Guid.Empty;
    
    [Required]
    public Guid InvoiceId { get; set; } = Guid.Empty;
    
    [Required]
    public double Amount { get; set; } = 0;
    
    [Required]
    public string Status { get; set; } = PaymentStatus.Pending.ToString();
}