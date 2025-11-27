namespace Orders.Api.Domain;

public class Order
{
    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public OrderStatus Status { get; private set; }

    public decimal TotalAmount { get; private set; }

    // Navegación
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { } // EF

    public Order(Guid customerId, IEnumerable<(Guid productId, string productName, decimal unitPrice, int quantity)> items)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required.", nameof(customerId));

        if (items is null || !items.Any())
            throw new ArgumentException("At least one item is required.", nameof(items));

        Id = Guid.NewGuid();
        CustomerId = customerId;
        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.Created;

        foreach (var item in items)
        {
            var orderItem = new OrderItem(
                Id,
                item.productId,
                item.productName,
                item.unitPrice,
                item.quantity);

            _items.Add(orderItem);
        }

        TotalAmount = _items.Sum(i => i.Total);
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Created)
            throw new InvalidOperationException("Only created orders can be confirmed.");

        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Canceled)
            return;

        Status = OrderStatus.Canceled;
    }
}