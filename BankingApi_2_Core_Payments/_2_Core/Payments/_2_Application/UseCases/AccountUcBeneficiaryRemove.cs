using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks.Utils;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

public sealed class AccountUcBeneficiaryRemove(
   IAccountRepository accountRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<AccountUcBeneficiaryRemove> logger
) {

   public async Task<Result> ExecuteAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   ) {
      
      var account = await accountRepository.FindByIdAsync(accountId, ct);
      if (account is null) 
         return Result.Failure(BeneficiaryErrors.AccountNotFound);
      
      // Domain operation
      account.RemoveBeneficiary(beneficiaryId, clock.UtcNow);
      
      // Persistence with Unit of Work
      var savedRows = await unitOfWork.SaveAllChangesAsync("Remove beneficiary", ct);

      logger.LogInformation("Beneficiary removed {id}, saedRow {rows})", 
         beneficiaryId.To8(), savedRows);
      
      return Result.Success();
   }
}
