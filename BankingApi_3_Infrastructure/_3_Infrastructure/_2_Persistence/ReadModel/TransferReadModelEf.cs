using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.ReadModel;

internal sealed class TransferReadModelEf(
   ITransferDbContext transferDbContext
) : ITransferReadModel {
   
   public async Task<Result<TransferDto>> FindTransferByAccountIdAndTransferIdAsync(
      Guid accountId, 
      Guid transferId, 
      CancellationToken ct = default
   ) {
      // 1. Fetch the specific transfer ensuring it belongs to the provided AccountId.
      // We use AsNoTracking for read-only performance and project directly to a DTO.      
      var transferDto = await transferDbContext.Transfers
         .AsNoTracking()
         .Where(t => t.DebitAccountId == accountId && t.Id == transferId)
         .Select(t => t.ToTransferDto())
         .SingleOrDefaultAsync(ct);
      
      // 2a. If no record matches both IDs, we return a NotFound failure.
      // 2b. Return success with the projected data.      
      return transferDto is null
         ? Result<TransferDto>.Failure(AccountErrors.NotFound)
         : Result<TransferDto>.Success(transferDto);
   }

   public async Task<Result<IReadOnlyList<TransferDto>>> SelectTransfersByAccountIdAsync(
      Guid accountId,
      CancellationToken ct = default
   ) {
      var transferDtos = await transferDbContext.Transfers
         .AsNoTracking()
         .Where(t => t.DebitAccountId == accountId)
         .Select(t => t.ToTransferDto())
         .ToListAsync(ct);
      return Result<IReadOnlyList<TransferDto>>.Success(transferDtos.AsReadOnly());
   }
}
