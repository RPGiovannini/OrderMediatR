using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderMediatR.Application.Features.Products.CreateProduct;
using OrderMediatR.Application.Features.Products.GetProducts;

namespace OrderMediatR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtém uma lista paginada de produtos
    /// </summary>
    /// <param name="searchTerm">Termo de busca (nome, descrição, SKU)</param>
    /// <param name="category">Filtro por categoria</param>
    /// <param name="brand">Filtro por marca</param>
    /// <param name="minPrice">Preço mínimo</param>
    /// <param name="maxPrice">Preço máximo</param>
    /// <param name="isAvailable">Filtro por disponibilidade</param>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 10)</param>
    /// <param name="sortBy">Campo para ordenação (name, price, category, createdAt)</param>
    /// <param name="isDescending">Ordenação decrescente (padrão: false)</param>
    /// <returns>Lista paginada de produtos</returns>
    [HttpGet]
    public async Task<ActionResult<GetProductsQueryResponse>> GetProducts(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? category = null,
        [FromQuery] string? brand = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? isAvailable = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool isDescending = false)
    {
        var query = new GetProductsQuery
        {
            SearchTerm = searchTerm,
            Category = category,
            Brand = brand,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            IsAvailable = isAvailable,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            IsDescending = isDescending
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Cria um novo produto
    /// </summary>
    /// <param name="request">Dados do produto a ser criado</param>
    /// <returns>Dados do produto criado</returns>
    [HttpPost]
    public async Task<ActionResult<CreateProductCommandResponse>> CreateProduct([FromBody] CreateProductCommand request)
    {
        var result = await _mediator.Send(request);
        return CreatedAtAction("GetProduct", new { id = result.Id }, result);
    }
}