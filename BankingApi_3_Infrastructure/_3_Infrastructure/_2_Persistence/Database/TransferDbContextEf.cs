using System.Runtime.CompilerServices;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Entities;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Database;

internal sealed class TransferDbContextEf(
   AppDbContext db
) : ITransferDbContext {
   
   public IQueryable<Transfer> Transfers => db.Set<Transfer>();
 
   public void Add(Transfer transfer) 
      => db.Set<Transfer>().Add(transfer);
   public void AddRange(IEnumerable<Transfer> transfers)  
      => db.Set<Transfer>().AddRange(transfers);
 
}