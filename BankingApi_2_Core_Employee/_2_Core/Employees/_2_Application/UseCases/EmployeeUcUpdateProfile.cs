using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._2_Application.Dtos;
using BankingApi._2_Core.Employees._2_Application.Mappings;
using BankingApi._2_Core.Employees._3_Domain.Errors;
using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.Employees._2_Application.UseCases;

public class EmployeeUcUpdateProfile(
   IIdentityGateway identityGateway,
   IEmployeeRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<EmployeeUcUpdateProfile> logger
) {
   public async Task<Result<EmployeeDto>> ExecuteAsync(
      EmployeeDto employeeDto,
      CancellationToken ct
   ) {
      // subject from gateway
      var subjectResult = SubjectCheck.Run(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<EmployeeDto>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // must be provisioned
      var employee = await repository.FindByIdentitySubjectAsync(subject, ct);
      if (employee is null)
         return Result<EmployeeDto>.Failure(EmployeeErrors.NotProvisioned);

      // override email address (if changed) 
      var email = employee.EmailVo.Value;
      var resultDtoEmail = EmailVo.Create(employeeDto.Email);
      if (resultDtoEmail.IsFailure)
         return Result<EmployeeDto>.Failure(resultDtoEmail.Error);
      var emailVoDto = resultDtoEmail.Value;
      var emailDto = emailVoDto.Value;

      if (!string.Equals(email, emailDto, StringComparison.OrdinalIgnoreCase)) {
         // check uniqueness
         var existingByEmail = await repository.FindByEmailAsync(emailVoDto, ct);
         if (existingByEmail is not null && existingByEmail.Id != employee.Id)
            return Result<EmployeeDto>.Failure(EmployeeErrors.EmailAlreadyInUse);
         // override previous email
         email = emailDto;
      }

      var resultPhone = PhoneVo.Create(employeeDto.Phone);
      if (resultPhone.IsFailure)
         return Result<EmployeeDto>.Failure(resultPhone.Error);
      var phoneVo = resultPhone.Value;

      // domain update (now includes country)
      var updateResult = employee.UpdateProfile(
         firstname: employeeDto.Firstname,
         lastname: employeeDto.Lastname,
         emailVo: emailVoDto,
         phoneVo: phoneVo,
         personnelNumber: employeeDto.PersonnelNumber,
         updatedAt: clock.UtcNow
      );
      if (updateResult.IsFailure)
         return Result<EmployeeDto>.Failure(updateResult.Error);

      // persist changes with unit of work
      var savedRows = await unitOfWork.SaveAllChangesAsync("Employee profile updated", ct);

      logger.LogInformation(
         "Employee profile subject={sub} Id={id} savedRows={rows}",
         subject, employee.Id, savedRows
      );

      return Result<EmployeeDto>.Success(employee.ToEmployeeDto());
   }
}