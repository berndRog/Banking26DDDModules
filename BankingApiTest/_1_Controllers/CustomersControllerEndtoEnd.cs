using System.Net;
using System.Net.Http.Json;
using BankingApi._2_Core.BuildingBlocks._2_Application.Mappings;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using BankingApiTest.TestController;
using BankingApiTest.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApiTest._2_Modules.Employees.Application;

public sealed class CustomersControllerEndtoEnd : TestBaseEndToEnd {
   private TestSeed _seed = new TestSeed();

   // For teaching: keep DB so students can inspect it afterwards.
   protected override bool DeleteDatabaseOnDispose => false;

   #region Post_Customer_Create
   [Fact]
   public async Task PostCustomer_Create_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      // Arrange
      var customer1 = _seed.Customer1();
      var account1 = _seed.Account1();

      var requestDto = new CustomerDto(
         Id: customer1.Id,
         Firstname: customer1.Firstname,
         Lastname: customer1.Lastname,
         CompanyName: customer1.CompanyName,
         Email: customer1.EmailVo.Value,
         StatusInt: (int) customer1.Status,
         AddressDto: customer1.AddressVo.ToAddressDto()
      );
      // Act
      Factory.TestSubject = "12345678-0000-0000-0000-000000000000";
      var account1Id = account1.Id.ToString();
      var ibanVo1 = account1.IbanVo;
      
      var request = new HttpRequestMessage(
         method: HttpMethod.Post,
         requestUri:"/bankingapi/v1/customers?"+
         $"accountId={Uri.EscapeDataString(account1Id)}&"+
         $"iban={Uri.EscapeDataString(ibanVo1.Value)}"
      );
      //request.Headers.Add(TestAuthHandler.Header, "Employee");
      request.Content = JsonContent.Create(requestDto);

      var response = await Client.SendAsync(request, ct);
      
      var customerDto = 
         await response.Content.ReadFromJsonAsync<CustomerDto>(ct);
      NotNull(customerDto);
      True(
         condition: response.StatusCode is HttpStatusCode.Created,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n{customerDto?.Id}"
      );

      // assert
      // Domain-level checks
      Equal(requestDto.Firstname, customerDto?.Firstname);
      Equal(requestDto.Lastname, customerDto?.Lastname);

      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

         // IMPORTANT: use AsNoTracking to avoid tracking artifacts
         var customer = await dbContext.Customers
            .AsNoTracking()
            .Where(o => o.Id == customerDto!.Id)
            .SingleOrDefaultAsync(ct);

         NotNull(customer);

         // Domain-level checks
         Equal(requestDto.Firstname, customer.Firstname);
         Equal(requestDto.Lastname, customer.Lastname);
         Equal(requestDto.Email, customer.EmailVo.Value);
         Equal(requestDto.StatusInt, (int)customer.Status);
         Equal(Factory.TestSubject, customer.Subject);
         Equal(requestDto.AddressDto, customer.AddressVo.ToAddressDto());
         
         var accounts = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.CustomerId == customerDto!.Id)
            .ToListAsync(ct);
         Equal(1, accounts?.Count); // exactly one account should be created
      });
   }
   #endregion

   #region Post_Customer_Provision
   [Fact]
   public async Task PostCustomer_Provison_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      // Arrange
      Factory.TestSubject = "testCustomer-123";
      Factory.TestUsername = "test.customer@test.local";
      Factory.TestAdminRights = 0; // Customer, kein Employe
      var provisionCustomer = _seed.CustomerRegister();
      var provisionDto = provisionCustomer.ToCustomerDto();

      // Act
      var request = new HttpRequestMessage(
         method: HttpMethod.Post,
         requestUri: "/bankingapi/v1/customers/me/provision"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");
      request.Content = JsonContent.Create(provisionDto);

      var response = await Client.SendAsync(request, ct);

      // status code can be 201 Created (if owner was just provisioned) or 200 OK (if owner already exist)
      True(
         condition: response.StatusCode is HttpStatusCode.Created || response.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n"
      );

      var ownerProvisionDto = 
         await response.Content.ReadFromJsonAsync<CustomerProvisionDto>(ct); // helpful for debugging
      NotNull(ownerProvisionDto);
      var id = ownerProvisionDto.Id;

      // Assert (DB)
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

         // IMPORTANT: use AsNoTracking to avoid tracking artifacts
         var owner = await dbContext.Customers
            .AsNoTracking()
            .Where(o => o.Id == id)
            .SingleOrDefaultAsync(ct);

         NotNull(owner);

         Equal(provisionDto.Firstname, owner.Firstname);
         Equal(provisionDto.Lastname, owner.Lastname);
         Equal(provisionDto.CompanyName, owner.CompanyName);
         Equal(provisionDto.AddressDto, owner.AddressVo.ToAddressDto());
         Equal(Factory.TestUsername, owner.EmailVo.Value);
         Equal(Factory.TestSubject, owner.Subject);
      });
   }
   #endregion

   #region Get_and_Post_Customer_Profile
   [Fact]
   public async Task GetAndPostCustomer_Profile_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      // Arrange
      Factory.TestSubject = "testCustomer-123";
      Factory.TestUsername = "test.customer@test.local";
      Factory.TestAdminRights = 0; // Customer
      var provisionCustomer = _seed.CustomerRegister();
      var provisionDto = provisionCustomer.ToCustomerDto();

      // Provisioning (idempotent, should return same owner on repeated calls)
      var request = new HttpRequestMessage(
         method: HttpMethod.Post,
         requestUri: "/bankingapi/v1/customers/me/provision"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");
      request.Content = JsonContent.Create(provisionDto);

      var responsePostProvision = await Client.SendAsync(request, ct);
      // status code must be 201 Created 
      True(
         condition: responsePostProvision.StatusCode is HttpStatusCode.Created,
         userMessage: $"Unexpected status {(int)responsePostProvision.StatusCode} {responsePostProvision.StatusCode}\n"
      );

      var customerProvisionDto =
         await responsePostProvision.Content.ReadFromJsonAsync<CustomerProvisionDto>(ct);

      // Act Get Profile and Put Profile (update)
      request = new HttpRequestMessage(
         method: HttpMethod.Get,
         requestUri: "/bankingapi/v1/customers/me/profile"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");

      var responseGetProfile = await Client.SendAsync(request, ct);

      // status code must be 200 OK
      True(
         condition: responseGetProfile.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)responseGetProfile.StatusCode} {responseGetProfile.StatusCode}\n"
      );

      var getProfileOwnerDto = 
         await responseGetProfile.Content.ReadFromJsonAsync<CustomerDto>(ct);
      NotNull(getProfileOwnerDto);
      Equal(provisionDto.Firstname, getProfileOwnerDto.Firstname);
      Equal(provisionDto.Lastname, getProfileOwnerDto.Lastname);
      Equal(provisionDto.CompanyName, getProfileOwnerDto.CompanyName);
      Equal(provisionDto.AddressDto, getProfileOwnerDto.AddressDto);

      // update profile with new data (except Id, Email and Status, which are not updatable in this scenario)
      var id = getProfileOwnerDto.Id;
      var addressVo = AddressVo.Create(
         street: "Herbert-Meyer-Str 7",
         postalCode: "29556",
         city: "Suderburg",
         country: "DE"
      ).Value;
      
      var reqPostProfileOwnerDto = getProfileOwnerDto with {
         Firstname = "Bernd",
         Lastname = "Rogalla",
         CompanyName = null,
         AddressDto = addressVo.ToAddressDto()
      };

      // build request manually
      request = new HttpRequestMessage(
         method: HttpMethod.Put,
         requestUri: "/bankingapi/v1/customers/me/profile"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");
      request.Content = JsonContent.Create(reqPostProfileOwnerDto);

      var responsePutProfile = await Client.SendAsync(request, ct);

      // status code must be 200 Ok
      True(
         condition: responsePutProfile.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)responsePutProfile.StatusCode} {responsePutProfile.StatusCode}\n"
      );

      var resPostProfileOwnerDto = 
         await responsePutProfile.Content.ReadFromJsonAsync<CustomerDto>(ct);
      NotNull(resPostProfileOwnerDto);

      Equal(reqPostProfileOwnerDto.Id, resPostProfileOwnerDto.Id);
      Equal(reqPostProfileOwnerDto.Firstname, resPostProfileOwnerDto.Firstname);
      Equal(reqPostProfileOwnerDto.Lastname, resPostProfileOwnerDto.Lastname);
      Equal(reqPostProfileOwnerDto.CompanyName, resPostProfileOwnerDto.CompanyName);
      Equal(reqPostProfileOwnerDto.Email, resPostProfileOwnerDto.Email);
      Equal(reqPostProfileOwnerDto.StatusInt, resPostProfileOwnerDto.StatusInt);
      Equal(reqPostProfileOwnerDto.AddressDto, resPostProfileOwnerDto.AddressDto);
      // Assert (DB) 
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

         // IMPORTANT: use AsNoTracking to avoid tracking artifacts
         var customer = await dbContext.Customers
            .AsNoTracking()
            .Where(o => o.Id == id)
            .SingleOrDefaultAsync(ct);

         NotNull(customer);

         Equal(reqPostProfileOwnerDto.Id, customer.Id);
         Equal(reqPostProfileOwnerDto.Firstname, customer.Firstname);
         Equal(reqPostProfileOwnerDto.Lastname, customer.Lastname);
         Equal(reqPostProfileOwnerDto.Email, customer.EmailVo.Value);
         Equal(reqPostProfileOwnerDto.StatusInt, (int)customer.Status);
         Equal(reqPostProfileOwnerDto.AddressDto, customer.AddressVo.ToAddressDto());
      });
   }
   #endregion

   #region Get_Customer_ById_and_Email
   [Fact]
   public async Task GetCustomer_ById_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      // Assert
      var employees = _seed.Customers;
      //  var owner = employees[0];
      var customer = employees[1];

      // damit TestAuthHandler den
      await Factory.WithScopeAsync(async serviceProvider => {
         var db = serviceProvider.GetRequiredService<AppDbContext>();
         // seed here...
         db.Customers.AddRange(employees);
         await db.SaveChangesAsync(ct);
      });

      // Act
      var id = customer.Id;

      var request = new HttpRequestMessage(
         method: HttpMethod.Get,
         requestUri: $"/bankingapi/v1/customers/{id}"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");

      var response = await Client.SendAsync(request, ct);

      // status code must be 200 OK
      True(
         condition: response.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n"
      );

      // Assert
      var actualCustomerDto = 
         await response.Content.ReadFromJsonAsync<CustomerDto>(ct);
      NotNull(actualCustomerDto);

      Equals(customer.Id, actualCustomerDto?.Id);
      Equals(customer.Firstname, actualCustomerDto?.Firstname);
      Equals(customer.Lastname, actualCustomerDto?.Lastname);
      Equals(customer.CompanyName, actualCustomerDto?.CompanyName);
      Equals(customer.EmailVo, actualCustomerDto?.Email);
      Equals((int)customer.Status, actualCustomerDto?.StatusInt);
      //Equal(Factory.TestSubject, owner.Subject);
      Equals(customer.AddressVo, actualCustomerDto);
   }

   [Fact]
   public async Task GetOwner_ByEmail_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      // Assert
      var customers = _seed.Customers;
      var customer1 = customers[0];
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
         // seed here...
         dbContext.Customers.AddRange(customers);
         await dbContext.SaveChangesAsync(ct);
      });

      // Act
      var email = customer1.EmailVo;

      var request = new HttpRequestMessage(
         method: HttpMethod.Get,
         requestUri: $"/bankingapi/v1/customers/email/{email}"
      );
      request.Headers.Add(TestAuthHandler.Header, "Customer");

      var response = await Client.SendAsync(request, ct);

      // status code must be 200 OK
      True(
         condition: response.StatusCode is HttpStatusCode.OK,
         userMessage: $"Unexpected status {(int)response.StatusCode} {response.StatusCode}\n"
      );

      // Assert
      response.EnsureSuccessStatusCode();
      Equal(HttpStatusCode.OK, response.StatusCode);
      var actualCustomerDto = 
         await response.Content.ReadFromJsonAsync<CustomerDto>(ct);

      Equals(customer1.Id, actualCustomerDto?.Id);
      Equals(customer1.Firstname, actualCustomerDto?.Firstname);
      Equals(customer1.Lastname, actualCustomerDto?.Lastname);
      Equals(customer1.CompanyName, actualCustomerDto?.CompanyName);
      Equals(customer1.EmailVo, actualCustomerDto?.Email);
      Equals((int)customer1.Status, actualCustomerDto?.StatusInt);
      Equals(customer1.AddressVo, actualCustomerDto);
   }
   #endregion

   #region Get_All_Customers
   [Fact]
   public async Task GetAllCustomers_ok() {
      var ct = TestContext.Current.CancellationToken;
      
      // Assert
      var customers = _seed.Customers;
      await Factory.WithScopeAsync(async serviceProvider => {
         var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
         // seed here...
         dbContext.Customers.AddRange(customers);
         await dbContext.SaveChangesAsync(ct);
      });

      // Act
      var request = new HttpRequestMessage(
         method: HttpMethod.Get,
         requestUri: $"/bankingapi/v1/customers"
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
      var actualCustomersDtos = 
         await response.Content.ReadFromJsonAsync<List<CustomerDto>>(ct);

      Equal(customers.Count, actualCustomersDtos?.Count);
      
   }
   #endregion
}