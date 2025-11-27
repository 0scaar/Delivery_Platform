namespace Orders.Api.Contracts.Responses;

public record SalesPerDayResponse(
    DateOnly Date,
    int OrdersCount,
    decimal TotalSales);