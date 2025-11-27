namespace Catalog.Api.Domain;

public class Product
{
    // Clave primaria
    public Guid Id { get; private set; }

    // Nombre del producto
    public string Name { get; private set; } = null!; // varchar(200)

    // Descripción opcional
    public string? Description { get; private set; }  // varchar(500)

    // Precio
    public decimal Price { get; private set; }        // decimal(18,2)

    // Stock disponible
    public int StockQuantity { get; private set; }

    // Activo o no en el catálogo
    public bool IsActive { get; private set; }

    // Fecha de creación (para auditoría)
    public DateTime CreatedAt { get; private set; }

    // Constructor requerido por EF
    private Product() { }

    public Product(string name, decimal price, int stockQuantity, string? description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Price = price;
        StockQuantity = stockQuantity;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    // Métodos de dominio (para no exponer setters públicos)
    public void Update(string name, decimal price, int stockQuantity, string? description)
    {
        Name = name;
        Price = price;
        StockQuantity = stockQuantity;
        Description = description;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
        if (StockQuantity < quantity) throw new InvalidOperationException("Insufficient stock.");

        StockQuantity -= quantity;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
        StockQuantity += quantity;
    }
}