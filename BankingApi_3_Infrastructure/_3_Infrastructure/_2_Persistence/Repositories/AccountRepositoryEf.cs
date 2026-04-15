using System.Runtime.CompilerServices;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Repositories;

internal sealed class AccountRepositoryEf(
   IAccountDbContext dbContext
) : IAccountRepository {
   #region --- Aggregate root: Account ------------------------------------------------------
   
   // Loads a single account by its primary key (Id) 
   public async Task<Account?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   ) => await dbContext.Accounts
      .FirstOrDefaultAsync(a => a.Id == id, ct);

   // Loads a single account by its unique IBAN Value Object.
   public async Task<Account?> FindByIbanAsync(
      IbanVo ibanVo,
      CancellationToken ct = default
   ) => await dbContext.Accounts
      .FirstOrDefaultAsync(a => a.IbanVo == ibanVo, ct);

   // Efficiently checks if at least one account exists for a specific customer.
   public async Task<bool> ExistsByCustomerIdAsync(
      Guid customerId,
      CancellationToken ct = default
   ) => await dbContext.Accounts
      .Where(a => a.CustomerId == customerId && a.DeactivatedAt == null)
      .AnyAsync(ct);

   // Retrieves all accounts associated with a customer ID.
   public async Task<IReadOnlyList<Account>> SelelctByCustomerIdAsync(
      Guid customerId,
      CancellationToken ct = default
   ) => await dbContext.Accounts
      .Where(a => a.CustomerId == customerId)
      .ToListAsync(ct);

   // Mark Account entity as added in the tracker
   public void Add(Account account)
      => dbContext.Add(account);

   // Mark multiple Account as added to the tracker
   public void AddRange(IEnumerable<Account> accounts)
      => dbContext.AddRange(accounts);

   // Mark Account as modified in the tracker
   public void Update(Account account)
      => dbContext.Update(account);
   #endregion

   #region --- Child entity: Benficiary -----------------------------------------------------
   // Loads the Account Aggregate Root and all its Beneficiary child entities.
   public async Task<Account?> FindAccountByIdWithBeneficiariesAsync(
      Guid accountId,
      CancellationToken ct = default
   ) => await dbContext.Accounts
      .Include(a => a.Beneficiaries)
      .FirstOrDefaultAsync(a => a.Id == accountId, ct);

   // Loads the Account root and attach the specific Beneficiary we want to modify.
   public async Task<Account?> FindAccountByWithBeneficiaryByIdAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   ) => await dbContext.Accounts
      .Include(a => a.Beneficiaries.Where(b => b.Id == beneficiaryId))
      .FirstOrDefaultAsync(a => a.Id == accountId, ct);

   public void Add(Beneficiary beneficiary)
      => dbContext.Add(beneficiary);

   public void AddRange(IEnumerable<Beneficiary> beneficiaries)
      => dbContext.AddRange(beneficiaries);

   public void Remove(Beneficiary beneficiary)
      => dbContext.Remove(beneficiary);
   #endregion

   #region --- Child entity: Transaction ----------------------------------------------------
   public async Task<Account?> FindAccountByIdWithTransactionByIdAsync(
      Guid accountId,
      Guid transactionId,
      CancellationToken ct = default
   ) => await dbContext.Accounts
      .Where(a => a.Id == accountId)
      .Include(a => a.Transactions.Where(b => b.Id == transactionId))
      .SingleOrDefaultAsync(ct);
   
   public async Task<Account?> FindAccountByIbanWithTransactionByIdAsync(
      IbanVo accountIbanVo,
      Guid transactionId,
      CancellationToken ct = default
   ) => await dbContext.Accounts
      .Where(a => a.IbanVo == accountIbanVo)
      .Include(a => a.Transactions.Where(b => b.Id == transactionId))
      .SingleOrDefaultAsync(ct);

   public async Task<Account?> FindAccountByIdWithTransactionsAsync(
      Guid accountId,
      CancellationToken ct = default
   ) => await dbContext.Accounts
      .Where(a => a.Id == accountId)
      .Include(a => a.Transactions)
      .SingleOrDefaultAsync(ct);
   
   public async Task<Account?> FindAccountByIbanWithTransactionsAsync(
      IbanVo ibanVo,
      CancellationToken ct = default
   ) => await dbContext.Accounts
      .Where(a => a.IbanVo == ibanVo)
      .Include(a => a.Transactions)
      .SingleOrDefaultAsync(ct);

   public void Add(Transaction transaction)
      => dbContext.Add(transaction);

   public void AddRange(IEnumerable<Transaction> transactions)
      => dbContext.AddRange(transactions);
   #endregion
}