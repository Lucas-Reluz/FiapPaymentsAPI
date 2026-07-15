namespace PaymentsAPI.Domain.Enums;

public enum PaymentStatus
{
    Processing = 0,    // Processando pagamento
    Completed = 1,     // Pagamento aprovado
    Failed = 2         // Pagamento recusado
}
