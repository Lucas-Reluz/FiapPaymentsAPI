namespace PaymentsAPI.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,           // Aguardando resposta do estoque
    AwaitingPayment = 1,   // Estoque reservado, aguardando pagamento
    Confirmed = 2,         // Pagamento aprovado
    Cancelled = 3          // Cancelado (estoque insuficiente ou pagamento falhou)
}
