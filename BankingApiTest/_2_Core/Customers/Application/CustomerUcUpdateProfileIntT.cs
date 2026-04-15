using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApi._2_Core.Customers._3_Domain.Enum;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Customers.Application;

public sealed class CustomerUcUpdateProfileIntT : TestBaseIntegration {

   public CustomerUcUpdateProfileIntT() {
      DbMode = DbMode.FileUnique;
      DbName = "CustomerUcUpdateProfileIntTest";
      SensitiveDataLogging = true;
   }
   
   [Fact]
   public async Task UpdateProfile_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      var customerUcCreateProvision = scope.ServiceProvider.GetRequiredService<CustomerUcCreateProvision>();
      var sut = scope.ServiceProvider.GetRequiredService<CustomerUcUpdateProfile>();

      // Arrange
      var customer = seed.CustomerRegister();
      var customerDto = customer.ToCustomerDto();
      // create provision
      var result = await customerUcCreateProvision.ExecuteAsync(customerDto, ct);
      True(result.IsSuccess);

      // Act, update profile
      var resultProfile = await sut.ExecuteAsync(customerDto, ct);
      unitOfWork.ClearChangeTracker();

      // Assert
      True(resultProfile.IsSuccess);
      var actualProfile = resultProfile.Value;
      var actual = await customerRepository.FindByIdAsync(customer.Id, ct);
      
      NotNull(actual);
      Equal(customer.Id, actual.Id);
      Equal(customer.Firstname, actual!.Firstname);
      Equal(customer.Lastname, actual.Lastname);
      Equal(customer.CompanyName, actual.CompanyName);
      Equal(customer.DisplayName, actual.DisplayName);
      Equal(customer.Subject, actual.Subject);
      Equal(CustomerStatus.Pending, actual.Status);
      Equal(customer.EmailVo, actual.EmailVo);
      Equal(customer.AddressVo, actual.AddressVo);
   }
}
