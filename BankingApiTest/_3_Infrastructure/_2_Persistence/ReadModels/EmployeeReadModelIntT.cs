using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._2_Application.Mappings;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._2_Application.Mappings;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BankingApiTest._3_Infrastructure._2_Persistence.Repositories;
public sealed class EmployeeReadModelIntT : TestBaseIntegration {

   [Fact]
   public async Task FindByIdAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<IEmployeeRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<IEmployeeReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var employees = seed.Employees;
      var employee = employees[0];
      var employeeDto = employee.ToEmployeeDto();
      
      repository.AddRange(employees);
      await unitOfWork.SaveAllChangesAsync("Employees inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByIdAsync(employee.Id, ct);
      
      // Assert
      True(result.IsSuccess);
      var actualDto = result.Value;
      NotNull(actualDto);
      Equal(employeeDto.Id, actualDto.Id);
      Equal(employeeDto.Firstname, actualDto.Firstname);
      Equal(employeeDto.Lastname, actualDto.Lastname);
      Equal(employeeDto.Email, actualDto.Email);
      Equal(employeeDto.Phone, actualDto.Phone);
      Equal(employeeDto.PersonnelNumber, actualDto.PersonnelNumber);
      Equal(employeeDto.AdminRightsInt, actualDto.AdminRightsInt);
   }
   
   [Fact]
   public async Task FindByEmailAsync_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ICustomerReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var customers = seed.Customers;
      var customer1 = customers[0];
      repository.AddRange(customers);
      await unitOfWork.SaveAllChangesAsync("Customers inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.FindByEmailAsync(customer1.EmailVo.Value, ct);
      
      // Assert
      True(result.IsSuccess);
      var actual = result.Value;
      NotNull(actual);
      Equal(customer1.Id, actual.Id);
      Equal(customer1.Firstname, actual.Firstname);
      Equal(customer1.Lastname, actual.Lastname);
      Equal(customer1.CompanyName, actual.CompanyName);
      Equal((int) customer1.Status, actual.StatusInt);
      Equal(customer1.EmailVo.Value, actual.Email);
      Equal(customer1.AddressVo.ToAddressDto(), actual.AddressDto);
   }
   
   [Fact]
   public async Task SelectByNameAsync_loads_Mustermann_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ICustomerReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var customers = seed.Customers;
      var customer1 = customers[0];
      var customer2 = customers[1];
      repository.AddRange(customers);
      await unitOfWork.SaveAllChangesAsync("Customers inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.SelectByDisplayNameAsync("Mustermann", ct);
      
      // Assert
      True(result.IsSuccess);
      var actual = result.Value.ToList();
      Equal(2, actual.Count());
      NotNull(actual);
      Equal(customer1.Id, actual[0].Id);
      Equal(customer1.Firstname, actual[0].Firstname);
      Equal(customer1.Lastname, actual[0].Lastname);
      Equal(customer1.CompanyName, actual[0].CompanyName);
      Equal((int) customer1.Status, actual[0].StatusInt);
      Equal(customer1.EmailVo.Value, actual[0].Email);
      Equal(customer1.AddressVo.ToAddressDto(), actual[0].AddressDto);
      Equal(customer2.Id, actual[1].Id);
      Equal(customer2.Firstname, actual[1].Firstname);
      Equal(customer2.Lastname, actual[1].Lastname);
      Equal(customer2.CompanyName, actual[1].CompanyName);
      Equal((int) customer2.Status, actual[1].StatusInt);
      Equal(customer2.EmailVo.Value, actual[1].Email);
      Equal(customer2.AddressVo.ToAddressDto(), actual[1].AddressDto);
   }
   [Fact]
   public async Task SelectByNameAsync_loads_CompanyName_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ICustomerReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var customers = seed.Customers;
      var customer5 = customers[4];
      repository.AddRange(customers);
      await unitOfWork.SaveAllChangesAsync("Customers inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.SelectByDisplayNameAsync("Conrad Consulting", ct);
      
      // Assert
      True(result.IsSuccess);
      var actual = result.Value.ToList();
      Single(actual);
      NotNull(actual);
      Equal(customer5.Id, actual[0].Id);
      Equal(customer5.Firstname, actual[0].Firstname);
      Equal(customer5.Lastname, actual[0].Lastname);
      Equal(customer5.CompanyName, actual[0].CompanyName);
      Equal((int) customer5.Status, actual[0].StatusInt);
      Equal(customer5.EmailVo.Value, actual[0].Email);
      Equal(customer5.AddressVo.ToAddressDto(), actual[0].AddressDto); 
   }
   [Fact]
   public async Task SelectAll_ok() {
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var repository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
      var readModel = scope.ServiceProvider.GetRequiredService<ICustomerReadModel>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();

      // Arrange
      var customers = seed.Customers;
      repository.AddRange(customers);
      await unitOfWork.SaveAllChangesAsync("Customers inserted", ct);
      unitOfWork.ClearChangeTracker();

      // Act
      var result = await readModel.SelectAllAsync( ct);
      
      // Assert
      True(result.IsSuccess);
      var actual = result.Value.ToList();
      Equal(6, actual.Count());
      NotNull(actual);
      Equals(customers, actual);
   
   }
}