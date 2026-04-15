using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.Payments._2_Application.Dtos;
namespace BankingApi._2_Core.Payments._1_Ports.Outbound;


public interface ITransferReadModel {

   // Load a transfer by its accout id and transfer id
   Task<Result<TransferDto>> FindTransferByAccountIdAndTransferIdAsync(
      Guid accountId,
      Guid transferId,
      CancellationToken ct = default
   );
   
   
   Task<Result<IReadOnlyList<TransferDto>>> SelectTransfersByAccountIdAsync(
      Guid accountId,
      CancellationToken ct = default
   );


   
}