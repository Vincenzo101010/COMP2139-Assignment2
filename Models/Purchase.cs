using System.ComponentModel.DataAnnotations;

namespace VirtualEventTicketingSystem.Models;

public class Purchase
{
    public int PurchaseId { get; set; }   // ✅ match everywhere

    [Required]
    public string GuestName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string GuestEmail { get; set; } = string.Empty;

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    public decimal TotalCost { get; set; }

    public ICollection<EventPurchase> EventPurchases { get; set; } = new List<EventPurchase>();
}