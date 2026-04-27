namespace BankingApi._2_Core.Payments._2_Application.Dtos;

public record TransferDto(
   Guid Id,
   Guid DebitAccountId,
   string CreditAccountIban,         // Receipient name
   string Purpose,
   decimal Amount,
   int Currency,
   Guid DebitTransactionId,
   Guid CreditTransactionId
); 
