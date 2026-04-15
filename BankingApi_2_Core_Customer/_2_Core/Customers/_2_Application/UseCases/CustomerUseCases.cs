using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.Customers._1_Ports.Inbound;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._3_Domain.Enum;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._2_Core.Customers._2_Application.UseCases;


// UseCases Facade for Customer aggregate
internal class CustomerUseCases(
   CustomerUcCreate createUc,
   CustomerUcCreateProvision createProvisionUc,
   CustomerUcUpdateProfile updateProfileUc,
   CustomerUcActivate activateUc,
   CustomerUcReject rejectUc,
   CustomerUcDeactivate deactivateUc,
   CustomerUcUpdate updateUc
): ICustomerUseCases {

   public Task<Result<CustomerDto>> CreateAsync(
      CustomerCreateDto customerCreateDto,
      CancellationToken ct
   ) => createUc.ExecuteAsync(
      customerCreateDto: customerCreateDto,
      ct: ct
   );

   public Task<Result<CustomerProvisionDto>> CreateProvisionAsync(
      CustomerDto customerDto,
      CancellationToken ct
   ) => createProvisionUc.ExecuteAsync(customerDto, ct);

   public Task<Result<CustomerDto>> UpdateProfileAsync(
      CustomerDto dto, 
      CancellationToken ct
   ) => updateProfileUc.ExecuteAsync(dto, ct);
   
   public Task<Result> ActivateAsync(
      Guid customerId,
      string? accountId,
      string? iban,
      decimal? balance,
      CancellationToken ct
   ) => activateUc.ExecuteAsync(
      customerId: customerId,
      accountId: accountId, 
      iban: iban,
      balance: balance,
      ct: ct);

   public Task<Result> RejectAsync(
      Guid customerId, 
      RejectCode rejectCode,
      CancellationToken ct
   ) => rejectUc.ExecuteAsync(customerId, rejectCode, ct);
   
   public Task<Result> DeactivateAsync(
      Guid customerId,
      CancellationToken ct
   ) => deactivateUc.ExecuteAsync(customerId, ct);
   
   public Task<Result> UpdateAsync(
      CustomerDto customerDto, 
      CancellationToken ct = default
   ) => updateUc.ExecuteAsync(customerDto,ct);
   
}