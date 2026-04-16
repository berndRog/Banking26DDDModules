using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._2_Application.Mappings;
using BankingApi._2_Core.Employees._2_Application.UseCases;
using BankingApiTest.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Core.Employees.Application;

public sealed class EmployeeUcCreateIntT : TestBaseIntegration {
   
   public EmployeeUcCreateIntT() {
      DbMode = DbMode.FileUnique;
      DbName = "EmployeeUcCreateIntTest";
      SensitiveDataLogging = true;
   }
   
   [Fact]
   public async Task Create_Employee_ok() {
      
      using var scope = Root.CreateDefaultScope();
      var ct = TestContext.Current.CancellationToken;
      var employeeRepository = scope.ServiceProvider.GetRequiredService<IEmployeeRepository>();
      var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
      var seed = scope.ServiceProvider.GetRequiredService<TestSeed>();
      var sut = scope.ServiceProvider.GetRequiredService<EmployeeUcCreate>();

      // Arrange
      // Employee2 is used as Admin and must exists in the dataabse
      var employee2 = seed.Employee2();
      employeeRepository.Add(employee2);
      await unitOfWork.SaveAllChangesAsync("Employee2 must exist", ct);
      unitOfWork.ClearChangeTracker();
      
      var employee = seed.EmployeeRegister(); // with address
      var employeeCreateDto = employee.ToEmployeeCreateDto(); 
     
      // Act
      await sut.ExecuteAsync(
         employeeCreateDto: employeeCreateDto,
         ct
      );
      unitOfWork.ClearChangeTracker();

      // Assert
      var actualEmployee = await employeeRepository.FindByIdAsync(employee.Id, ct);
      NotNull(actualEmployee);
      Equal(employee.Id, actualEmployee.Id);
      Equal(employee.Firstname, actualEmployee.Firstname);
      Equal(employee.Lastname, actualEmployee.Lastname);
      Equal(employee.EmailVo, actualEmployee.EmailVo);
      Equal(employee.PhoneVo, actualEmployee.PhoneVo);
      Equal(employee.Subject, actualEmployee.Subject);
      
     
   }
}