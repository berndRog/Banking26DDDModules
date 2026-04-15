using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.Customers._2_Application.UseCases;

public class CustomerUcCreateProvision(
   IIdentityGateway identityGateway,
   ICustomerRepository repository,
   IUnitOfWork unitOfWork,
   ILogger<CustomerUcCreateProvision> logger
) {
   
   public async Task<Result<CustomerProvisionDto>> ExecuteAsync(
      CustomerDto customerDto,
      CancellationToken ct
   ) {
      
      // 1) subject required
      var resultSubject = SubjectCheck.Run(identityGateway.Subject);
      if (resultSubject.IsFailure) 
         return Result<CustomerProvisionDto>.Failure(resultSubject.Error);
      var subject = resultSubject.Value;

      // 2) idempotent lookup
      var existing = await repository.FindByIdentitySubjectAsync(subject, ct);
      if (existing is not null) 
         return Result<CustomerProvisionDto>.Success(existing.ToCustomerProvisionDto(wasCreated: false));
      
      // 3) required identity data (translate missing-claim exceptions)
      string username;
      DateTimeOffset createdAt;
      try {
         username = identityGateway.Username;   // preferred_username
         createdAt = identityGateway.CreatedAt; // created_at
      }
      catch (InvalidOperationException ex) {
         logger.LogWarning(ex, "Provisioning failed: required identity claim missing (sub={sub})", subject);
         return Result<CustomerProvisionDto>.Failure(CommonErrors.IdentityClaimsMissing);
      }

      // interpret preferred_username as initial email
      var resultEmail = EmailVo.Create(username);
      if (resultEmail.IsFailure)
         return Result<CustomerProvisionDto>.Failure(resultEmail.Error);
      var emailVo = resultEmail.Value;

      // check uniqueness
      var existingWithEmail = await repository.FindByEmailAsync(emailVo, ct);
      if (existingWithEmail is not null)
         return Result<CustomerProvisionDto>.Failure(CustomerErrors.EmailAlreadyInUse);
      
      // 4) create aggregate
      var resultCustomer = Customer.CreateProvision(
         subject: subject,
         emailVo: emailVo,
         createdAt: createdAt,
         id: customerDto.Id.ToString()
      );
      if (resultCustomer.IsFailure)
         return Result<CustomerProvisionDto>.Failure(resultCustomer.Error)
            .LogIfFailure(logger, "CustomerUcCreateProvision.DomainRejected", 
               new { subject, emailVo, createdAt, customerDto.Id });

      // 5) add to repository
      var customer = resultCustomer.Value;
      repository.Add(customer);

      // 6) persist with unit of work
      var savedRows = await unitOfWork.SaveAllChangesAsync("Customer provisioned on first login", ct);

      logger.LogInformation(
         "Customer provisioned subject={sub} customerId={id} savedRows={rows}",
         subject, customer.Id, savedRows
      );
      
      return Result<CustomerProvisionDto>.Success(customer.ToCustomerProvisionDto(wasCreated: true));
   }
}