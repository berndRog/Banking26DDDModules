using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
namespace BankingApi._2_Core.Payments._1_Ports.Outbound;

// Read model interface for querying accounts, beneficiaies and transsfer data.
public interface IAccountReadModel {

   #region accounts
   // Find account by technical identifier
   Task<Result<AccountDto>> FindByIdAsync(
      Guid id,
      CancellationToken ctToken = default
   );

   // Find account using an IBAN
   Task<Result<AccountDto>> FindByIbanAsync(
      string iban,
      CancellationToken ct
   );

   // Return all accounts
   Task<Result<IEnumerable<AccountDto>>> SelectAsync(
      CancellationToken ctToken = default
   );

   // Return all accounts owned by a specific customer
   Task<Result<IEnumerable<AccountDto>>> SelectByCustomerIdAsync(
      Guid customerId,
      CancellationToken ct = default
   );
   #endregion
   
   #region beneficiaries
   // Find a beneficiary by identifier
   Task<Result<BeneficiaryDto>> FindBeneficiaryByIdAsync(
      Guid accountId,
      Guid beneficiaryId,
      CancellationToken ct = default
   );

   // Return all beneficiaries of an account
   Task<Result<IEnumerable<BeneficiaryDto>>> SelectBeneficiariesByAccountIdAsync(
      Guid accountId,
      CancellationToken ct = default
   );

   // Search beneficiaries by name
   Task<Result<IEnumerable<BeneficiaryDto>>> SelectBeneficiariesByNameAsync(
      Guid accountId,
      string name,
      CancellationToken ct = default
   );
   #endregion
   
   #region transactions
   // Load a transaction by its identifier
   Task<Result<TransactionDetailDto>> FindTransactionByAccountIdAndTransactionIdAsync(
      Guid accountId,
      Guid transactionId,
      CancellationToken ct = default
   );
   
   // Return all transactions of an account within a time period
   Task<Result<IEnumerable<TransactionDetailDto>>> SelectTransactionsByAccountIdAndPeriodAsync(
      Guid accountId,
      DateTimeOffset fromUtc,
      DateTimeOffset toUtc,
      CancellationToken ct = default
   );
   
   
   #endregion
   
   // Optional filtering / paging query
   // Task<Result<PagedResult<CustomerDto>>> FilterAsync(
   //    CustomerSearchFilter filter,
   //    PageRequest page,
   //    CancellationToken ct
   // );
}

/*
Didaktik
--------

Dieses Interface beschreibt ein ReadModel im Payments / Accounts
Bounded Context.

Ein ReadModel wird ausschließlich für Lesezugriffe verwendet
(Query-Seite im Sinne von CQRS).

Das ReadModel liefert Projektionen (DTOs) und keine Domain-Objekte.

Typische Einsatzfälle sind:

- Anzeige von Kontodetails
- Auflisten von Konten eines Kunden
- Anzeigen von Begünstigten (Beneficiaries)
- Suche nach IBAN oder Name

Der Zugriff erfolgt in der Regel über optimierte Datenbankabfragen
(z.B. LINQ-Projektionen mit AsNoTracking).

Ein wichtiger Unterschied zum Repository:

Repository
→ arbeitet mit Aggregates (Account)

ReadModel
→ arbeitet mit DTOs (AccountDto, BeneficiaryDto)


Lernziele
---------

- Unterschied zwischen Repository und ReadModel verstehen
- Einsatz von CQRS für getrennte Lese- und Schreiboperationen
- Verwendung von DTO-Projektionen für effiziente Abfragen
- Entkopplung von Domainmodell und API-Ausgabe
*/