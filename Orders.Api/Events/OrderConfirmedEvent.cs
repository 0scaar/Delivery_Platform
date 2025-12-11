namespace Orders.Api.Events;

public record OrderConfirmedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    DateTime ConfirmedAt);