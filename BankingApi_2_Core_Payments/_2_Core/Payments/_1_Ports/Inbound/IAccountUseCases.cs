using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.Payments._2_Application.Dtos;
namespace BankingApi._2_Core.Payments._1_Ports.Inbound;

// Application port defining command use cases for the Accounts domain.
// Used by API controllers to trigger state changes in the account aggregate.
// Represents the write side of the Payments / Accounts context.
public interface IAccountUseCases {

   // Create a new account for a customer
   Task<Result<AccountDto>> CreateAsync(
      Guid customerId,
      AccountDto accountDto,
      CancellationToken ct = default
   );
   
   // deactivate an account 
   Task<Result> DeactivateAsync(
      Guid accountId,
      CancellationToken ct = default
   );

   // Add a beneficiary to an account
   // Beneficiaries represent allowed transfer targets
   Task<Result<BeneficiaryDto>> AddBeneficiaryAsync(
      Guid accountId,
      BeneficiaryDto beneficiaryDto,
      CancellationToken ct = default
   );

   // Remove a beneficiary from an account
   Task<Result> RemoveBeneficiaryAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   );

}

/*
Didaktik
--------

Dieses Interface beschreibt die UseCases (Commands) für den
Accounts-Bereich im Payments-Bounded-Context.

UseCases definieren fachliche Aktionen, die den Zustand eines
Aggregates verändern.

Typische Beispiele:

- Konto anlegen
- Begünstigten hinzufügen
- Begünstigten entfernen

Controller rufen diese Methoden auf, um fachliche Operationen
auszuführen. Die eigentliche Geschäftslogik befindet sich
im Application Layer und im Domain Model.

UseCases arbeiten mit Domain-Aggregaten und Repositories,
liefern aber DTOs zurück, die von der API verwendet werden.


Lernziele
---------

- Verständnis von UseCase-Ports in Clean Architecture
- Trennung von Commands und Queries (CQRS light)
- Modellierung fachlicher Aktionen im Application Layer
- Entkopplung von API und Domainlogik
*/