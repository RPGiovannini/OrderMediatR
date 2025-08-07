# OrderMediatR - Sistema de Pedidos com Outbox Pattern

Sistema de gerenciamento de pedidos implementando o padrão Outbox para sincronização assíncrona de dados entre banco de escrita e leitura.

## 🏗️ Arquitetura

### Padrões Implementados
- **CQRS** (Command Query Responsibility Segregation)
- **Outbox Pattern** para sincronização assíncrona
- **Domain Events** para comunicação entre camadas
- **Value Objects** para encapsulamento de regras de negócio
- **Repository Pattern** para abstração de acesso a dados

### Estrutura do Projeto
```
src/
├── OrderMediatR.API/           # API REST
├── OrderMediatR.Application/   # Casos de uso e validações
├── OrderMediatR.Common/        # Classes base e utilitários
├── OrderMediatR.Domain/        # Entidades e regras de negócio
├── OrderMediatR.Infra/         # Implementações de infraestrutura
└── OrderMediatR.SyncWorker/    # Worker para sincronização
```

## 🚀 Funcionalidades

### Entidades Principais
- **Customer**: Clientes com validações de email e telefone
- **Product**: Produtos com SKU e preços
- **Order**: Pedidos com itens e cálculos de valores
- **Payment**: Pagamentos associados aos pedidos

### Fluxo de Sincronização
1. **API** recebe requisição e salva no banco de escrita
2. **Domain Events** são capturados e convertidos em `OutboxEvent`
3. **OutboxProcessor** processa eventos pendentes e publica no RabbitMQ
4. **SyncWorker** consome mensagens e sincroniza no banco de leitura

## 🛠️ Tecnologias

- **.NET 8**
- **Entity Framework Core**
- **RabbitMQ** (Message Broker)
- **SQL Server**
- **MediatR** (Mediator Pattern)
- **FluentValidation**

## 📋 Pré-requisitos

- .NET 8 SDK
- SQL Server (local ou Docker)
- RabbitMQ (Docker)

## 🚀 Como Executar

### 1. Configurar Banco de Dados
```bash
# Aplicar migrações do banco de escrita
dotnet ef database update --project src/OrderMediatR.Infra --startup-project src/OrderMediatR.API --context WriteContext

# Aplicar migrações do banco de leitura
dotnet ef database update --project src/OrderMediatR.Infra --startup-project src/OrderMediatR.SyncWorker --context ReadContext
```

### 2. Iniciar RabbitMQ
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### 3. Executar Aplicações
```bash
# Terminal 1: API
dotnet run --project src/OrderMediatR.API --profile http

# Terminal 2: SyncWorker
dotnet run --project src/OrderMediatR.SyncWorker
```

### 4. Acessar API
- **Swagger UI**: http://localhost:5046/swagger
- **Health Check**: http://localhost:5046/health

## 📊 Monitoramento

### Logs
- **API**: Logs de requisições e validações
- **OutboxProcessor**: Processamento de eventos pendentes
- **SyncWorker**: Sincronização de dados

### Métricas
- Eventos processados por minuto
- Tempo de sincronização
- Taxa de erro na sincronização

## 🔧 Configuração

### Connection Strings
```json
{
  "ConnectionStrings": {
    "WriteConnection": "Server=localhost;Database=OrderMediatRWrite;User Id=sa;Password=1234;TrustServerCertificate=True",
    "ReadConnection": "Server=localhost;Database=OrderMediatRRead;User Id=sa;Password=1234;TrustServerCertificate=True"
  }
}
```

### RabbitMQ
```json
{
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  }
}
```

## 🧪 Testes

### Exemplo de Criação de Cliente
```bash
curl -X POST "http://localhost:5046/api/customers" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "João",
    "lastName": "Silva",
    "email": "joao@email.com",
    "phone": "(11) 99999-9999",
    "documentNumber": "123.456.789-00",
    "dateOfBirth": "1990-01-01"
  }'
```

## 📈 Benefícios da Arquitetura

- **Escalabilidade**: Separação entre escrita e leitura
- **Resiliência**: Outbox pattern garante entrega de eventos
- **Performance**: Queries otimizadas no banco de leitura
- **Manutenibilidade**: Código limpo e bem estruturado

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature
3. Commit suas mudanças
4. Push para a branch
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT.

