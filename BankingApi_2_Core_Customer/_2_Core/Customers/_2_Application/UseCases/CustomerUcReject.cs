using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._4_IntegrationContracts._1_Ports;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._3_Domain.Enum;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.Customers._2_Application.UseCases;

/// <summary>
/// Employee use case: reject a customer (e.g., KYC failed).
/// </summary>
public sealed class CustomerUcReject(
   IEmployeeContract employeeContract,
   ICustomerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<CustomerUcReject> logger
) {

   public async Task<Result> ExecuteAsync(
      Guid customerId,
      CustomerRejectCode customerRejectCode,
      CancellationToken ct
   ) {
      // 1) Authorization: check if caller is an employee with required rights
      var requiredRights = AdminRights.ManageCustomers;
      var resultEmployee = await employeeContract.GetAuthorizedEmployeeAsync(requiredRights, ct);
      if (resultEmployee.IsFailure)
         return Result.Failure(resultEmployee.Error);
      var employeeContractDto = resultEmployee.Value;

      // 2) Validate input
      if (customerId == Guid.Empty)
         return Result.Failure(CustomerErrors.InvalidId);
      if (customerRejectCode == default)
         return Result.Failure(CustomerErrors.RejectionRequiresReason);

      // 3) Load aggregate
      var customer = await repository.FindByIdAsync(customerId,  ct);

      if (customer is null)
         return Result.Failure(CustomerErrors.NotFound);

      // 4) Domain change (audit + status transition)
      var rejectedAt = clock.UtcNow;
      var employeeId = employeeContractDto.Id;
      var result = customer.Reject(employeeId, customerRejectCode, rejectedAt);
      if (result.IsFailure)
         return Result.Failure(result.Error);

      // 5) Persist
      var savedRows = await unitOfWork.SaveAllChangesAsync("Customer rejected by employee", ct);
      logger.LogInformation("Customer rejected customerId={customerId} savedRows={rows}", customerId, savedRows);

      return Result.Success();
   }
}
