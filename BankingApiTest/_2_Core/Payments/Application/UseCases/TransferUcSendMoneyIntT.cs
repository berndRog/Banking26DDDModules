using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.UseCases;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using BankingApi._3_Infrastructure._2_Persistence;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Core.Application.UseCases;

public sealed class TransferUcSendMoneyIntT : TestBaseIntegration {

   [Fact]
   public async Task SendMoney_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = CancellationToken.None;
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
      var transferRepository = scope.ServiceProvider.GetRequiredService<ITransferRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      var sut = scope.ServiceProvider.GetRequiredService<TransferUcSendMoney>();

      // Arrange
      var customer = seed.Customer1();
      // fill datbase with customer
      customerRepository.Add(customer);

      var debitAccount = seed.Account1();
      var beneficiary = seed.Beneficiary1();
      debitAccount.AddBeneficiary(beneficiary, seed.Clock.UtcNow);
      accountRepository.Add(debitAccount);

      accountRepository.AddRange([
         seed.Account2(), seed.Account3(),
         seed.Account4(), seed.Account5(), seed.Account6()
      ]);

      await unitOfWork.SaveAllChangesAsync("Seeding data", ct);
      unitOfWork.ClearChangeTracker();

      var transfer = seed.Transfer1();

      var sendMoneyDto = new SendMoneyDto(
         Id: transfer.Id,
         DebitAccountId: debitAccount.Id,
         BeneficiaryId: beneficiary.Id,
         Purpose: transfer.Purpose,
         Amount: transfer.AmountVo.Amount,
         Currency: (int)transfer.AmountVo.Currency,
         BookedAt: transfer.BookedAt,
         DebitId: transfer.DebitTransactionId.ToString(),
         CreditId: transfer.CreditTransactionId.ToString()
      );

      // Act
      var result = await sut.ExecuteAsync(
         sendMoneyDto,
         ct: ct
      );
      unitOfWork.ClearChangeTracker();

      // Assert
      True(result.IsSuccess);
      var transferDto = result.Value;
      
      var debitAccountId = transferDto.DebitAccountId;
      var debitId = transferDto.DebitTransactionId;
      var creditAccountIbanVo = IbanVo.Create(transferDto.CreditAccountIban).GetValueOrThrow();
      var creditId = transferDto.CreditTransactionId;

      var actualTransfer = await transferRepository.FindByIdAsync(transfer.Id, ct);

      var actualCreditAccount = await accountRepository.FindAccountByIdWithTransactionByIdAsync(
            accountId: debitAccountId, 
            transactionId: debitId, 
            ct: ct);
      var actualDebitAccount = await accountRepository.FindAccountByIbanWithTransactionByIdAsync(
            accountIbanVo: creditAccountIbanVo, 
            transactionId: creditId,   
            ct: ct);
      NotNull(actualTransfer);
      NotNull(actualCreditAccount);
      NotNull(actualDebitAccount);
      Equal(transfer.Id, actualTransfer!.Id);
      Equal(transfer.DebitAccountId, actualTransfer.DebitAccountId);
      Equal(transfer.CreditAccountIbanVo, actualTransfer.CreditAccountIbanVo);
      Equal(transfer.Purpose, actualTransfer.Purpose);
      Equal(transfer.AmountVo.Amount, actualTransfer.AmountVo.Amount);
      Equal(transfer.AmountVo.Currency, actualTransfer.AmountVo.Currency);
      Equal(transfer.DebitTransactionId, actualTransfer.DebitTransactionId);
      Equal(transfer.CreditTransactionId, actualTransfer.CreditTransactionId);
      
      
   }
}
//    [Fact]
//    public async Task SendMoney_with() {
//       // Arrange
//       var fromAccount = _seed.Account1;
//       var beneficiary =  _seed.Beneficiary1;
//       var transfer = _seed.Transfer1;
//
//       // create fromAccount for Customer1 in database
//       var accountId = await CreateAccountForOwner(_seed.Customer1, fromAccount);
//       var account = await _accountsRepository.FindWithBeneficiariesByIdAsync(accountId, _ct);
//       NotNull(account);
//       // add beneficiary to account
//       account!.AddBeneficiary(beneficiary.Name, beneficiary.Iban, beneficiary.Id.ToString());
//       await _unitOfWork.SaveAllChangesAsync("AddBeneficiary", _ct);
//       // create toAccount as receiver
//       _accountsRepository.Add(_seed.Account6);
//       // unit of work, save changes to database
//       await _unitOfWork.SaveAllChangesAsync("Add other accounts", _ct);
//       
//       // Act
//       // create beneficiary for account in database
//       var sendMoneyCmd = new SendMoneyCmd(
//          Id: transfer.Id.ToString(),
//          FromAccountId: account!.Id,
//          BeneficiaryId: beneficiary.Id,
//          Amount: transfer.Amount,
//          Purpose: transfer.Purpose,
//          IdempotencyKey: Guid.NewGuid().ToString()
//       );
//       
//       var result = await _sut.ExecuteAsync(sendMoneyCmd,_ct);
//       True(result.IsSuccess);
//       _dbContext!.ChangeTracker.Clear();
//       
//       // Assert
//       var actualTransfer = await _transferRepository.FindWithTransactionsByIdAsync(transfer.Id, _ct);
//       _unitOfWork.LogChangeTracker("Load transfer with transactions");
//       
//       NotNull(actualTransfer);
//       Equal(transfer.FromAccountId, actualTransfer.FromAccountId);
//       Equal(transfer.Amount, actualTransfer!.Amount);
//       Equal(transfer.Purpose, actualTransfer.Purpose);
//       // var actual = actualAccount!.Beneficiaries
//       //    .FirstOrDefault(b => b.Id == beneficiary.Id);
//       // NotNull(actual);
//       // Equal(beneficiary.Name, actual!.Name);
//       // Equal(beneficiary.Iban, actual.Iban); 
//    }
//
//    //--- Helpers ---
//    private async Task<Guid> CreateAccountForOwner(Customer owner, Account account) {
//       // create account in database
//       var resultAccount = await _accountUcCreate.ExecuteAsync(
//          customerId: owner.Id,
//          iban: account.Iban,
//          balance: account.Balance,
//          id: account.Id.ToString(),
//          ct: _ct
//       );
//       True(resultAccount.IsSuccess);
//       var accountId = resultAccount.Value;
//       return accountId;
//    }
// }