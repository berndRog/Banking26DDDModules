using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.Payments._2_Application.Dtos;
namespace BankingApi._2_Core.Payments._1_Ports.Inbound;

public interface ITransferUseCases {
   
   Task<Result<TransferDto>> SendMoneyAsync(
      SendMoneyDto sendMoneyDto,
      CancellationToken ct = default
   );
   
   // Task<Result<TransferDto>> ReverseMoneyAsync(
   //    Guid fromAccountId,
   //    Guid transferId,
   //    string purpose,
   //    CancellationToken ct = default
   // );

   // // Add a beneficiary to an account
   // // Beneficiaries represent allowed transfer targets
   // Task<Result<BeneficiaryDto>> AddBeneficiaryAsync(
   //    Guid accountId,
   //    BeneficiaryDto beneficiaryDto,
   //    CancellationToken ct = default
   // );


}
