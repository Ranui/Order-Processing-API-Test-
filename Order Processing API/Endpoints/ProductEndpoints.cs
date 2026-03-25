using Microsoft.EntityFrameworkCore;
using Order_Processing_API.DTOs;
using Order_Processing_API.Models;

namespace Order_Processing_API.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        // Get All
        app.MapGet("/products", async (AppDbContext db) =>
        {
            var products = await db.Products.ToListAsync();
            return Results.Ok(products.Select(MapProductDto));
        });
        
        
        // Get by ID
        app.MapGet("/products/{id}", async (string id, AppDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            return product is null
                ? Results.NotFound()
                : Results.Ok(MapProductDto(product));
        });
        
        // PUT
        app.MapPut("/products/{id}", async (string id, ProductDto dto, AppDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return Results.NotFound();

            product.Name = dto.Name;
            product.UnitPrice = dto.UnitPrice;
            product.AvailableStock = dto.AvailableStock;

            await db.SaveChangesAsync();
            return Results.Ok(MapProductDto(product));
        });
    }

    // Map DATA to our DTO instead of base object (Avoids displaying unnecessary/sensitive data)
    private static ProductDto MapProductDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        UnitPrice = product.UnitPrice,
        AvailableStock = product.AvailableStock
    };
}