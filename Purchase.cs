using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VirtualEventTicketingSystem.Models
{
    public class Purchase
    {
        public int PurchaseId { get; set; }
    
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
    
        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; }
    
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    
        public decimal TotalCost { get; set; }
    
        public List<EventPurchase> EventPurchases { get; set; } = new();
    }
}