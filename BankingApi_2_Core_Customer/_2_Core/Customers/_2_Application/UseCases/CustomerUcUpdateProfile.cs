using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._2_Application.Mappings;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Customers._1_Ports.Outbound;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._2_Application.Mappings;
using BankingApi._2_Core.Customers._3_Domain.Errors;
using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.Customers._2_Application.UseCases;

public class CustomerUcUpdateProfile(
   IIdentityGateway identityGateway,
   ICustomerRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<CustomerUcUpdateProfile> logger
) {
   
   public async Task<Result<CustomerDto>> ExecuteAsync(
      CustomerDto customerDto,
      CancellationToken ct
   ) {
      // subject from gateway
      var subjectResult = SubjectCheck.Run(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<CustomerDto>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // must be provisioned
      var customer = await repository.FindByIdentitySubjectAsync(subject, ct);
      if (customer is null)
         return Result<CustomerDto>.Failure(CustomerErrors.NotProvisioned);

      // optional: forbid employees/admins
      if (identityGateway.AdminRights != 0)
         return Result<CustomerDto>.Failure(
            CustomerErrors.EmployeesCannotUpdateCustomerProfile);
      if (customerDto.AddressDto is null)
         return Result<CustomerDto>.Failure(CustomerErrors.AddressIsRequired);

      // override email address (if changed) 
      var email = customer.EmailVo.Value;
      var resultDtoEmail = EmailVo.Create(customerDto.Email);
      if (resultDtoEmail.IsFailure)
         return Result<CustomerDto>.Failure(resultDtoEmail.Error);
      var emailVoDto = resultDtoEmail.Value;
      var emailDto = emailVoDto.Value;
      
      if (!string.Equals(email, emailDto, StringComparison.OrdinalIgnoreCase)) {
         // create new email value object from dto.Email
    
         // check uniqueness
         var existingByEmail = await repository.FindByEmailAsync(emailVoDto, ct);
         if (existingByEmail is not null && existingByEmail.Id != customer.Id)
            return Result<CustomerDto>.Failure(CustomerErrors.EmailAlreadyInUse);
         // override previous email
         email = emailVoDto.Value;
      }
      
      var dto = customerDto with { Email = email }; // for logging only

      // domain update (now includes country)
      var updateResult = customer.UpdateProfile(
         firstname: customerDto.Firstname,
         lastname: customerDto.Lastname,
         companyName: customerDto.CompanyName,
         emailVo: emailVoDto,
         addressVo: customerDto.AddressDto.ToAddressVo(),
         updatedAt: clock.UtcNow
      );
      if (updateResult.IsFailure)
         return Result<CustomerDto>.Failure(updateResult.Error)
            .LogIfFailure(logger, "CustomerUcUpdateProfile failed", new { dto = customerDto, subject });

      // persist changes with unit of work
      var savedRows = await unitOfWork.SaveAllChangesAsync("Customer profile updated", ct);

      logger.LogInformation(
         "Customer profile subject={sub} customerId={id} savedRows={rows}",
         subject, customer.Id, savedRows
      );
      
      return Result<CustomerDto>.Success(customer.ToCustomerDto());
   }
}