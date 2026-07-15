# PaymentsAPI

API para gerenciar pedidos e pagamentos da plataforma.

## O que faz

- Criar pedidos de compra de jogos
- Processar pagamentos
- Controlar status dos pedidos
- Confirmar ou cancelar pedidos

## Como funciona

Quando alguem cria um pedido, a API avisa o CatalogAPI para reservar estoque. Se tiver estoque disponivel, processa o pagamento automaticamente. Quando o pagamento e aprovado, avisa o NotificationsAPI para criar uma notificacao para o usuario.

## Como rodar

1. Ter Docker rodando com PostgreSQL e RabbitMQ
2. Entrar na pasta do projeto:
   ```
   cd PaymentsAPI/src/PaymentsAPI.Api
   ```
3. Rodar o comando:
   ```
   dotnet run
   ```
4. A API vai abrir em: http://localhost:5229

## O que precisa configurar

No arquivo appsettings.json tem:
- Conexao com banco PostgreSQL (PaymentsDb)
- Configuracao do RabbitMQ
- Chave secreta do JWT

## Endpoints principais

- POST /api/orders - Criar um pedido novo
- GET /api/orders/{id} - Buscar um pedido especifico
- GET /api/orders/user/{userId} - Listar pedidos de um usuario
- GET /api/health - Ver se a API esta funcionando

## Eventos que escuta

- StockReservedEvent - Quando o estoque e reservado, processa o pagamento
- StockReservationFailedEvent - Quando nao tem estoque, cancela o pedido

## Eventos que publica

- OrderCreatedEvent - Avisa que um pedido foi criado
- OrderConfirmedEvent - Avisa que o pedido foi confirmado
- OrderCancelledEvent - Avisa que o pedido foi cancelado

## Status dos pedidos

- Pending - Pedido criado, esperando estoque
- AwaitingPayment - Estoque reservado, vai processar pagamento
- Confirmed - Pagamento aprovado
- Cancelled - Pedido cancelado (sem estoque ou erro)
