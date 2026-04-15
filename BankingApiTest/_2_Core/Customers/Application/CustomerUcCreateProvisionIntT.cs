using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Customers.Application;

public sealed class CustomerUcCreateProvisionIntT : TestBaseIntegration {
   
   public CustomerUcCreateProvisionIntT() {
      DbMode = DbMode.FileUnique;
      DbName = "CustomerUcCreateProvisionIntTest";
      SensitiveDataLogging = true;
   }
   
   [Fact]
   public async Task CustomerUcCreateProvison_ok() {
      
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var identity = scope.ServiceProvider.GetRequiredService<IIdentityGateway>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      var sut = scope.ServiceProvider.GetRequiredService<CustomerUcCreateProvision>();
      
      // Test Onwer
      var customer = seed.CustomerRegister();
      var customerDto = customer.ToCustomerDto();
      
      // Act
      var result = await sut.ExecuteAsync(customerDto, ct);
      unitOfWork.ClearChangeTracker();
      
      // Assert
      True(result.IsSuccess);
      var customerId = result.Value.Id;
      NotEqual(Guid.Empty, customerId);

      var actual = await customerRepository.FindByIdAsync(customerId, ct);
      NotNull(actual);

      Equal(customerId, actual.Id);
      Equal(identity.Subject, actual.Subject);
      Equal(identity.Username, actual.EmailVo.Value);
      Equal(identity.CreatedAt, actual.CreatedAt);
   }

   [Fact]
   public async Task CustomerUcCreateProvision_second_call_is_idempotent() {
      var ct = TestContext.Current.CancellationToken;
      using var scope = Root.CreateDefaultScope();
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var identity = scope.ServiceProvider.GetRequiredService<IIdentityGateway>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      var sut = scope.ServiceProvider.GetRequiredService<CustomerUcCreateProvision>();

      var customerDto = seed.CustomerRegister().ToCustomerDto();

      var first = await sut.ExecuteAsync(customerDto, ct);
      var second = await sut.ExecuteAsync(customerDto, ct);

      if (first.IsFailure)
         Fail($"First provision call failed: {first.Error}");
      if (second.IsFailure)
         Fail($"Second provision call failed: {second.Error}");

      var firstProvision = first.Value;
      var secondProvision = second.Value;

      False(secondProvision.WasCreated);
      Equal(firstProvision.Id, secondProvision.Id);
   }
}
