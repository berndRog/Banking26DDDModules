// using BankingApi._2_Modules.AccountsTransfers._1_Ports.Outbound;
// using BankingApi._2_Modules.Transfers._3_Domain.Errors;
// using BankingApi._4_BuildingBlocks;
// using BankingApi._4_BuildingBlocks.Utils;
// namespace BankingApi._2_Modules.Transfers._2_Application.UseCases;
//
// public sealed class AccountUcGetTransactions(
//    IAccountRepository accountRepository,
//    ITransactionRepository transactionRepository,
//    ILogger<AccountUcGetTransactions> logger
// ) {
//
//    public async Task<Result<IReadOnlyList<Transaction>>> ExecuteAsync(
//       Guid accountId,
//       DateOnly fromDate,
//       DateOnly toDate,
//       CancellationToken ct = default
//    ) {
//
//       if (fromDate > toDate) {
//          logger.LogWarning(
//             "Invalid period for account {AccountId}: {From} > {To}",
//             accountId.To8(), fromDate, toDate
//          );
//          return Result<IReadOnlyList<Transaction>>
//             .Failure(TransactionErrors.InvalidPeriod);
//       }
//
//       var account = await accountRepository.FindByIdAsync(accountId, ct);
//       if (account is null) {
//          logger.LogWarning("Get transactions failed: account not found ({AccountId})",
//             accountId.To8());
//          return Result<IReadOnlyList<Transaction>>
//             .Failure(TransactionErrors.AccountNotFound);
//       }
//
//       var transactions = await transactionRepository
//             .SelectByAccountIdAndPeriodAsync(accountId, fromDate, toDate, ct);
//
//       logger.LogInformation(
//          "Loaded {Count} transactions for account {AccountId}",
//          transactions.Count, accountId.To8()
//       );
//
//       return Result<IReadOnlyList<Transaction>>
//          .Success(transactions);
//    }
// }
