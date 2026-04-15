using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
namespace BankingApi._2_Core.Employees._3_Domain.Errors;

/// <summary>
/// Domain-level error definitions for employee-related validation and business rules.
/// </summary>
public static class EmployeeErrors {
   public static readonly DomainErrors FirstnameIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Employee: First Name Is Required",
         Message: "A First Name Must Be Provided."
      );

   public static readonly DomainErrors InvalidFirstname =
      new(
         ErrorCode.BadRequest,
         Title: "Employee: Invalid Firstname",
         Message: "The Provided Firstname is too shot or too long (2-100 chars)."
      );

   public static readonly DomainErrors LastnameIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Employee: Last Name Is Required",
         Message: "A Last Name Must Be Provided."
      );

   public static readonly DomainErrors InvalidLastname =
      new(
         ErrorCode.BadRequest,
         Title: "Employee: Invalid Lastname",
         Message: "The Provided Lastname is too shot or too long (2-100 chars)."
      );

   public static readonly DomainErrors EmailIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Employee: Email Is Required",
         Message: "An Email Address Must Be Provided."
      );

   public static readonly DomainErrors EmailInvalidFormat =
      new(
         ErrorCode.BadRequest,
         Title: "Employee: Invalid Email Format",
         Message: "The email address has an invalid format."
      );

   public static readonly DomainErrors EmailMustBeUnique =
      new(
         ErrorCode.Conflict,
         Title: "Employee: Email Must Be Unique",
         Message: "An employee with the given email address already exists."
      );

   public static readonly DomainErrors InvalidEmail =
      new(
         ErrorCode.BadRequest,
         Title: "Employee: Invalid email address",
         Message: "The provided email address is not valid."
      );
   
   public static readonly DomainErrors InvalidId =
      new(
         ErrorCode.BadRequest,
         Title: "Employee: Invalid Employee Id",
         Message: "The provided employee id is invalid."
      );

   public static readonly DomainErrors PersonnelNumberIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Employee: Personnel Number Is Required",
         Message: "A personnel number must be provided."
      );

   public static readonly DomainErrors PersonnelNumberInvalidFormat =
      new(
         ErrorCode.BadRequest,
         Title: "Employee: Invalid Personnel Number Format",
         Message: "The personnel number has an invalid format."
      );

   public static readonly DomainErrors PersonnelNumberMustBeUnique =
      new(
         ErrorCode.Conflict,
         Title: "Employee: Personnel Number Must Be Unique",
         Message: "An employee with the given personnel number already exists."
      );

   public static readonly DomainErrors AdminRightsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Employee: Admin Rights Required",
         Message: "An employee must have at least one admin right."
      );

   public static readonly DomainErrors AdminRightsNotSufficient =
      new(
         ErrorCode.Forbidden,
         Title: "Employee: Insufficient Admin Rights",
         Message: "The authenticated employee does not have sufficient administrative permissions to perform this action."
      );
   
   public static readonly DomainErrors InvalidAdminRightsBitmask =
      new(
         ErrorCode.BadRequest,
         Title: "Employee: Invalid Admin Rights Bitmask",
         Message: "The provided admin rights value contains undefined or unsupported flag bits."
      );

   public static readonly DomainErrors CreatedAtIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Employee: Creation Timestamp Required",
         Message: "The creation timestamp (createdAt) must be provided."
      );

   public static readonly DomainErrors AlreadyDeactivated =
      new(
         ErrorCode.Conflict,
         Title: "Employee Already Deactivated",
         Message: "The employee is already deactivated."
      );
   public static readonly DomainErrors AlreadyActive =
      new(
         ErrorCode.Conflict,
         Title: "Employee Already Active",
         Message: "The employee is already active."
      );

   public static readonly DomainErrors DeactivatedAtIsRequired =
      new(
         ErrorCode.BadRequest,
         Title: "Employee: Deactivation Timestamp Required",
         Message: "The deactivation timestamp (deactivatedAt) must be provided when creating an employee."
      );
   
   // Authorization / Access control
   // ---------------------------------------------------------------------
   public static readonly DomainErrors NotAuthenticated =
      new(
         ErrorCode.Unauthorized,
         Title: "Employee Not Authenticated",
         Message: "The current user is not authenticated as an employee."
      );

   public static readonly DomainErrors MissingAdminRights =
      new(
         ErrorCode.Forbidden,
         Title: "Employee: Missing Admin Rights",
         Message: "The employee does not have the required administrative rights."
      );

   public static readonly DomainErrors ManageEmployeesRightRequired =
      new(
         ErrorCode.Forbidden,
         Title: "Employee: Manage Customers Right Required",
         Message: "The employee must have the 'ManageEmployees' admin right to perform this action."
      );

   public static readonly DomainErrors ViewEmployeesRightRequired =
      new(
         ErrorCode.Forbidden,
         Title: "Employee: View Customers Right Required",
         Message: "The employee must have the 'ViewEmployees' admin right to access customer data."
      );
   
   public static readonly DomainErrors NotProvisioned =
      new(ErrorCode.NotFound,
         Title: "Employee: Is not provisioned",
         Message: "No employee with the given sub exists.");

   public static readonly DomainErrors NotFound =
      new(
         ErrorCode.NotFound,
         Title: "Employee: Not found",
         Message: "No employee with the given id exists."
      );
   
   public static readonly DomainErrors OwnerCannotUpdateEmployeeProfile =
      new(ErrorCode.UnprocessableEntity,
         Title: "Employee: Customer cannot update Employee profiles",
         Message: "The employee profile is blocked against owner access.");
   
   public static readonly DomainErrors EmployeeRightsRequired =
      new(
         ErrorCode.Forbidden,
         Title: "Employee: Admin Rights Required",
         Message: "Employee: This operation requires employee privileges."
      );
   
   public static readonly DomainErrors EmailAlreadyInUse =
      new(ErrorCode.Conflict,
         Title: "Employee: Email Already Used",
         Message: "The employee email is already in use by another employee");


}