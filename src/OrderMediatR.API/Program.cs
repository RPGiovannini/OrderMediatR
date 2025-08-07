using FluentValidation;
using MediatR;
using OrderMediatR.Application.Behaviors;
using OrderMediatR.Application.Features.Customers.CreateCustomer;
using OrderMediatR.Infra.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Infrastructure
builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(OrderMediatR.Application.Features.Customers.CreateCustomer.CreateCustomerCommand).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateCustomerCommandValidator).Assembly);

// Pipeline Behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "OrderMediatR API",
        Version = "v1",
        Description = "API para gerenciamento de pedidos usando MediatR e Clean Architecture"
    });

    // Resolver conflitos de nomes duplicados
    c.CustomSchemaIds(type =>
    {
        var name = type.Name;
        if (type.Namespace != null)
        {
            var parts = type.Namespace.Split('.');
            if (parts.Length > 2)
            {
                // Usa as últimas 2 partes do namespace + nome da classe
                return $"{parts[^2]}{parts[^1]}{name}";
            }
        }
        return name;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
