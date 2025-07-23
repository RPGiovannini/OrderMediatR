using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderMediatR.Application.Features.Customers.CreateCustomer;

namespace OrderMediatR.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
        {
            try
            {
                var customerId = await _mediator.Send(command);

                return CreatedAtAction(nameof(GetById), new { id = customerId }, new { id = customerId });
            }
            catch (FluentValidation.ValidationException ex)
            {
                var errors = ex.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage });
                return BadRequest(new { Errors = errors });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // Implementar query para buscar cliente
            return Ok(new { Message = "Cliente encontrado", Id = id });
        }
    }
}