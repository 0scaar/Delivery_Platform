using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Contracts.Requests;
using Orders.Api.Contracts.Responses;
using Orders.Api.Domain;
using Orders.Api.Events;
using Orders.Api.Infrastructure;
using Orders.Api.Messaging;

namespace Orders.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _context;
    private readonly IEventPublisher _eventPublisher;
    private readonly string _exchange;
    
    public OrdersController(
        OrdersDbContext context,
        IEventPublisher eventPublisher,
        IConfiguration configuration)
    {
        _context = context;
        _eventPublisher = eventPublisher;
        _exchange = configuration.GetValue<string>("RabbitMQ:Exchange") ?? "orders.exchange";
    }

    // GET: api/v1/orders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAll()
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var result = orders.Select(MapToResponse).ToList();

        return Ok(result);
    }

    // GET: api/v1/orders/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return NotFound();

        return Ok(MapToResponse(order));
    }

    // POST: api/v1/orders
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create([FromBody] CreateOrderRequest request)
    {
        var items = request.Items.Select(i =>
            (i.ProductId, i.ProductName, i.UnitPrice, i.Quantity));

        var order = new Order(request.CustomerId, items);

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        
        // Publicar evento OrderCreated
        var @event = new OrderCreatedEvent(
            order.Id,
            order.CustomerId,
            order.TotalAmount,
            order.CreatedAt);

        await _eventPublisher.PublishAsync(_exchange, "order.created", @event);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, MapToResponse(order));
    }

    // PUT: api/v1/orders/{id}/confirm
    [HttpPut("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null)
            return NotFound();

        order.Confirm();
        await _context.SaveChangesAsync();
        
        // Publicar evento OrderConfirmed
        var @event = new OrderConfirmedEvent(
            order.Id,
            order.CustomerId,
            order.TotalAmount,
            DateTime.UtcNow);

        await _eventPublisher.PublishAsync(_exchange, "order.confirmed", @event);

        return NoContent();
    }

    // PUT: api/v1/orders/{id}/cancel
    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order is null)
            return NotFound();

        order.Cancel();
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    // GET: api/v1/orders/report/sales-per-day
    [HttpGet("report/sales-per-day")]
    public async Task<ActionResult<IEnumerable<SalesPerDayResponse>>> GetSalesPerDay(
        [FromQuery] DateOnly start,
        [FromQuery] DateOnly end)
    {
        var startDateTime = start.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endDateTime = end.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var query = await _context.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= startDateTime && o.CreatedAt <= endDateTime)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new SalesPerDayResponse(
                DateOnly.FromDateTime(g.Key),
                g.Count(),
                g.Sum(o => o.TotalAmount)))
            .OrderBy(r => r.Date)
            .ToListAsync();

        return Ok(query);
    }

    private static OrderResponse MapToResponse(Order order)
        => new(
            order.Id,
            order.CustomerId,
            order.CreatedAt,
            order.Status,
            order.TotalAmount,
            order.Items
                .Select(i => new OrderItemResponse(
                    i.Id,
                    i.ProductId,
                    i.ProductName,
                    i.UnitPrice,
                    i.Quantity,
                    i.Total))
                .ToList());
}
