using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._2_Application.Dtos;
using BankingApi._2_Core.BuildingBlocks._2_Application.Mappings;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Customers._3_Domain.Enum;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using BankingApiTest.TestInfrastructure;
namespace BankingApiTest._2_Core.Customers.Domain.Entities;

public sealed class CustomerUt {
   private readonly TestSeed _seed = default!;
   private readonly IClock _clock = default!;
   private readonly Customer _customer;
   private readonly Customer _customer5; // with CompanyName
   private readonly AddressVo _addressVo = default!;

   public CustomerUt() {
      _seed = new TestSeed();
      _clock = _seed.Clock;
      _customer = _seed.Customer1();
      _customer5 = _seed.Customer5();
      _addressVo = _seed.Address1Vo;
   }

   public static IEnumerable<object[]> InvalidLengths() {
      yield return new object[] { "A" }; // too short (1)
      yield return new object[] { new string('A', 81) }; // too long (81)
   }

   #region--- CreatePerson tests ------------------------------------------------------
   [Fact]
   public void CreateCustomer_valid_input_and_id_creates_customer() {
      // Act
      var result = Customer.Create(
         firstname: _customer.Firstname,
         lastname: _customer.Lastname,
         companyName: _customer.CompanyName,
         subject: _customer.Subject,
         emailVo: _customer.EmailVo,
         addressVo: _customer.AddressVo,
         createdAt: _customer.CreatedAt,
         id: _customer.Id.ToString()
      );

      // Assert
      True(result.IsSuccess);

      var actual = result.Value!;
      IsType<Customer>(actual);
      Equal(_customer.Id, actual.Id);
      Equal(_customer.Firstname, actual.Firstname);
      Equal(_customer.Lastname, actual.Lastname);
      Equal(_customer.CompanyName, actual.CompanyName);
      Equal(_customer.DisplayName, actual.DisplayName);
      Equal(_customer.EmailVo, actual.EmailVo);
      Equal(_customer.Subject, actual.Subject);
      Equal(_customer.Status, actual.Status);
      Equal(_customer.AddressVo, actual.AddressVo);
      True(actual.IsActive);
      True(actual.IsProfileComplete);
   }

   [Fact]
   public void CreateCustomer_valid_input_and_without_id() {
      // Act
      var result = Customer.Create(
         firstname: _customer.Firstname,
         lastname: _customer.Lastname,
         companyName: _customer.CompanyName,
         subject: _customer.Subject,
         emailVo: _customer.EmailVo,
         addressVo: _customer.AddressVo,
         createdAt: _customer.CreatedAt,
         id: null // <== without id
      );

      // Assert
      True(result.IsSuccess);

      var actual = result.Value!;
      IsType<Customer>(actual);
      False(actual.Id == Guid.Empty);
      Equal(_customer.Firstname, actual.Firstname);
      Equal(_customer.Lastname, actual.Lastname);
      Equal(_customer.CompanyName, actual.CompanyName);
      Equal(_customer.Subject, actual.Subject);
      Equal(_customer.EmailVo, actual.EmailVo);
      Equal(_addressVo, actual.AddressVo);
      Equal(_customer.DisplayName, actual.DisplayName);
      Equal(_customer.Status, actual.Status);
      True(actual.IsActive);
      True(actual.IsProfileComplete);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreateCustomer_invalid_firstname_fails(string firstname) {
      // Act
      var result = Customer.Create(
         firstname: firstname,
         lastname: _customer.Lastname,
         companyName: _customer.CompanyName,
         subject: _customer.Subject,
         emailVo: _customer.EmailVo,
         addressVo: _customer.AddressVo,
         createdAt: _customer.CreatedAt,
         id: _customer.Id.ToString()
      );

      // Assert
      True(result.IsFailure);
      Equal(CustomerErrors.FirstnameIsRequired, result.Error);
   }

   [Theory]
   [MemberData(nameof(InvalidLengths))]
   public void CreateCustomer_invalid_firstname_length_fails(string firstname) {
      var result = Customer.Create(
         firstname: firstname,
         lastname: _customer.Lastname,
         companyName: _customer.CompanyName,
         subject: _customer.Subject,
         emailVo: _customer.EmailVo,
         addressVo: _customer.AddressVo,
         createdAt: _customer.CreatedAt,
         id: _customer.Id.ToString()
      );

      True(result.IsFailure);
      Equal(CustomerErrors.InvalidFirstname, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreateCustomer_invalid_lastname_fails(string lastname) {
      // Act
      var result = Customer.Create(
         firstname: _customer.Firstname,
         lastname: lastname,
         companyName: _customer.CompanyName,
         subject: _customer.Subject,
         emailVo: _customer.EmailVo,
         addressVo: _customer.AddressVo,
         createdAt: _customer.CreatedAt,
         id: _customer.Id.ToString()
      );

      // Assert
      True(result.IsFailure);
      Equal(CustomerErrors.LastnameIsRequired, result.Error);
   }

   [Theory]
   [MemberData(nameof(InvalidLengths))]
   public void CreateCustomer_invalid_lastname_length_fails(string lastname) {
      var result = Customer.Create(
         firstname: _customer.Firstname,
         lastname: lastname,
         companyName: _customer.CompanyName,
         subject: _customer.Subject,
         emailVo: _customer.EmailVo,
         addressVo: _customer.AddressVo,
         createdAt: _customer.CreatedAt,
         id: _customer.Id.ToString()
      );

      True(result.IsFailure);
      Equal(CustomerErrors.InvalidLastname, result.Error);
   }

   [Fact]
   public void CreateCustomer_invalid_id_should_fail() {
      // Arrange
      var id = "not-a-guid";

      // Act
      var result = Customer.Create(
         firstname: _customer.Firstname,
         lastname: _customer.Lastname,
         companyName: _customer.CompanyName,
         subject: _customer.Subject,
         emailVo: _customer.EmailVo,
         addressVo: _customer.AddressVo,
         createdAt: _customer.CreatedAt,
         id: id
      );

      // Assert
      True(result.IsFailure);
      Equal(CustomerErrors.InvalidId, result.Error);
   }
   #endregion

   #region --- CreateCompany tests -----------------------------------------------------
   [Fact]
   public void CreateCompany_ok() {
      var result = Customer.Create(
         firstname: _customer5.Firstname,
         lastname: _customer5.Lastname,
         companyName: _customer5.CompanyName,
         subject: _customer5.Subject,
         emailVo: _customer5.EmailVo,
         addressVo: _customer5.AddressVo,
         createdAt: _customer5.CreatedAt,
         id: _customer5.Id.ToString()
      );

      // Assert
      True(result.IsSuccess);

      var actual = result.Value!;
      IsType<Customer>(actual);
      IsType<Customer>(actual);
      Equal(_customer5.Id, actual.Id);
      Equal(_customer5.Firstname, actual.Firstname);
      Equal(_customer5.Lastname, actual.Lastname);
      Equal(_customer5.CompanyName, actual.CompanyName);
      Equal(_customer5.DisplayName, actual.DisplayName);
      Equal(_customer5.EmailVo, actual.EmailVo);
      Equal(_customer5.Subject, actual.Subject);
      Equal(_customer5.Status, actual.Status);
      Equal(_customer5.AddressVo, actual.AddressVo);
      True(actual.IsActive);
      True(actual.IsProfileComplete);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreateCompany_invalid_companyName_length_ok(string companyName) {
      var result = Customer.Create(
         firstname: _customer5.Firstname,
         lastname: _customer5.Lastname,
         companyName: companyName,
         subject: _customer5.Subject,
         emailVo: _customer5.EmailVo,
         addressVo: _customer5.AddressVo,
         createdAt: _customer5.CreatedAt,
         id: _customer5.Id.ToString()
      );

      True(result.IsSuccess);
   }

   [Theory]
   [MemberData(nameof(InvalidLengths))]
   public void CreateCompany_invalid_companyName_length_fails(string companyName) {
      var result = Customer.Create(
         firstname: _customer5.Firstname,
         lastname: _customer5.Lastname,
         companyName: companyName,
         subject: _customer5.Subject,
         emailVo: _customer5.EmailVo,
         addressVo: _customer5.AddressVo,
         createdAt: _customer5.CreatedAt,
         id: _customer5.Id.ToString()
      );

      True(result.IsFailure);
      Equal(CustomerErrors.InvalidCompanyName, result.Error);
   }
   #endregion

   #region --- CreateProvision, UpdateProfle, Activate, Deactivate tests ------------------
   [Fact]
   public void CreateProvision_ok() {
      // Arrange
      var customer = _seed.CustomerRegister();

      // Act
      var result = Customer.CreateProvision(
         subject: customer.Subject,
         emailVo: customer.EmailVo,
         createdAt: customer.CreatedAt,
         id: customer.Id.ToString()
      );

      // Assert
      True(result.IsSuccess);
      var actual = result.Value;

      Equal(customer.Id, actual.Id);
      Equal(customer.Subject, actual.Subject);
      Equal(customer.EmailVo, actual.EmailVo);
      Equal(customer.CreatedAt, actual.CreatedAt);
      Equal(CustomerStatus.Pending, actual.Status);
      False(actual.IsActive);
   }
   
   [Fact]
   public void UpdateProfile_ok() {
      
      // Arrange: provisioned customer first
      var customer = _seed.CustomerRegister();
      var resultCreateProvision = Customer.CreateProvision(
         subject: customer.Subject,
         emailVo: customer.EmailVo,
         createdAt: customer.CreatedAt,
         id: customer.Id.ToString()
      );
      True(resultCreateProvision.IsSuccess);
      var newCustomer = resultCreateProvision.Value;
      
      // Act
      var resultUpdateProfile = newCustomer.UpdateProfile(
         firstname: customer.Firstname,
         lastname: customer.Lastname,
         companyName: customer.CompanyName,
         emailVo: customer.EmailVo,
         addressVo: customer.AddressVo,
         updatedAt: customer.UpdatedAt
      );

      // Assert
      True(resultUpdateProfile.IsSuccess);
      
      Equal(customer.Firstname, newCustomer.Firstname);
      Equal(customer.Lastname, newCustomer.Lastname);
      Equal(customer.CompanyName, newCustomer.CompanyName);
      Equal(customer.EmailVo, newCustomer.EmailVo);
      Equal(customer.AddressVo, newCustomer.AddressVo);
      Equal(customer.Subject, newCustomer.Subject);
      Equal(customer.CreatedAt, newCustomer.CreatedAt);
      Equal(customer.UpdatedAt, newCustomer.UpdatedAt);
      Equal(CustomerStatus.Pending, newCustomer.Status);
      True(newCustomer.IsProfileComplete);
      False(newCustomer.IsActive);
   }
   
   [Fact]
   public void Activate_ok() {
      
      // Arrange: provisioned customer first
      var customer = _seed.CustomerRegister();
      
      var resultCreateProvision = Customer.CreateProvision(
         subject: customer.Subject,
         emailVo: customer.EmailVo,
         createdAt: customer.CreatedAt,
         id: customer.Id.ToString()
      );
      True(resultCreateProvision.IsSuccess);
      var newCustomer = resultCreateProvision.Value;
      
      // update profile (required for activation)
      var resultUpdateProfile = newCustomer.UpdateProfile(
         firstname: customer.Firstname,
         lastname: customer.Lastname,
         companyName: customer.CompanyName,
         emailVo: customer.EmailVo,
         addressVo: customer.AddressVo,
         updatedAt: customer.UpdatedAt
      );
      True(resultUpdateProfile.IsSuccess);
      
      // Act
      var resultActivated = newCustomer.Activate(
         auditedByEmployeeId: _seed.Employee2().Id,
         activatedAt: (DateTimeOffset) customer.ActivatedAt!
      );
      True(resultActivated.IsSuccess);
      
      Equal(customer.Firstname, newCustomer.Firstname);
      Equal(customer.Lastname, newCustomer.Lastname);
      Equal(customer.CompanyName, newCustomer.CompanyName);
      Equal(customer.EmailVo, newCustomer.EmailVo);
      Equal(customer.AddressVo, newCustomer.AddressVo);
      Equal(customer.Subject, newCustomer.Subject);
      Equal(customer.CreatedAt, newCustomer.CreatedAt);
      Equal(customer.UpdatedAt, newCustomer.UpdatedAt);
      Equal(customer.ActivatedAt, newCustomer.ActivatedAt);
      Equal(CustomerStatus.Active, newCustomer.Status);
      True(newCustomer.IsProfileComplete);
      True(newCustomer.IsActive);

   }
   
   [Fact]
   public void Deactivate_ok() {
      
      // Arrange: provisioned customer first
      var customer = _seed.CustomerRegister();
      
      var resultCreateProvision = Customer.CreateProvision(
         subject: customer.Subject,
         emailVo: customer.EmailVo,
         createdAt: customer.CreatedAt,
         id: customer.Id.ToString()
      );
      True(resultCreateProvision.IsSuccess);
      var newCustomer = resultCreateProvision.Value;
      
      // update profile (required for activation)
      var resultUpdateProfile = newCustomer.UpdateProfile(
         firstname: customer.Firstname,
         lastname: customer.Lastname,
         companyName: customer.CompanyName,
         emailVo: customer.EmailVo,
         addressVo: customer.AddressVo,
         updatedAt: customer.UpdatedAt
      );
      True(resultUpdateProfile.IsSuccess);
      
      // activate customer (required for deactivation)
      var resultActivated = newCustomer.Activate(
         auditedByEmployeeId: _seed.Employee2().Id,
         activatedAt: (DateTimeOffset) customer.ActivatedAt!
      );
      True(resultActivated.IsSuccess);
      
      // Act
      var deactivatedAt = _clock.UtcNow.AddDays(15);
      var resultDeactivated = newCustomer.Deactivate(
         deactivatedByEmployeeId: _seed.Employee2().Id,
         deactivatedAt: deactivatedAt
      );
      True(resultDeactivated.IsSuccess);
      
      Equal(customer.Firstname, newCustomer.Firstname);
      Equal(customer.Lastname, newCustomer.Lastname);
      Equal(customer.CompanyName, newCustomer.CompanyName);
      Equal(customer.EmailVo, newCustomer.EmailVo);
      Equal(customer.AddressVo, newCustomer.AddressVo);
      Equal(customer.Subject, newCustomer.Subject);
      Equal(customer.CreatedAt, newCustomer.CreatedAt);
      Equal(deactivatedAt, newCustomer.UpdatedAt);      
      Equal(customer.ActivatedAt, newCustomer.ActivatedAt);
      Equal(deactivatedAt, newCustomer.DeactivatedAt);      
      Equal(CustomerStatus.Deactivated, newCustomer.Status);
      True(newCustomer.IsProfileComplete);
      False(newCustomer.IsActive);

   }
   #endregion

   /*
         // =========================================================================================
         // ChangeEmail tests
         // =========================================================================================
         #region --- ChangeEmail tests ---------------------------

         [Fact]
         public void ChangeEmail_valid_updates_email_and_updatedAt() {
            // Arrange
            var owner = Customer.Create(
               clock: _clock,
               firstname: _firstname,
               lastname: _lastname,
               companyName: null,
               email: _email,
               subject: _subject,
               id: _id
            ).Value!;

            var now = _seed.UtcNow.AddDays(1);
            var newEmail = "new.mail@domain.de";

            // Act
            var result = owner.ChangeEmail(newEmail, now);

            // Assert
            True(result.IsSuccess);
            Equal(newEmail, owner.Email);
            Equal(now, owner.UpdatedAt);
         }

         [Fact]
         public void ChangeEmail_now_default_fails() {
            var owner = Customer.Create(
               clock: _clock,
               firstname: _firstname,
               lastname: _lastname,
               companyName: null,
               email: _email,
               subject: _subject,
               id: _id
            ).Value!;

            var result = owner.ChangeEmail("new.mail@domain.de", utcNow: default);

            True(result.IsFailure);
            Equal(CommonErrors.TimestampIsRequired, result.Error);
         }

         #endregion

         // =========================================================================================
         // Status transition tests (Activate / Reject / Deactivate)
         // =========================================================================================
         #region --- Status transition tests (Activate / Reject / Deactivate) ---------------------------

         [Fact]
         public void Activate_now_default_fails() {
            var owner = Customer.Create(
               clock: _clock,
               firstname: _firstname,
               lastname: _lastname,
               companyName: null,
               email: _email,
               subject: _subject,
               id: _id
            ).Value!;

            var result = owner.Activate(
               employeeId: Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000"),
               utcNow: default
            );

            True(result.IsFailure);
            Equal(CommonErrors.TimestampIsRequired, result.Error);
         }

         [Fact]
         public void Activate_with_empty_employeeId_fails() {
            var owner = Customer.Create(
               clock: _clock,
               firstname: _firstname,
               lastname: _lastname,
               companyName: null,
               email: _email,
               subject: _subject,
               id: _id
            ).Value!;
            var utcNow = _seed.UtcNow;

            var result = owner.Activate(Guid.Empty, utcNow);

            True(result.IsFailure);
            Equal(CustomerErrors.AuditRequiresEmployee, result.Error);
            Equal(CustomerStatus.Pending, owner.Status);
            Null(owner.ActivatedAt);
            Null(owner.AuditedByEmployeeId);
         }

         [Fact]
         public void Activate_when_profile_incomplete_fails() {
            var owner = Customer.CreateProvision(_clock, _subject, _email, _seed.UtcNow, _id).Value!;
            False(owner.IsProfileComplete);

            var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
            var utcNow = _seed.UtcNow.AddDays(1);

            var result = owner.Activate(employeeId, utcNow);

            True(result.IsFailure);
            Equal(CustomerErrors.ProfileIncomplete, result.Error);
            Equal(CustomerStatus.Pending, owner.Status);
         }

         [Fact]
         public void Activate_when_pending_and_profile_complete_sets_active_and_audit_fields() {
            var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
            var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
            var utcNow = _seed.UtcNow.AddDays(1);

            var result = owner.Activate(employeeId, utcNow);

            True(result.IsSuccess);
            Equal(CustomerStatus.Active, owner.Status);
            Equal(utcNow, owner.ActivatedAt);
            Equal(employeeId, owner.AuditedByEmployeeId);
            True(owner.IsActive);
            Equal(utcNow, owner.UpdatedAt);
         }

         [Fact]
         public void Activate_when_not_pending_fails() {
            var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
            var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
            var utcNow = _seed.UtcNow.AddDays(1);

            var first = owner.Activate(employeeId, utcNow);
            True(first.IsSuccess);

            var second = owner.Activate(employeeId, utcNow.AddMinutes(1));

            True(second.IsFailure);
            Equal(CustomerErrors.NotPending, second.Error);
         }

         [Fact]
         public void Reject_now_default_fails() {
            var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;

            var result = owner.Reject(
               employeeId: Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000"),
               reasonCode: "KYC_FAILED",
               utcNow: default
            );

            True(result.IsFailure);
            Equal(CommonErrors.TimestampIsRequired, result.Error);
         }

         [Fact]
         public void Reject_with_empty_employeeId_fails() {
            var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
            var utcNow = _seed.UtcNow;

            var result = owner.Reject(Guid.Empty, "KYC_FAILED", utcNow);

            True(result.IsFailure);
            Equal(CustomerErrors.AuditRequiresEmployee, result.Error);
            Equal(CustomerStatus.Pending, owner.Status);
         }

         [Fact]
         public void Reject_with_missing_reason_fails() {
            var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
            var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
            var utcNow = _seed.UtcNow;

            var result = owner.Reject(employeeId, "   ", utcNow);

            True(result.IsFailure);
            Equal(CustomerErrors.RejectionRequiresReason, result.Error);
            Equal(CustomerStatus.Pending, owner.Status);
         }

         [Fact]
         public void Reject_when_pending_sets_rejected_and_audit_fields() {
            var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
            var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
            var utcNow = _seed.UtcNow.AddDays(1);

            var result = owner.Reject(employeeId, "KYC_FAILED", utcNow);

            True(result.IsSuccess);
            Equal(CustomerStatus.Rejected, owner.Status);
            Equal(utcNow, owner.RejectedAt);
            Equal(employeeId, owner.AuditedByEmployeeId);
            Equal("KYC_FAILED", owner.RejectionReasonCode);
            False(owner.IsActive);
            Equal(utcNow, owner.UpdatedAt);
         }

         [Fact]
         public void Reject_when_not_pending_fails() {
            var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
            var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
            var utcNow = _seed.UtcNow.AddDays(1);

            var act = owner.Activate(employeeId, utcNow);
            True(act.IsSuccess);

            var rej = owner.Reject(employeeId, "KYC_FAILED", utcNow.AddMinutes(1));

            True(rej.IsFailure);
            Equal(CustomerErrors.NotPending, rej.Error);
         }

         [Fact]
         public void Deactivate_now_default_fails() {
            var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;

            var result = owner.Deactivate(
               employeeId: Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000"),
               utcNow: default
            );

            True(result.IsFailure);
            Equal(CommonErrors.TimestampIsRequired, result.Error);
         }

         [Fact]
         public void Deactivate_with_empty_employeeId_fails() {
            var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
            var utcNow = _seed.UtcNow;

            var result = owner.Deactivate(Guid.Empty, utcNow);

            True(result.IsFailure);
            Equal(CustomerErrors.AuditRequiresEmployee, result.Error);
            Null(owner.DeactivatedAt);
            Null(owner.DeactivatedByEmployeeId);
            NotEqual(CustomerStatus.Deactivated, owner.Status);
         }

         [Fact]
         public void Deactivate_when_not_deactivated_sets_status_and_audit_fields() {
            var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
            var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
            var utcNow = _seed.UtcNow.AddDays(2);

            var result = owner.Deactivate(employeeId, utcNow);

            True(result.IsSuccess);
            Equal(CustomerStatus.Deactivated, owner.Status);
            Equal(utcNow, owner.DeactivatedAt);
            Equal(employeeId, owner.DeactivatedByEmployeeId);
            False(owner.IsActive);
            Equal(utcNow, owner.UpdatedAt);
         }

         [Fact]
         public void Deactivate_when_already_deactivated_fails() {
            var owner = Customer.Create(_clock, _firstname, _lastname, null, _email, _subject, _id).Value!;
            var employeeId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
            var now = _seed.UtcNow.AddDays(2);

            var first = owner.Deactivate(employeeId, now);
            True(first.IsSuccess);

            var second = owner.Deactivate(employeeId, now.AddMinutes(1));

            True(second.IsFailure);
            Equal(CustomerErrors.AlreadyDeactivated, second.Error);
         }
         */
   //#endregion
}