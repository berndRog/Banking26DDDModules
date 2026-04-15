using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Enum;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.Customers._2_Application.UseCases;

/// <summary>
/// Employee use case: reject a customer (e.g., KYC failed).
/// </summary>
public sealed class CustomerUcReject(
   IIdentityGateway identityGateway,
   ICustomerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<CustomerUcReject> logger
) {

   public async Task<Result> ExecuteAsync(
      Guid customerId,
      RejectCode rejectCode,
      CancellationToken ct
   ) {
      // 1) Authorization: must be an employee/admin with the required rights
      if (identityGateway.AdminRights == 0)
         return Result.Failure(CustomerErrors.EmployeeRightsRequired);

      // 2) Validate input
      if (customerId == Guid.Empty)
         return Result.Failure(CustomerErrors.InvalidId);
      if (rejectCode == default)
         return Result.Failure(CustomerErrors.RejectionRequiresReason);

      // 3) Load aggregate
      var customer = await repository.FindByIdAsync(customerId,  ct);

      if (customer is null)
         return Result.Failure(CustomerErrors.NotFound);

      // 4) Domain change (audit + status transition)
      var rejectedAt = clock.UtcNow;
      var employeeId = ParseEmployeeId(identityGateway.Subject);
      var result = customer.Reject(employeeId, rejectCode, rejectedAt);
      if (result.IsFailure)
         return Result.Failure(result.Error);

      // 5) Persist
      var savedRows = await unitOfWork.SaveAllChangesAsync("Customer rejected by employee", ct);
      logger.LogInformation("Customer rejected customerId={customerId} savedRows={rows}", customerId, savedRows);

      return Result.Success();
   }

   private static Guid ParseEmployeeId(string subject) =>
      Guid.TryParse(subject, out var id) ? id : Guid.Empty;
}
