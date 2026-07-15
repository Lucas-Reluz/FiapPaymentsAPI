using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentsAPI.Application.Commands;
using PaymentsAPI.Application.DTOs;
using PaymentsAPI.Application.Queries;

namespace PaymentsAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Criar novo pedido
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var command = new CreateOrderCommand(
                request.UserId,
                request.GameId,
                request.GameTitle,
                request.Quantity,
                request.UnitPrice
            );

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetOrderById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pedido");
            return StatusCode(500, new { message = "Erro ao criar pedido" });
        }
    }

    /// <summary>
    /// Buscar pedido por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<OrderResponse>> GetOrderById(Guid id)
    {
        try
        {
            var query = new GetOrderByIdQuery(id);
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound(new { message = "Pedido não encontrado" });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pedido {OrderId}", id);
            return StatusCode(500, new { message = "Erro ao buscar pedido" });
        }
    }

    /// <summary>
    /// Listar pedidos do usuário
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [Authorize]
    public async Task<ActionResult<OrdersListResponse>> GetUserOrders(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetUserOrdersQuery(userId, page, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar pedidos do usuário {UserId}", userId);
            return StatusCode(500, new { message = "Erro ao listar pedidos" });
        }
    }

    /// <summary>
    /// Processar pagamento de um pedido
    /// </summary>
    [HttpPost("{id:guid}/payment")]
    [Authorize]
    public async Task<ActionResult> ProcessPayment(Guid id, [FromBody] ProcessPaymentRequest request)
    {
        try
        {
            var command = new ProcessPaymentCommand(id, request.PaymentMethod);
            var success = await _mediator.Send(command);

            if (success)
                return Ok(new { message = "Pagamento processado com sucesso" });
            else
                return BadRequest(new { message = "Falha ao processar pagamento" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Pedido não encontrado" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pagamento do pedido {OrderId}", id);
            return StatusCode(500, new { message = "Erro ao processar pagamento" });
        }
    }
}

public class ProcessPaymentRequest
{
    public string PaymentMethod { get; set; } = "CreditCard";
}
