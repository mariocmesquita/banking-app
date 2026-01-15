# BankingApp

Sistema bancário com microserviços usando .NET 10, DDD, CQRS e comunicação assíncrona via Kafka. Implementa contas correntes, transferências com saga pattern e aplicação automática de tarifas.

---

## O que é

Este projeto demonstra uma arquitetura de microserviços para um sistema bancário simples:

- **API Gateway** - Ponto de entrada único com autenticação JWT
- **CheckingAccountService** - Gerencia contas correntes e movimentações
- **TransferService** - Processa transferências entre contas
- **FeeService** - Aplica tarifas automaticamente via eventos Kafka

---

## Tecnologias

**Backend:**

- .NET 10
- Dapper
- SQLite (banco por serviço)
- MediatR (CQRS)
- FluentValidation

**Mensageria:**

- Kafka + Zookeeper
- KafkaFlow

**Infraestrutura:**

- YARP (API Gateway)
- Redis (cache de idempotência)
- Docker + Docker Compose

**Segurança:**

- JWT Authentication
- BCrypt (hash de senhas)

---

## Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (opcional, se for rodar fora do Docker)

---

## Como Rodar

**1. Subir todos os serviços:**

```bash
docker-compose up -d
```

**2. Verificar status:**

```bash
docker ps
```

Você verá:

- `api-gateway` - Porta 5096
- `checking-account-service` - Porta 5001
- `transfer-service` - Porta 5002
- `fee-service` - Worker em background
- `kafka`, `zookeeper`, `redis`

**3. Parar tudo:**

```bash
docker-compose down
```

---

## Configuração

### Portas dos Serviços

| Serviço                | URL                   |
| ---------------------- | --------------------- |
| API Gateway            | http://localhost:5096 |
| CheckingAccountService | http://localhost:5001 |
| TransferService        | http://localhost:5002 |
| Kafka UI               | http://localhost:8080 |

### JWT (Autenticação)

A configuração JWT é definida em `docker-compose.yml` via variáveis de ambiente:

```yaml
environment:
  Jwt__SecretKey: "secret-key"
  Jwt__Issuer: "BankingApp.CheckingAccountService"
  Jwt__Audience: "BankingApp.Clients"
  Jwt__ExpirationMinutes: "60"
```

Todos os serviços compartilham a mesma configuração. Para alterar, edite o environment de cada serviço dentro do `docker-compose.yml`.

### Kafka Topics

- `checking-account.movement` - Movimentos criados
- `transfer.completed` - Transferências finalizadas
- `fee.applied` - Tarifas aplicadas

**Monitorar mensagens:** http://localhost:8080

---

## Como Testar

### Acessar Swagger

Cada serviço tem documentação interativa:

- **CheckingAccountService:** http://localhost:5001/swagger
- **TransferService:** http://localhost:5002/swagger

### Fluxo Básico

**1. Registrar conta:**

```bash
curl -X POST http://localhost:5096/api/checking-accounts/register \
  -H "Content-Type: application/json" \
  -d '{
    "cpf": "12345678901",
    "name": "João Silva",
    "password": "senha123"
  }'
```

Resposta:

```json
{
  "accountNumber": 1234567890,
  "name": "João Silva",
  "cpf": "12345678901"
}
```

**2. Fazer login:**

```bash
curl -X POST http://localhost:5096/api/checking-accounts/login \
  -H "Content-Type: application/json" \
  -d '{
    "cpf": "12345678901",
    "password": "senha123"
  }'
```

Resposta:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "accountNumber": 1234567890
}
```

**3. Criar movimento (depósito):**

```bash
curl -X POST http://localhost:5096/api/checking-accounts/movement \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: $(uuidgen)" \
  -d '{
    "type": "C",
    "amount": 1000.00
  }'
```

Tipos: `C` = Crédito, `D` = Débito

**4. Consultar saldo:**

```bash
curl -X GET http://localhost:5096/api/checking-accounts/balance \
  -H "Authorization: Bearer SEU_TOKEN"
```

**5. Criar transferência:**

```bash
curl -X POST http://localhost:5096/api/transfers \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: $(uuidgen)" \
  -d '{
    "destinationAccountNumber": 9876543210,
    "amount": 100.00,
  }'
```

---

## Idempotência

Todas as operações de escrita requerem o header `Idempotency-Key`:

```http
Idempotency-Key: 550e8400-e29b-41d4-a716-446655440000
```

- Mesma chave retorna resposta em cache

---

## Arquitetura

**Padrões utilizados:**

- Domain-Driven Design (DDD)
- CQRS (Command Query Responsibility Segregation)
- Saga Pattern (orquestração de transferências)
- Event-Driven Architecture
- Repository Pattern

**Fluxo de transferência com tarifa:**

1. TransferService debita origem e credita destino
2. Publica evento `TransferCompletedEvent` no Kafka
3. FeeService consome evento, calcula tarifa (R$ 2,00) e publica `FeeAppliedEvent`
4. CheckingAccountService consome evento e debita tarifa da conta origem

---

## Bancos de Dados

Cada serviço tem seu próprio SQLite:

```
src/Services/CheckingAccountService/BankingApp.CheckingAccountService.Api/checking_account.db
src/Services/TransferService/BankingApp.TransferService.Api/transfer.db
src/Services/FeeService/BankingApp.FeeService.Worker/fee.db
```

---

## Comandos Úteis

**Ver logs:**

```bash
docker logs -f checking-account-service
docker logs -f transfer-service
docker logs -f fee-service
```

**Reiniciar serviços:**

```bash
docker-compose restart
```

**Limpar tudo (inclusive volumes):**

```bash
docker-compose down -v
```

---

## Testes

```bash
# Rodar todos os testes
dotnet test

# Projeto específico
dotnet test tests/CheckingAccountService.UnitTests/
```

---
