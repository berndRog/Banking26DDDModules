using System.Runtime.CompilerServices;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Repositories;

internal sealed class TransferRepositoryEf(
   ITransferDbContext transferDbContext
) : ITransferRepository {
   
   public async Task<Transfer?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   ) => await transferDbContext.Transfers
         .Where(t => t.Id == id )   
         .SingleOrDefaultAsync(ct);

   public async Task<IEnumerable<Transfer?>> SelectTransfersByAccountIdAsync(
      Guid accountId,
      CancellationToken ct = default
   ) => await transferDbContext.Transfers
      .Where(t => t.DebitAccountId == accountId)   
      .ToListAsync(ct);
   
   public void Add(Transfer transfer) 
      => transferDbContext.Add(transfer);
   
   public void AddRange(IEnumerable<Transfer> transfers) 
      => transferDbContext.AddRange(transfers);
}
