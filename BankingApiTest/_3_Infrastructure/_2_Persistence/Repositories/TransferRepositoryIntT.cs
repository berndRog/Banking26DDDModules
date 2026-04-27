using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._3_Infrastructure._2_Persistence;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._3_Infrastructure._2_Persistence.Repositories;
public sealed class TransferRepositoryIntT : TestBaseIntegration {
   
   [Fact]
   public async Task FindByIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ITransferRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<Seed>();

      // Arrange
      var transfer = seed.Transfer1();
      repository.Add(transfer);
      await unitOfWork.SaveAllChangesAsync("Add transfer", ct);
      unitOfWork.ClearChangeTracker();

      var transferId = transfer.Id;
      var accountId = transfer.DebitAccountId;
      
      // Act
      var actual = await repository.FindByIdAsync(transferId, ct);

      // Assert
      NotNull(actual);
      Equal(transfer.Id, actual.Id);
      Equal(transfer.DebitAccountId, actual.DebitAccountId);
      Equal(transfer.CreditAccountIbanVo, actual.CreditAccountIbanVo);
      Equal(transfer.AmountVo, actual.AmountVo);
      Equal(transfer.Purpose, actual.Purpose);
      
   }
   
   [Fact]
   public async Task SelectAsync_retSelectTransfersByAccountIdAsync_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      using var scope = Root.CreateDefaultScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<Seed>();

      // Arrange
      dbContext.Customers.AddRange(seed.Customers);
      await unitOfWork.SaveAllChangesAsync("Add customers", ct);
      dbContext.ChangeTracker.Clear();

      // Act
      var customers = await repository.SelectAllAsync(ct);
      
      // Assert
      var actualIds = customers.Select(c => c.Id).OrderBy(id => id).ToList();
      var expectedIds = seed.Customers.Select(c => c.Id).OrderBy(id => id).ToList();
      Equal(6, actualIds.Count);
      Equal(expectedIds, actualIds);
   }
   
   [Fact]
   public async Task Add_transfer_ok() {
      
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ITransferRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var transfer = seed.Transfer1();

      // Act
      repository.Add(transfer);
      await unitOfWork.SaveAllChangesAsync("Add a transfer", ct);

      // Assert
      var actual = await repository.FindByIdAsync(transfer.Id, ct);
      NotNull(actual);
      Equal(transfer.Id, actual.Id);
      Equal(transfer.Purpose, actual.Purpose);
      Equal(transfer.AmountVo, actual.AmountVo);
      
   }

}