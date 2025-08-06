using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderMediatR.Application.Features.Orders.CancelOrder;
using OrderMediatR.Application.Features.Orders.GetOrder;

namespace OrderMediatR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtém um pedido específico por ID
    /// </summary>
    /// <param name="id">ID do pedido</param>
    /// <returns>Dados completos do pedido</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetOrderQueryResponse>> GetOrder(Guid id)
    {
        var query = new GetOrderQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Cancela um pedido
    /// </summary>
    /// <param name="id">ID do pedido a ser cancelado</param>
    /// <param name="request">Dados do cancelamento (motivo)</param>
    /// <returns>Dados do pedido cancelado</returns>
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<CancelOrderCommandResponse>> CancelOrder(Guid id, [FromBody] CancelOrderCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);
        return Ok(result);
    }
}