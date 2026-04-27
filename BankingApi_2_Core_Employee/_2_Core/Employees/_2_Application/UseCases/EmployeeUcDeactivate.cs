using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._3_Domain.Errors;
using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.Employees._2_Application.UseCases;

/// <summary>
/// Use case: Deactivate an employee (EM-2).
///
/// Flow:
/// 1) Check guards
/// 2) Load employee aggregate (tracked)
/// 3) Apply domain transition (Deactivate)
/// 4) Commit via UnitOfWork
///
/// Logging:
/// - Uses LogIfFailure for NotFound and domain rejection
/// </summary>
public sealed class EmployeeUcDeactivate(
   IEmployeeRepository repository,
   IClock clock,
   IUnitOfWork unitOfWork,
   ILogger<EmployeeUcDeactivate> logger
) {

   public async Task<Result> ExecuteAsync(
      Guid employeeId,
      DateTimeOffset deactivatedAt = default,
      CancellationToken ct = default
   ) {
       // 1) Check guards
      if (deactivatedAt == default)
         return Result.Failure(EmployeeErrors.DeactivatedAtIsRequired)
            .LogIfFailure(logger, "EmployeeUcDeactivate.DeactivatedAtIsRequired", employeeId);
      
      if (employeeId == Guid.Empty) 
         return Result.Failure(EmployeeErrors.InvalidId)
            .LogIfFailure(logger, "EmployeeUcDeactivate.InvalidId", employeeId );
      
      // 2) Load aggregate (tracked)
      var employee = await repository.FindByIdAsync(employeeId, ct);
      if (employee is null) {
         var fail = Result.Failure(EmployeeErrors.NotFound);
         fail.LogIfFailure(logger, "EmployeeUcDeactivate.NotFound", new { employeeId });
         return fail;
      }
      
      // 3) Apply domain transition (pure)
      if(deactivatedAt == default) deactivatedAt = clock.UtcNow;
      var result = employee.Deactivate(deactivatedAt);
      if (result.IsFailure) {
         result.LogIfFailure(logger, "EmployeeUcDeactivate.DomainRejected", 
            new { employeeId, deactivatedAt });
         return result;
      }

      // 4) Persist changes
      var savedRows = await unitOfWork.SaveAllChangesAsync("Employee deactivated", ct);
      logger.LogInformation(
         "EmployeeUcDeactivate done employeeId={id} savedRows={rows}", 
         employeeId, savedRows);
      
      return Result.Success();
   }
}
