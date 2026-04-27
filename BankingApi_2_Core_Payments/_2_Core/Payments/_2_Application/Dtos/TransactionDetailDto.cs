using BankingApi._2_Core.Payments._3_Domain.Enums;
namespace BankingApi._2_Core.Payments._2_Application.Dtos;

public sealed record TransactionDetailDto(
   Guid Id,
   Guid AccountId,
   int TypeInt,
   string Purpose,
   decimal Amount,
   decimal BalanceAfter,
   int Currency,
   string OtherAccountName,
   string OtherAccountIban,
   DateTimeOffset BookedAt,
   Guid? transferId
);