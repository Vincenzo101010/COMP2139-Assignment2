using System.ComponentModel.DataAnnotations;

namespace VirtualEventTicketingSystem.Models;

public class EventPurchase
{
    public int EventId { get; set; }
    public Event Event { get; set; } = default!;

    public int PurchaseId { get; set; }  // <--- foreign key
    public Purchase Purchase { get; set; } = default!;

    public int Quantity { get; set; }
    
}