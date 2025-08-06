using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderMediatR.Application.Features.Customers.CreateCustomer;
using OrderMediatR.Application.Features.Customers.GetCustomer;
using OrderMediatR.Application.Features.Customers.GetCustomers;
using OrderMediatR.Application.Features.Customers.UpdateCustomer;

namespace OrderMediatR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtém uma lista paginada de clientes
    /// </summary>
    /// <param name="searchTerm">Termo de busca (nome, email)</param>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 10)</param>
    /// <param name="sortBy">Campo para ordenação (firstName, lastName, email, createdAt)</param>
    /// <param name="isDescending">Ordenação decrescente (padrão: false)</param>
    /// <returns>Lista paginada de clientes</returns>
    [HttpGet]
    public async Task<ActionResult<GetCustomersQueryResponse>> GetCustomers(
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool isDescending = false)
    {
        var query = new GetCustomersQuery
        {
            SearchTerm = searchTerm,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            IsDescending = isDescending
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtém um cliente específico por ID
    /// </summary>
    /// <param name="id">ID do cliente</param>
    /// <returns>Dados do cliente</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetCustomerQueryResponse>> GetCustomer(Guid id)
    {
        var query = new GetCustomerQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Cria um novo cliente
    /// </summary>
    /// <param name="request">Dados do cliente a ser criado</param>
    /// <returns>ID do cliente criado</returns>
    [HttpPost]
    public async Task<ActionResult<CreateCustomerCommandResponse>> CreateCustomer([FromBody] CreateCustomerCommand request)
    {
        var result = await _mediator.Send(request);
        return CreatedAtAction(nameof(GetCustomer), new { id = result.Id }, result);
    }

    /// <summary>
    /// Atualiza um cliente existente
    /// </summary>
    /// <param name="id">ID do cliente a ser atualizado</param>
    /// <param name="request">Dados atualizados do cliente</param>
    /// <returns>Dados do cliente atualizado</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateCustomerCommandResponse>> UpdateCustomer(Guid id, [FromBody] UpdateCustomerCommand request)
    {
        request.Id = id;
        var result = await _mediator.Send(request);
        return Ok(result);
    }
}