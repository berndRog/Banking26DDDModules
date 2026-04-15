using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._3_Domain.Entities;
namespace BankingApi._2_Core.Payments._2_Application.Mappings;

public static class TransferMappings {

   public static TransferDto ToTransferDto(this Transfer transfer) => new(
      Id: transfer.Id,
      DebitAccountId: transfer.DebitAccountId,
      CreditAccountIban: transfer.CreditAccountIbanVo.Value,
      Purpose: transfer.Purpose,
      Amount: transfer.AmountVo.Amount,
      Currency: (int) transfer.AmountVo.Currency,
      DebitTransactionId: transfer.DebitTransactionId,
      CreditTransactionId: transfer.CreditTransactionId
   );
   
}


