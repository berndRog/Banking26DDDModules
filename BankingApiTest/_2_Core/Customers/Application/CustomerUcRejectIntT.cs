using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._2_Application.UseCases;
using BankingApi._2_Core.Customers._3_Domain.Enum;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Customers.Application;

public sealed class CustomerUcRejectIntT : TestBaseIntegration {

   public CustomerUcRejectIntT() {
      DbMode = DbMode.FileUnique;
      DbName = "CustomerUcRejectIntTest";
      SensitiveDataLogging = true;
   }
   
   [Fact]
   public async Task CustomerUcReject_ok() {

      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var employeeRepository = scope.ServiceProvider.GetRequiredService<IEmployeeRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      var customerUcCreateProvision = scope.ServiceProvider.GetRequiredService<CustomerUcCreateProvision>();
      var customerUcUpdateProfile = scope.ServiceProvider.GetRequiredService<CustomerUcUpdateProfile>();
      var sut = scope.ServiceProvider.GetRequiredService<CustomerUcReject>();

      var customer = seed.CustomerRegister();
      var customerDto = customer.ToCustomerDto();
      var employee = seed.Employee2();  // Walter Wagner

      // Arrange
      employeeRepository.Add(employee);
      
      // create provision
      var resultProvision = await customerUcCreateProvision.ExecuteAsync(customerDto, ct);
      True(resultProvision.IsSuccess);
      // update profile
      var resultProfile = await customerUcUpdateProfile.ExecuteAsync(customerDto, ct);
      True(resultProfile.IsSuccess);
      unitOfWork.ClearChangeTracker();
      
      // Act
      var resultReject = await sut.ExecuteAsync(
         customerId: customer.Id,
         customerRejectCode: CustomerRejectCode.ComplianceCheckFailed,
         ct: ct);
      True(resultReject.IsSuccess);   

      // Assert
      var actual = await customerRepository.FindByIdAsync(customer.Id, ct);
      NotNull(actual);
      Equal(customer.Id, actual.Id);
      Equal(customer.Firstname, actual.Firstname);
      Equal(customer.Lastname, actual.Lastname);
      Equal(customer.CompanyName, actual.CompanyName);
      Equal(customer.EmailVo, actual.EmailVo);
      Equal(customer.Subject, actual.Subject);
      Equal(CustomerRejectCode.ComplianceCheckFailed, actual.CustomerRejectCode);
      Equal(CustomerStatus.Rejected, actual.Status);
      Equal(customer.AddressVo, actual.AddressVo);
   }
}