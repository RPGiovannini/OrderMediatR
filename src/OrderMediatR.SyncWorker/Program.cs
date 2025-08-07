using Microsoft.EntityFrameworkCore;
using OrderMediatR.Infra.Context;
using OrderMediatR.Infra.MessageBus;
using OrderMediatR.SyncWorker;
using OrderMediatR.SyncWorker.Handlers;
using OrderMediatR.SyncWorker.Interfaces;
using OrderMediatR.SyncWorker.Messages;
using OrderMediatR.SyncWorker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configurar contexto de leitura
builder.Services.AddDbContext<ReadContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar RabbitMQ
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMq"));

// Registrar Message Bus
builder.Services.AddSingleton<IConsumerMessageBus, RabbitMqConsumerMessageBus>();

// Registrar Dispatcher e Handlers
builder.Services.AddScoped<IMessageDispatcher, MessageDispatcher>();
builder.Services.AddScoped<IMessageHandler<OrderMessage>, OrderMessageHandler>();
builder.Services.AddScoped<IMessageHandler<CustomerMessage>, CustomerMessageHandler>();
builder.Services.AddScoped<IMessageHandler<ProductMessage>, ProductMessageHandler>();

// Registrar Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
