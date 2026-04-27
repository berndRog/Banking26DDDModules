// using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
// using BankingApi._2_Core.Customers._1_Ports.Outbound;
// using BankingApi._2_Core.Payments._1_Ports.Outbound;
// using BankingApi._2_Core.Payments._2_Application.UseCases;
// using BankingApiTest.TestInfrastructure;
// using Microsoft.Extensions.DependencyInjection;
// namespace BankingApiTest._2_Core.Core.Application.UseCases;
//
// public sealed class TransferUcReverseIntT : TestBaseIntegration {
//    private readonly TestSeed _seed = new();
//    
//    [Fact]
//    public async Task Create_transfer_ok() {
//       using var scope = Root.CreateDefaultScope();
//       var ct = CancellationToken.None;
//       var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
//       var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
//       var transferRepository = scope.ServiceProvider.GetRequiredService<ITransferRepository>();
//       var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
//       var sut = scope.ServiceProvider.GetRequiredService<TransferUcReverse>();
//       
//       // Arrange
//       var customer = _seed.Customer1();
//       // fill datbase with customer
//       customerRepository.Add(customer);
//       var account = _seed.Account1();
//       accountRepository.Add(account);
//       await unitOfWork.SaveAllChangesAsync("Seeding data", ct);
//       unitOfWork.ClearChangeTracker();
//       var transfer = _seed.Transfer1();
//       
//       // // Act
//       // var result = await sut.ExecuteAsync(
//       //     fromAccountId: account.Id,
//       //     toName: transfer.ToName,
//       //     toIbanString: transfer.ToIbanVo.Value,
//       //     purpose: transfer.Purpose,
//       //     amountDecimal: transfer.AmountVo.Amount,
//       //     currencyInt: (int) transfer.AmountVo.Currency,
//       //     id: transfer.Id.ToString(),
//       //     ct: ct
//       //     );
//       // unitOfWork.ClearChangeTracker();
//       
//       // Assert
//       var actual = await accountRepository.FindByIdAsync(account.Id, ct);
//       NotNull(actual);
//       Equal(account.Id, actual!.Id);
//       Equal(account.IbanVo, actual.IbanVo);
//       Equal(account.BalanceVo, actual.BalanceVo);
//    }
//    
// }