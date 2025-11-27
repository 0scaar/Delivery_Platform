using Orders.Api.Domain;

namespace Orders.Api.Contracts.Responses;

public record OrderResponse(
    Guid Id,
    Guid CustomerId,
    DateTime CreatedAt,
    OrderStatus Status,
    decimal TotalAmount,
    List<OrderItemResponse> Items);

public record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal Total);