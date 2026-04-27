using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

public class TransferUseCases(
   TransferUcSendMoney transferUcSendMoney
   //TransferUcReverse transferUcReverse
) : ITransferUseCases {
   
   public Task<Result<TransferDto>> SendMoneyAsync(
      SendMoneyDto sendMoneyDto, 
      CancellationToken ct = default
   ) => transferUcSendMoney.ExecuteAsync(sendMoneyDto, ct);

   // public Task<Result<TransferDto>> ReverseMoneyAsync(
   //    Guid fromAccountId,
   //    Guid transferId, 
   //    string purpose,
   //    CancellationToken ct = default
   // ) => transferUcReverse.ExecuteAsync(fromAccountId, transferId, purpose, ct);

   
}