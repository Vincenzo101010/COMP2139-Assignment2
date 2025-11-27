using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketingSystem.Models;

namespace VirtualEventTicketingSystem.Controllers;

public class PurchasesController : Controller
{
    private readonly ApplicationDbContext _context;

    public PurchasesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Purchases/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Events = await _context.Events.ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Purchase purchase, int[] eventIds, int[] quantities)
    {
        if (eventIds == null || quantities == null || eventIds.Length != quantities.Length)
        {
            ModelState.AddModelError("", "Invalid ticket selection.");
            ViewBag.Events = await _context.Events.ToListAsync();
            return View(purchase);
        }

        purchase.EventPurchases = new List<EventPurchase>();
        decimal totalCost = 0;

        for (int i = 0; i < eventIds.Length; i++)
        {
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventIds[i]);
            if (ev == null) continue;

            var quantity = quantities[i];
            if (quantity <= 0) continue;

            totalCost += ev.TicketPrice * quantity;

            purchase.EventPurchases.Add(new EventPurchase
            {
                EventId = ev.Id,
                Event = ev,                // ✅ explicitly attach Event object
                Quantity = quantity
            });
        }

        purchase.TotalCost = totalCost;

        // ✅ Ensure all event details are loaded before sending to view
        foreach (var ep in purchase.EventPurchases)
        {
            ep.Event = await _context.Events.FirstOrDefaultAsync(e => e.Id == ep.EventId);
        }

        return View("Confirmation", purchase);
    }
    
    public async Task<IActionResult> Index()
    {
        var purchases = await _context.Purchases
            .Include(p => p.EventPurchases)
            .ThenInclude(ep => ep.Event)
            .OrderByDescending(p => p.PurchaseDate)
            .ToListAsync();

        return View(purchases);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(Purchase purchase, int[] eventIds, int[] quantities)
    {
        if (eventIds == null || quantities == null || eventIds.Length != quantities.Length)
        {
            ModelState.AddModelError("", "Invalid event selection.");
            return View("Confirmation", purchase);
        }

        // Create a new purchase record
        var newPurchase = new Purchase
        {
            GuestName = purchase.GuestName,
            GuestEmail = purchase.GuestEmail,
            PurchaseDate = DateTime.UtcNow,
            TotalCost = 0
        };

        _context.Purchases.Add(newPurchase);
        await _context.SaveChangesAsync(); // ✅ Commit early so we have a valid PurchaseId

        for (int i = 0; i < eventIds.Length; i++)
        {
            int eventId = eventIds[i];
            int qty = quantities[i];

            // Get the event fresh from DB (no tracking to avoid stale cache)
            var ev = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null)
            {
                ModelState.AddModelError("", $"Event with ID {eventId} not found.");
                return View("Confirmation", purchase);
            }

            if (qty < 1)
            {
                ModelState.AddModelError("", $"Invalid ticket quantity for {ev.Title}.");
                return View("Confirmation", purchase);
            }

            if (ev.AvailableTickets < qty)
            {
                ModelState.AddModelError("", $"Not enough tickets left for {ev.Title}. Only {ev.AvailableTickets} remaining.");
                return View("Confirmation", purchase);
            }

            // Deduct tickets
            ev.AvailableTickets -= qty;

            // Reattach event as modified so EF updates it
            _context.Events.Attach(ev);
            _context.Entry(ev).Property(e => e.AvailableTickets).IsModified = true;

            // Add the purchase-event relationship
            _context.EventPurchases.Add(new EventPurchase
            {
                PurchaseId = newPurchase.PurchaseId,
                EventId = ev.Id,
                Quantity = qty
            });

            newPurchase.TotalCost += ev.TicketPrice * qty;

            Console.WriteLine($"🎟 Updated {ev.Title}: new AvailableTickets = {ev.AvailableTickets}");
        }

        // ✅ Commit all changes (tickets + purchase)
        var rows = await _context.SaveChangesAsync();

        return RedirectToAction("Confirmed");
    }


    // GET: Purchases/Confirmation/5
    public IActionResult Confirmed()
    {
        return View();
    }
    
    // GET: Purchases/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var purchase = await _context.Purchases
            .Include(p => p.EventPurchases)
            .ThenInclude(ep => ep.Event)
            .FirstOrDefaultAsync(p => p.PurchaseId == id);

        if (purchase == null)
        {
            return NotFound();
        }

        return View(purchase);
    }

// POST: Purchases/DeleteConfirmed/5
    [HttpPost, ActionName("DeleteConfirmed")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var purchase = await _context.Purchases
            .Include(p => p.EventPurchases)
            .ThenInclude(ep => ep.Event)
            .FirstOrDefaultAsync(p => p.PurchaseId == id);

        if (purchase == null)
        {
            return NotFound();
        }

        // Restore tickets for all related events
        foreach (var ep in purchase.EventPurchases)
        {
            if (ep.Event != null)
            {
                ep.Event.AvailableTickets += ep.Quantity;
                _context.Events.Update(ep.Event);
            }
        }

        // Remove related EventPurchase entries
        _context.EventPurchases.RemoveRange(purchase.EventPurchases);

        // Remove the purchase
        _context.Purchases.Remove(purchase);

        await _context.SaveChangesAsync();

        TempData["Message"] = "Purchase deleted and tickets restored successfully.";
        return RedirectToAction(nameof(Index));
    }
}