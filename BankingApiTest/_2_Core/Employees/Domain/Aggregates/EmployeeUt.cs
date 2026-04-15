using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Employees._3_Domain.Entities;
using BankingApiTest.TestInfrastructure;
namespace BankingApiTest._2_Core.Employees.Domain.Aggregates;

public sealed class EmployeeUt {

   private readonly TestSeed _seed = default!;
   private readonly IClock _clock = default!;

   private readonly Employee _employee;

   public EmployeeUt() {
      _seed = new TestSeed();
      _clock = _seed.Clock;
      _employee = _seed.Employee1();
   }

   public static IEnumerable<object[]> InvalidNameLengths() {
      yield return new object[] { "A" };                         // too short (1)
      yield return new object[] { new string('A', 81) };         // too long (81)
   }
   
   [Fact]
   public void Create_valid_input_and_id_creates_employee() {

      // Act
      var result = Employee.Create(
         firstname: _employee.Firstname,
         lastname: _employee.Lastname,
         emailVo: _employee.EmailVo,
         phoneVo: _employee.PhoneVo,
         subject: _employee.Subject,
         personnelNumber: _employee.PersonnelNumber,
         adminRights: _employee.AdminRights,
         createdAt: _employee.CreatedAt,
         id: _employee.Id.ToString()
      );

      // Assert
      True(result.IsSuccess);
      var actual = result.Value!;
      IsType<Employee>(actual);
      Equal(_employee.Id, actual.Id);
      Equal(_employee.Firstname, actual.Firstname);
      Equal(_employee.Lastname, actual.Lastname);
      Equal(_employee.EmailVo, actual.EmailVo);
      Equal(_employee.PhoneVo, actual.PhoneVo);
      Equal(_employee.Subject, actual.Subject);
      Equal(_employee.PersonnelNumber, actual.PersonnelNumber);
      Equal(_employee.AdminRights, actual.AdminRights);
   }

   [Fact]
   public void Create_valid_input_and_without_id_creates_employee() {
      // Act
      var result = Employee.Create(
         firstname: _employee.Firstname,
         lastname: _employee.Lastname,
         emailVo: _employee.EmailVo,
         phoneVo: _employee.PhoneVo,
         subject: _employee.Subject,
         personnelNumber: _employee.PersonnelNumber,
         adminRights: _employee.AdminRights,
         createdAt: _employee.CreatedAt,
         id: null
      );
      
      // Assert
      True(result.IsSuccess);
      var actual = result.Value!;
      IsType<Employee>(actual);
      True(actual.Id != Guid.Empty);
      Equal(_employee.Firstname, actual.Firstname);
      Equal(_employee.Lastname, actual.Lastname);
      Equal(_employee.EmailVo, actual.EmailVo);
      Equal(_employee.PhoneVo, actual.PhoneVo);
      Equal(_employee.Subject, actual.Subject);
      Equal(_employee.PersonnelNumber, actual.PersonnelNumber);
      Equal(_employee.AdminRights, actual.AdminRights);
   }
   
/*
   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreatePerson_invalid_firstname_fails(string firstname) {
      // Act
      var result = Customer.Create(
         clock: _clock,
         firstname: firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id
      );

      // Assert
      True(result.IsFailure);
      Equal(CustomerErrors.FirstnameIsRequired, result.Error);
   }

   [Theory]
   [MemberData(nameof(InvalidNameLengths))]
   public void CreatePerson_invalid_firstname_length_fails(string firstname) {
      var result = Customer.Create(
         clock: _clock,
         firstname: firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id
      );

      True(result.IsFailure);
      Equal(CustomerErrors.InvalidFirstname, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   public void CreatePerson_invalid_lastname_fails(string lastname) {
      // Act
      var result = Customer.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id
      );

      // Assert
      True(result.IsFailure);
      Equal(CustomerErrors.LastnameIsRequired, result.Error);
   }

   [Theory]
   [MemberData(nameof(InvalidNameLengths))]
   public void CreatePerson_invalid_lastname_length_fails(string lastname) {
      var result = Customer.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: _id
      );

      True(result.IsFailure);
      Equal(CustomerErrors.InvalidLastname, result.Error);
   }

   [Theory]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("nonsense")]
   [InlineData("a.b.de")]
   public void CreatePerson_invalid_email_fails(string email) {
      // Act
      var result = Customer.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: email,
         subject: _subject,
         id: _id
      );

      // Assert
      True(result.IsFailure);
      // depending on your VO implementation this might be EmailIsRequired or CommonErrors.InvalidEmail
      // We assert failure is enough for teaching; refine if you want strict error matching.
   }

   [Fact]
   public void CreatePerson_with_valid_id_string_sets_id() {
      // Arrange
      var id = "11111111-1111-1111-1111-111111111111";

      // Act
      var result = Customer.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: id
      );

      // Assert
      True(result.IsSuccess);
      Equal(Guid.Parse(id), result.Value!.Id);
   }

   [Fact]
   public void CreatePerson_invalid_id_should_fail() {
      // Arrange
      var id = "not-a-guid";

      // Act
      var result = Customer.Create(
         clock: _clock,
         firstname: _firstname,
         lastname: _lastname,
         companyName: null,
         email: _email,
         subject: _subject,
         id: id
      );

      // Assert
      True(result.IsFailure);
      Equal(CustomerErrors.InvalidId, result.Error);
   }
*/
   
}
