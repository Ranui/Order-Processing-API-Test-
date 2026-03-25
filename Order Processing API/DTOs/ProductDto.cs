namespace Order_Processing_API.DTOs;

public class ProductDto
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public double UnitPrice { get; set; }

    public int AvailableStock { get; set; }
}