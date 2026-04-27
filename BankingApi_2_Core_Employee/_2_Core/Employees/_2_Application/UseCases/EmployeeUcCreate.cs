using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._2_Application.Dtos;
using BankingApi._2_Core.Employees._2_Application.Mappings;
using BankingApi._2_Core.Employees._3_Domain.Entities;
using BankingApi._2_Core.Employees._3_Domain.Errors;
using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.Employees._2_Application.UseCases;

/// <summary>
/// Use case: Create a new employee (EM-1).
///
/// Flow:
/// 1) Validate basic inputs (use-case guards)
/// 2) Check uniqueness constraints (personnel number / email)
/// 3) Create domain aggregate (Employee.Create)
/// 4) Add to repository + commit via UnitOfWork
///
/// Logging:
/// - Uses LogIfFailure for all business failures (Result-based)
/// - Does not handle technical exceptions (middleware responsibility)
/// </summary>
public sealed class EmployeeUcCreate(
   IIdentityGateway identityGateway,
   IEmployeeRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<EmployeeUcCreate> logger
) {
   public async Task<Result<EmployeeDto>> ExecuteAsync(
      EmployeeCreateDto employeeCreateDto,
      CancellationToken ct = default
   ) {
      // 1) subject required
      var resultSubject = SubjectCheck.Run(employeeCreateDto.Subject);
      if (resultSubject.IsFailure) 
         return Result<EmployeeDto>.Failure(resultSubject.Error);
      var subject = resultSubject.Value;

      // ---- Use-case guards (cheap validations) ----
      if (string.IsNullOrWhiteSpace(employeeCreateDto.PersonnelNumber))
         return Result<EmployeeDto>.Failure(EmployeeErrors.PersonnelNumberIsRequired);
      
      var resultEmail = EmailVo.Create(employeeCreateDto.Email);
      if (resultEmail.IsFailure)
         return Result<EmployeeDto>.Failure(resultEmail.Error);
      var emailVo = resultEmail.Value;
      
      var resultPhone = PhoneVo.Create(employeeCreateDto.Phone);
      if (resultPhone.IsFailure)
         return Result<EmployeeDto>.Failure(resultPhone.Error);
      var phoneVo = resultPhone.Value;
      
      // ---- Uniqueness checks (I/O) ----
      if (await repository.FindByEmailAsync(emailVo, ct) != null)
         return Result<EmployeeDto>.Failure(EmployeeErrors.EmailMustBeUnique);

      if (await repository.FindByPersonnelNumberAsync(employeeCreateDto.PersonnelNumber, ct) != null)
         return Result<EmployeeDto>.Failure(EmployeeErrors.PersonnelNumberMustBeUnique);

      // ---- Domain factory (invariants) ----
      var result = Employee.Create(
         firstname: employeeCreateDto.Firstname,
         lastname: employeeCreateDto.Lastname,
         emailVo: emailVo,
         phoneVo: phoneVo,
         subject: subject,
         personnelNumber: employeeCreateDto.PersonnelNumber,
         adminRights: (AdminRights) identityGateway.AdminRights,
         createdAt: clock.UtcNow,
         id: employeeCreateDto.Id.ToString()
      );
      if (result.IsFailure)
         return Result<EmployeeDto>.Failure(result.Error)
            .LogIfFailure(logger, "EmployeeUcCreate.DomainRejected", 
               new { employeeCreateDto.Firstname, employeeCreateDto.Lastname, emailVo, phoneVo, 
                  subject, employeeCreateDto.PersonnelNumber, identityGateway.AdminRights });

      // Add to repository
      var employee = result.Value!;
      repository.Add(employee);

      // Persist via UnitOfWork
      var savedRows = await unitOfWork.SaveAllChangesAsync("Employee created", ct);

      logger.LogInformation(
         "EmployeeUcCreate done Id={id} personnelNumber={nr} savedRows={rows}",
         employee.Id, employee.PersonnelNumber, savedRows);

      return Result<EmployeeDto>.Success(employee.ToEmployeeDto());
   }
}