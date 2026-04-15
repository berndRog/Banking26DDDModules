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
/// Employee use case: deactivate a customer relationship
/// </summary>
public sealed class CustomerUcDeactivate(
   IEmployeeContract employeeContract,
   ICustomerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<CustomerUcDeactivate> logger
) {

   public async Task<Result> ExecuteAsync(
      Guid customerId,
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

      // 3) Load aggregate
      var customer = await repository.FindByIdAsync(customerId, ct);
      if (customer is null)
         return Result.Failure(CustomerErrors.NotFound);

      // 4) Domain model change (audit + status transition)
      // Customer can only be deactivated if currently in "Active" status
      if (customer.Status != CustomerStatus.Active)
         return Result.Failure(CustomerErrors.InvalidStatusTransition);
      
      // customer is deactivated
      var deactivatedAt = clock.UtcNow;
      var employeeId = employeeContractDto.Id;
      var result = customer.Deactivate(employeeId, deactivatedAt);
      if (result.IsFailure)
         return Result.Failure(result.Error)
            .LogIfFailure(logger, "CustomerUcDeactivated", new { customerId, employeeId, deactivatedAt });

      // 5) Save changes to database
      var rows = await unitOfWork.SaveAllChangesAsync("Customer deactivated by employee", ct);
      logger.LogInformation("Account deactivated: {customerId} rows={rows}", customerId, rows);

      return Result.Success();
   }
}
