using Microsoft.EntityFrameworkCore;
using Order_Processing_API.DTOs;
using Order_Processing_API.Models;

namespace Order_Processing_API.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        app.MapPost("/orders", CreateOrder);
        app.MapGet("/orders/{id}", GetOrder);
        app.MapPost("/orders/{id}/confirm", ConfirmOrder);
        app.MapPost("/orders/{id}/cancel", CancelOrder);
    }
    
    // POST Generic
    private static async Task<IResult> CreateOrder(CreateOrderDto dto, AppDbContext db)
    {
        // Req 1: at least one item
        if (dto.Items.Count == 0)
            return Results.BadRequest("An order must contain at least one item.");
        
        // Req 2: all quantities > 0
        var invalidItems = dto.Items.Where(i => i.Quantity <= 0).ToList();
        if (invalidItems.Count > 0)
            return Results.BadRequest($"Item(s) with ProductId [{string.Join(", ", invalidItems.Select(i => i.ProductId))}] have an invalid quantity.");

        // Req 3: all products must exist
        var requestedIds = dto.Items.Select(i => i.ProductId).ToList();
        var foundProducts = await db.Products
            .Where(p => requestedIds.Contains(p.Id))
            .ToListAsync();
        
        var missingIds = requestedIds.Except(foundProducts.Select(p => p.Id)).ToList();
        if (missingIds.Count > 0)
            return Results.NotFound($"Product(s) not found: [{string.Join(", ", missingIds)}].");

        var order = new Order
        {
            CustomerEmail = dto.CustomerEmail,
            Status = "Draft",
            CreatedAtUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ssZ"),
            OrderItems = dto.Items.Select(i =>
            {
                var product = foundProducts.First(p => p.Id == i.ProductId);
                return new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.UnitPrice,
                    Quantity = i.Quantity,
                    LineTotal = product.UnitPrice * i.Quantity
                };
            }).ToList()
        };

        order.TotalAmount = order.OrderItems.Sum(i => i.LineTotal);

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return Results.Created($"/orders/{order.Id}", MapToDto(order));
    }
    
    // GET by id
    private static async Task<IResult> GetOrder(int id, AppDbContext db)
    {
        var order = await db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return Results.NotFound($"Order {id} not found.");

        return Results.Ok(MapToDto(order));
    }
    
    
    // POST confirmed order
    private static async Task<IResult> ConfirmOrder(int id, AppDbContext db)
    {
        var order = await db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return Results.NotFound($"Order {id} not found.");

        // Req 7-8: status gate
        if (order.Status != "Draft")
            return Results.Conflict($"Order cannot be confirmed from status '{order.Status}'.");
        
        // Req 5: check ALL items before touching anything
        var productIds = order.OrderItems.Select(i => i.ProductId).ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();
        
        var insufficientStock = order.OrderItems
            .Where(i =>
            {
                var product = products.First(p => p.Id == i.ProductId);
                return product.AvailableStock < i.Quantity;
            })
            .Select(i => $"{i.ProductId} (requested {i.Quantity}, available {products.First(p => p.Id == i.ProductId).AvailableStock})")
            .ToList();
        
        if (insufficientStock.Count > 0)
            return Results.Conflict($"Insufficient stock for: [{string.Join(", ", insufficientStock)}].");

        // Req 6: only reduce stock after ALL checks pass
        foreach (var item in order.OrderItems)
        {
            var product = products.First(p => p.Id == item.ProductId);
            product.AvailableStock -= item.Quantity;
        }

        order.Status = "Confirmed";
        order.ConfirmedAtUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ssZ");

        await db.SaveChangesAsync();
        return Results.Ok(new { message = "Order confirmed." });
    }
    
    // POST cancel order
    private static async Task<IResult> CancelOrder(int id, AppDbContext db)
    {
        var order = await db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return Results.NotFound($"Order {id} not found.");

        if (order.Status == "Cancelled")
            return Results.Conflict("Order is already cancelled.");

        if (order.Status == "Confirmed")
        {
            var productIds = order.OrderItems.Select(i => i.ProductId).ToList();
            var products = await db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            foreach (var item in order.OrderItems)
            {
                var product = products.First(p => p.Id == item.ProductId);
                product.AvailableStock += item.Quantity;
            }
        }

        order.Status = "Cancelled";
        order.CancelledAtUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ssZ");

        await db.SaveChangesAsync();
        return Results.Ok(new { message = "Order cancelled." });
    }
    
    
    // Map DATA to our DTO instead of base object
    private static OrderDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        CustomerEmail = order.CustomerEmail,
        Status = order.Status,
        CreatedAtUtc = DateTime.Parse(order.CreatedAtUtc),
        ConfirmedAtUtc = order.ConfirmedAtUtc is null ? null : DateTime.Parse(order.ConfirmedAtUtc),
        CancelledAtUtc = order.CancelledAtUtc is null ? null : DateTime.Parse(order.CancelledAtUtc),
        TotalAmount = (decimal)order.TotalAmount,
        Items = order.OrderItems.Select(i => new OrderItemDto
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            UnitPrice = (decimal)i.UnitPrice,
            Quantity = i.Quantity,
            LineTotal = (decimal)i.LineTotal
        }).ToList()
    };
}