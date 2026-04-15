using System.Net;
using System.Net.Http.Json;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Employees._2_Application.Dtos;
using BankingApi._2_Core.Employees._2_Application.Mappings;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApiTest.TestController;
using BankingApiTest.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Modules.Employees.Application;

public sealed class EmployeesControllerEndToEnd : TestBaseEndToEnd {
   private TestSeed _seed = new TestSeed();

   // For teaching: keep DB so students can inspect it afterwards.
   protected override bool DeleteDatabaseOnDispose => false;

   [Fact]
   public async Task Post_EmployeesCreate_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      // Arrange
      var requestDto = new EmployeeDto(
         Id: Guid.NewGuid(),
         Firstname: "Bernd",
         Lastname: "Rogalla",
         Email: "rogalla.b@mail.local",
         Phone: "+49 (0)1234 / 5678-9123",
         PersonnelNumber: "EMP-12345",
         IsActive: true,
         AdminRightsInt: 511
      );
      var expectedEmail = EmailVo.Create(requestDto.Email).Value;
      var expectdPhone = PhoneVo.Create(requestDto.Phone).Value; 
      
      // Act
      var subject = "12345678-0000-0000-0000-000000000000"; // in real scenario, subject should come from auth token or be generated in use case
      var response = await Client.PostAsJsonAsync(
         requestUri: $"/bankingapi/v1/employees?subject={Uri.EscapeDataString(subject)}",
         value: requestDto,
         cancellationToken: ct
      );
      
     var body = await response.Content.ReadAsStringAsync(ct); // helpful for debugging
     body = body.Trim().Trim('"'); // remove quotes if response is a plain string (e.g. Id)
     Guid.TryParse(body, out var id);
     
     // Assert (HTTP)
      True( 
         id == requestDto.Id, 
         $"Expected Id {requestDto.Id} in response body, but got {id.ToString()}"
      );
      True(
         condition: response.StatusCode is HttpStatusCode.Created,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n{id}"
      );
      
      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

         // IMPORTANT: use AsNoTracking to avoid tracking artifacts
         var employee = await dbContext.Employees
            .AsNoTracking()
            .Where(o => o.Id == id)
            .SingleOrDefaultAsync(ct);
         
         NotNull(employee);
         var employeeDto = employee.ToEmployeeDto();

         // Domain-level checks
         Equal(subject, employee.Subject);
         
         Equal(requestDto.Firstname, employeeDto.Firstname);
         Equal(requestDto.Lastname, employeeDto.Lastname);
         Equal(requestDto.Email, employeeDto.Email);
         Equal(requestDto.Phone, employeeDto.Phone);
         Equal(requestDto.PersonnelNumber, employeeDto.PersonnelNumber);
         Equal(requestDto.IsActive, employeeDto.IsActive);
         Equal(requestDto.AdminRightsInt, employeeDto.AdminRightsInt);
      });
   }
   
   [Fact]
   public async Task Post_EmployeeProvison_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      // Arrange
      Factory.TestSubject = "testOwner-123";
      Factory.TestUsername = "test.owner@test.local";
      Factory.TestAdminRights = 0; // Customer, kein Employe
      
      // Act
      var request = new HttpRequestMessage(
         method: HttpMethod.Post,
         requestUri: "/bankingapi/v1/employees/me/provision"
      );
      request.Headers.Add(TestAuthHandler.Header, "Employee");
      
      var response = await Client.SendAsync(request,ct);
      True(
         condition: response.StatusCode is HttpStatusCode.Created || response.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n"
      );
      
     var ownerProvisionDto = 
        await response.Content.ReadFromJsonAsync<CustomerProvisionDto>(ct); // helpful for debugging
     NotNull(ownerProvisionDto);
     var id = ownerProvisionDto.Id;
     
     // Assert (DB) – didaktisch stark
     await Factory.WithScopeAsync(async serviceProvider => {
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

        // IMPORTANT: use AsNoTracking to avoid tracking artifacts
        var employee = await dbContext.Employees
           .AsNoTracking()
           .Where(o => o.Id == id)
           .SingleOrDefaultAsync(ct);

        NotNull(employee);
        
        Equal(Factory.TestUsername, employee.EmailVo.Value);
        Equal(Factory.TestSubject, employee.Subject);

     });
   }
   
   
   [Fact]
   public async Task GetAndPut_EmployeeProfile_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      // Arrange
      Factory.TestSubject = "test-employee";
      Factory.TestUsername = "test.employee@test.local";
      Factory.TestAdminRights = 511; // Employe
      
      // Provisioning (idempotent, should return same owner on repeated calls)
      var request = new HttpRequestMessage(
         method: HttpMethod.Post,
         requestUri: "/bankingapi/v1/employees/me/provision"
      );
      request.Headers.Add(TestAuthHandler.Header, "Employee");
      
      var responsePostProvision = await Client.SendAsync(request, ct);
      True(
         condition: responsePostProvision.StatusCode is HttpStatusCode.Created,
         userMessage: $"Unexpected status {(int)responsePostProvision.StatusCode} {responsePostProvision.StatusCode}\n"
      );
      
      var employeeProvisionDto = 
         await responsePostProvision.Content.ReadFromJsonAsync<EmployeeProvisionDto>(ct); 
      
      // Act Get Profile and Post Profile (update)
      request = new HttpRequestMessage(
         HttpMethod.Get,
         "/bankingapi/v1/employees/me/profile" 
      );
      request.Headers.Add(TestAuthHandler.Header, "Employee");
      
      var responseGetProfile = await Client.SendAsync(request, ct);
      
      True(
         condition: responseGetProfile.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)responseGetProfile.StatusCode} {responseGetProfile.StatusCode}\n"
      );
      var getProfileEmployeeDto = 
         await responseGetProfile.Content.ReadFromJsonAsync<EmployeeDto>(ct);
      NotNull(getProfileEmployeeDto);
      
      // update profile with new data (except Id, Email and Status, which are not updatable in this scenario)
      var id = getProfileEmployeeDto.Id;
      var reqPostProfileOwnerDto = getProfileEmployeeDto with {
         Firstname = "Bernd",
         Lastname = "Rogalla",
         Phone = "+49 (0)1234 / 5678-9123",
         PersonnelNumber = "EMP-12345",
         IsActive = true,
      };
      var expectedEmail = EmailVo.Create(reqPostProfileOwnerDto.Email).Value;
      var expectdPhone = PhoneVo.Create(reqPostProfileOwnerDto.Phone).Value;

      // build request manually
      request = new HttpRequestMessage(
         method: HttpMethod.Put,
         requestUri: "/bankingapi/v1/employees/me/profile"
      );
      request.Headers.Add(TestAuthHandler.Header, "Employee");
      request.Content = JsonContent.Create(reqPostProfileOwnerDto);

      var responsePutProfile = await Client.SendAsync(request, ct);
      
      True(
         condition: responsePutProfile.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)responsePutProfile.StatusCode} {responsePutProfile.StatusCode}\n"
      );
    
      var resPostProfileEmployeeDto = 
         await responsePutProfile.Content.ReadFromJsonAsync<EmployeeDto>(ct);
      NotNull(resPostProfileEmployeeDto);

      var actualEmail = EmailVo.Create(resPostProfileEmployeeDto.Email).Value.Value;
      var actualPhone = PhoneVo.Create(resPostProfileEmployeeDto.Phone).Value.Value;
      Equal(reqPostProfileOwnerDto.Id, resPostProfileEmployeeDto.Id);
      Equal(reqPostProfileOwnerDto.Firstname, resPostProfileEmployeeDto.Firstname);
      Equal(reqPostProfileOwnerDto.Lastname, resPostProfileEmployeeDto.Lastname);
      Equal(reqPostProfileOwnerDto.Email, actualEmail);
      Equal(reqPostProfileOwnerDto.Phone, actualPhone);
      Equal(reqPostProfileOwnerDto.PersonnelNumber, resPostProfileEmployeeDto.PersonnelNumber);
      Equal(reqPostProfileOwnerDto.IsActive, resPostProfileEmployeeDto.IsActive);
      
      // Assert (DB) 
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

         // IMPORTANT: use AsNoTracking to avoid tracking artifacts
         var employee = await dbContext.Employees
            .AsNoTracking()
            .Where(o => o.Id == id)
            .SingleOrDefaultAsync(ct);

         NotNull(employee);
         
         Equal(reqPostProfileOwnerDto.Id, employee.Id);
         Equal(reqPostProfileOwnerDto.Firstname, employee.Firstname);
         Equal(reqPostProfileOwnerDto.Lastname, employee.Lastname);
         Equal(reqPostProfileOwnerDto.Email, employee.EmailVo.Value);
         Equal(reqPostProfileOwnerDto.Phone, employee.PhoneVo.Value);
         Equal(reqPostProfileOwnerDto.PersonnelNumber, employee.PersonnelNumber);
         Equal(reqPostProfileOwnerDto.IsActive, employee.IsActive);  
      });
   }
   
   [Fact]
   public async Task Employee_GetById_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      // Assert
      var employees = _seed.Employees;
      var employee = employees[1];
      
      // damit TestAuthHandler den
      await Factory.WithScopeAsync(async serviceProvider => {
         var db = serviceProvider.GetRequiredService<AppDbContext>();
         // seed here...
         db.Employees.AddRange(employees);
         await db.SaveChangesAsync(ct);
      });

      // Act
      var id = employee.Id;
      
      var request = new HttpRequestMessage(
         method: HttpMethod.Get,
         requestUri: $"/bankingapi/v1/employees/{id}"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");
      
      var response = await Client.SendAsync(request, ct);
      
      // status code must be 200 OK
      True(
         condition: response.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n"
      );
      
      // Assert
      var actualEmployeeDto = 
         await response.Content.ReadFromJsonAsync<EmployeeDto>(ct);
      NotNull(actualEmployeeDto);
      
      var actualEmailVo = EmailVo.Create(actualEmployeeDto?.Email).Value;
      var actualPhoneVo = PhoneVo.Create(actualEmployeeDto?.Phone).Value;
      
      Equals(employee.Id, actualEmployeeDto?.Id);
      Equals(employee.Firstname, actualEmployeeDto?.Firstname);
      Equals(employee.Lastname, actualEmployeeDto?.Lastname);
      Equals(employee.EmailVo, actualEmailVo);
      Equals(employee.PhoneVo, actualPhoneVo);
      Equals(employee.PersonnelNumber, actualEmployeeDto?.PersonnelNumber);
      Equals(employee.IsActive, actualEmployeeDto?.IsActive);
   }
   
   [Fact]
   public async Task Employee_GetByEmail_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      // Assert
      var employees = _seed.Employees; // damit TestAuthHandler den
      var employee = employees[0];
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
         // seed here...
         dbContext.Employees.AddRange(employees);
         await dbContext.SaveChangesAsync(ct);
      });

      // Act
      var email = employee.EmailVo;
      
      var request = new HttpRequestMessage(
         method: HttpMethod.Get,
         requestUri: $"/bankingapi/v1/employees/email/{email}"
      );
      request.Headers.Add(TestAuthHandler.Header, "Employee");
      
      var response = await Client.SendAsync(request, ct);
      
      // status code must be 200 OK
      True(
         condition: response.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n"
      );
      
      // Assert
      response.EnsureSuccessStatusCode();
      Equal(HttpStatusCode.OK, response.StatusCode);
      var actualEmployeeDto = 
         await response.Content.ReadFromJsonAsync<EmployeeDto>(ct);

      var actualEmailVo = EmailVo.Create(actualEmployeeDto?.Email).Value;
      var actualPhoneVo = PhoneVo.Create(actualEmployeeDto?.Phone).Value;
      
      Equals(employee.Id, actualEmployeeDto?.Id);
      Equals(employee.Firstname, actualEmployeeDto?.Firstname);
      Equals(employee.Lastname, actualEmployeeDto?.Lastname);
      Equals(employee.EmailVo, actualEmailVo);
      Equals(employee.PhoneVo, actualPhoneVo);
      Equals(employee.PersonnelNumber, actualEmployeeDto?.PersonnelNumber);
      Equals(employee.IsActive, actualEmployeeDto?.IsActive);
 
   }
}