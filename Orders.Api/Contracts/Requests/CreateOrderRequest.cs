namespace Orders.Api.Contracts.Requests;

public record CreateOrderRequest(
    Guid CustomerId,
    List<CreateOrderItemRequest> Items);

public record CreateOrderItemRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity);