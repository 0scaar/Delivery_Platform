namespace Orders.Api.Domain;

public class OrderItem
{
    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = null!; // denormalizado p/ reportes

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public decimal Total { get; private set; }

    private OrderItem() { } // EF

    public OrderItem(Guid orderId, Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
        if (unitPrice < 0) throw new ArgumentException("Price cannot be negative.");

        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
        Total = unitPrice * quantity;
    }
}