using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderMediatR.Application.Interfaces;
using OrderMediatR.Domain.Context;
using OrderMediatR.Infra.BackgroundServices;
using OrderMediatR.Infra.Context;
using OrderMediatR.Infra.MessageBus;
using OrderMediatR.Infra.Repositories;

namespace OrderMediatR.Infra.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        // Contextos separados para Write e Read
        services.AddDbContext<WriteContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("WriteConnection") ?? 
                                configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContext<ReadContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("ReadConnection") ?? 
                                configuration.GetConnectionString("DefaultConnection")));

        // Manter compatibilidade com o contexto original (se necessário)
        // services.AddDbContext<OrderMediatRContext>(options =>
        //     options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // services.AddScoped<IOrderMediatRContext>(provider =>
        //     provider.GetRequiredService<OrderMediatRContext>());

        // Repositórios (usando WriteContext para operações de escrita)
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Configurar RabbitMQ
        services.Configure<RabbitMqSettings>(
            configuration.GetSection("RabbitMq"));

        // Message Bus
        services.AddSingleton<IPublisherMessageBus, RabbitMqPublisherMessageBus>();

        // OutboxProcessor
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}