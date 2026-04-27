using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._2_Application.Dtos;
using BankingApi._2_Core.Employees._2_Application.Mappings;
using BankingApi._2_Core.Employees._3_Domain.Entities;
using BankingApi._2_Core.Employees._3_Domain.Errors;
using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.Employees._2_Application.UseCases;

public class EmployeeUcCreateProvision(
   IIdentityGateway identityGateway,
   IEmployeeRepository repository,
   IUnitOfWork unitOfWork,
   ILogger<EmployeeUcCreateProvision> logger
) {
   public async Task<Result<EmployeeProvisionDto>> ExecuteAsync(
      string? id,
      CancellationToken ct
   ) {
      // 1) subject required
      var result = SubjectCheck.Run(identityGateway.Subject);
      if (result.IsFailure)
         return Result<EmployeeProvisionDto>.Failure(result.Error);
      var subject = result.Value;

      // 2) idempotent lookup
      var existing = await repository.FindByIdentitySubjectAsync(subject, ct);
      if (existing is not null)
         return Result<EmployeeProvisionDto>.Success(existing.ToEmployeeProvisionDto());

      // 3) required identity data (translate missing-claim exceptions)
      string username;
      DateTimeOffset createdAt;
      AdminRights adminRights;
      try {
         username = identityGateway.Username;   // preferred_username
         createdAt = identityGateway.CreatedAt; // created_at
         adminRights = (AdminRights)identityGateway.AdminRights; // admin_rights
      }
      catch (InvalidOperationException ex) {
         logger.LogWarning(ex, 
            "Provisioning failed: required identity claim missing (sub={sub})", subject);
         return Result<EmployeeProvisionDto>.Failure(CommonErrors.IdentityClaimsMissing);
      }

      // interpret preferred_username as initial email
      var resultEmail = EmailVo.Create(username);
      if (resultEmail.IsFailure)
         return Result<EmployeeProvisionDto>.Failure(resultEmail.Error);
      var emailVo = resultEmail.Value;

      // check uniqueness
      var existingWithEmail = await repository.FindByEmailAsync(emailVo, ct);
      if (existingWithEmail is not null)
         return Result<EmployeeProvisionDto>.Failure(EmployeeErrors.EmailAlreadyInUse);

      // 4) create aggregate
      var resultEmployee = 
         Employee.CreateProvision(subject, emailVo, createdAt, adminRights, id);
      if (resultEmployee.IsFailure)
         return Result<EmployeeProvisionDto>.Failure(resultEmployee.Error);

      // 5) add to repository
      var employee = resultEmployee.Value;
      repository.Add(employee);

      // 6) persist with unit of work
      var savedRows = await unitOfWork.SaveAllChangesAsync("Employee provisioned on first login", ct);

      logger.LogInformation(
         "Employee provisioned subject={sub} Id={id} savedRows={rows}",
         subject, employee.Id, savedRows
      );
      return Result<EmployeeProvisionDto>.Success(employee.ToEmployeeProvisionDto());
   }
}
   
