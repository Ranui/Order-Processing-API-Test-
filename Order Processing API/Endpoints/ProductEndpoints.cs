using Microsoft.EntityFrameworkCore;
using Order_Processing_API.Models;

namespace Order_Processing_API.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        app.MapGet("/products", async (AppDbContext db) =>
        {
            var products = await db.Products.ToListAsync();
            return Results.Ok(products);
        });
        
        // TODO: add PUT endpoint
    }
}