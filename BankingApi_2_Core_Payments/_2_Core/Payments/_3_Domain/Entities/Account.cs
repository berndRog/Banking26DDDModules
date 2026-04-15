using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
namespace BankingApi._2_Core.Payments._3_Domain.Entities;

public sealed class Account : AggregateRoot {
   
   //--- Properties ------------------------------------------------------------
   // inherited from Entity + Aggregate root base class
   // public Guid Id { get; private set; } 
   // public DateTimeOffset CreatedAt { get; private set; }
   // public DateTimeOffset UpdatedAt { get; private set; }

   // IBAN as a domain value object.
   public IbanVo IbanVo { get; private set; } = default!;
   
   // Account balance as a domain value object.
   public MoneyVo BalanceVo { get; private set; } = default!;

   // Employee decisions (audit facts)
   public Guid? CreatedByEmployeeId { get; private set; }
   public DateTimeOffset? DeactivatedAt { get; private set; } = null;
   public Guid? DeactivatedByEmployeeId { get; private set; }
   
   public bool IsActive => DeactivatedAt == null;

   // BC: Account -> Customer [0..*] : [1]
   public Guid CustomerId { get; private set; }

   // Child Entities: Account -> Beneficiaries [1] : [0..*]
   private readonly List<Beneficiary> _beneficiaries = new();
   public IReadOnlyCollection<Beneficiary> Beneficiaries => 
      _beneficiaries.AsReadOnly();
   
   // Child Entities: Account -> Transactions [1] : [0..*]
   private readonly List<Transaction> _transactions = new();
   public IReadOnlyCollection<Transaction> Transactions => 
      _transactions.AsReadOnly();
   
   // Child Entities: Account -> Transfers [0..*]
   // private readonly List<Transfer> _transfers = new();
   // public IReadOnlyCollection<Transfer> Transfers => 
   //    _transfers.AsReadOnly();

   //--- Ctors -----------------------------------------------------------------
   // EF Core ctor
   private Account() { }

   // Domain ctor, to inject IClock for testing
   private Account(
      Guid id,
      IbanVo ibanVo,
      MoneyVo balanceVo,
      Guid customerId,
      Guid createdByEmployeeId
   )  {
      Id = id;
      IbanVo = ibanVo;
      BalanceVo = balanceVo;
      CustomerId = customerId;
      CreatedByEmployeeId = createdByEmployeeId;
   }

   //--- Static Factory --------------------------------------------------------
   // Static factory method to create a new account for an existing cutomer.
   public static Result<Account> Create(
      IbanVo ibanVo,
      MoneyVo balanceVo,
      Guid customerId,
      Guid createdByEmployeeId,
      DateTimeOffset createdAt,
      string? id = null
   ) {
      // invariant: customerId must be valid
      if (customerId == Guid.Empty)
         return Result<Account>.Failure(AccountErrors.InvalidCustomerId);
      
      if (createdByEmployeeId == Guid.Empty)
         return Result<Account>.Failure(AccountErrors.InvalidEmployeeId);
      
      var idResult = Resolve(id, AccountErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Account>.Failure(idResult.Error);
      var accountId = idResult.Value;

      if(balanceVo.Amount < 0)
         return Result<Account>.Failure(AccountErrors.InvalidBalance);
      
      // create entity
      var account = new Account(
         id: accountId, 
         customerId: customerId, 
         ibanVo: ibanVo, 
         balanceVo: balanceVo,
         createdByEmployeeId: createdByEmployeeId
      );
      
      // 
      account.Initialize(createdAt);
      
      return Result<Account>.Success(account);
   }

   //--- Domain operations -----------------------------------------------------
   // Employee deactivates the customer (end customer relationship).
   public Result Deactivate(
      Guid deactivatedByEmployeeId,
      DateTimeOffset deactivatedAt
   ) {
      if (deactivatedAt == default)
         return Result.Failure(CommonErrors.TimestampIsRequired);

      if (deactivatedByEmployeeId == Guid.Empty)
         return Result.Failure(AccountErrors.AuditRequiresEmployee);
      
      DeactivatedAt = deactivatedAt;
      DeactivatedByEmployeeId = deactivatedByEmployeeId;

      Touch(deactivatedAt);
      return Result.Success();
   }
   
   #region -------------------- Transactions --------------------------------------------
   // Debit = withdraw money from THIS account (Lastschrift)
   public Result<Transaction> PostDebit(
      string creditName,
      IbanVo creditIbanVo,
      MoneyVo amountVo,
      string purpose,
      DateTimeOffset bookedAt,
      string? id = null
   ) {
      // account must be active
      if (!IsActive)
         return Result<Transaction>.Failure(AccountErrors.InactiveAccount);

      // amount must be positive
      if (amountVo.Amount <= 0)
         return Result<Transaction>.Failure(AccountErrors.InvalidDebitAmount);
      
      // sufficient balance required
      if (BalanceVo < amountVo)
         return Result<Transaction>.Failure(AccountErrors.InsufficientFunds);

      // update balance (Lastschrift)
      BalanceVo = BalanceVo - amountVo;
      UpdatedAt = bookedAt;

      // create debit transaction
      var result = Transaction.CreateDebit(
         accountId: Id,
         creditAccountName: creditName,
         creditAccountIbanVo: creditIbanVo,
         purpose,
         amountVo,
         BalanceVo,
         bookedAt,
         id
      );
      if (result.IsFailure)
         return Result<Transaction>.Failure(result.Error);
      var transaction = result.Value;

      // add transaction to the list
      _transactions.Add(transaction);

      return Result<Transaction>.Success(transaction);
   }
   
   // Crebit = add money to THIS account (Gutschrift)
   public Result<Transaction> PostCredit(
      string debitName,    // who is sending the money
      IbanVo debitIbanVo,  // which sender iban is involved
      MoneyVo amountVo,
      string purpose,
      DateTimeOffset bookedAt,
      string? id = null
   ) {
      // account must be active
      if (!IsActive)
         return Result<Transaction>.Failure(AccountErrors.InactiveAccount);

      // amount must be positive
      if (amountVo.Amount <= 0)
         return Result<Transaction>.Failure(AccountErrors.InvalidCreditAmount);

      // update balance
      BalanceVo = BalanceVo + amountVo;
      UpdatedAt = bookedAt;

      // create credit transaction
      var result = Transaction.CreateCredit(
         accountId: Id,
         debitAccountName: debitName,
         debitAccountIbanVo: debitIbanVo,
         purpose,
         amountVo,
         BalanceVo,
         bookedAt,
         id
      );
      if(result.IsFailure)
         return Result<Transaction>.Failure(result.Error);
      var transaction = result.Value;

      // add transaction the list
      _transactions.Add(transaction);

      return Result<Transaction>.Success(transaction);
   }
   #endregion
   
   #region -------------------- Beneficiaries ------------------------------------------
   // Story 3.1: add a beneficiary to THIS account
   public Result<Beneficiary> AddBeneficiary(
      Beneficiary beneficiary,
      DateTimeOffset updatedAt
   ) {
      // check for duplicate IBANs
      if (_beneficiaries.Any(b => b.IbanVo.Equals(beneficiary.IbanVo)))
         return Result<Beneficiary>.Failure(BeneficiaryErrors.IbanAlreadyRegistred);
      
      // add to collection
      _beneficiaries.Add(beneficiary);
      Touch(updatedAt); 

      return Result<Beneficiary>.Success(beneficiary);
   }

   public Result<Beneficiary> FindBeneficiary(
      Guid beneficiaryId
   ) {
      var found = _beneficiaries.FirstOrDefault(b => b.Id == beneficiaryId);
      return found is null
         ? Result<Beneficiary>.Failure(BeneficiaryErrors.NotFound)
         : Result<Beneficiary>.Success(found);
   }

   public Result<Guid> RemoveBeneficiary(
      Guid beneficiaryId,
      DateTimeOffset updatedAt
   ) {
      if (beneficiaryId == Guid.Empty)
         return Result<Guid>.Failure(BeneficiaryErrors.InvalidId);

      // find beneficiary
      var found = _beneficiaries.FirstOrDefault(b => b.Id == beneficiaryId);
      if (found is null)
         return Result<Guid>.Failure(BeneficiaryErrors.NotFound);

      // remove from collection
      _beneficiaries.Remove(found);
      Touch(updatedAt); // update audit info

      return Result<Guid>.Success(beneficiaryId);
   }
   #endregion
   
   // #region -------------------- Transfers -----------------------------------------------
   // public Result<Transfer> AddTransfer(
   //    Transfer transfer,
   //    DateTimeOffset updatedAt
   // ) {
   //    // add to collection
   //    _transfers.Add(transfer);
   //    Touch(updatedAt); 
   //
   //    return Result<Transfer>.Success(transfer);
   // }
   //
   // public Result<Transfer> FindTransfers(
   //    Guid transferId
   // ) {
   //    var found = _transfers.FirstOrDefault(t => t.Id == transferId);
   //    return found is null
   //       ? Result<Transfer>.Failure(TransferErrors.NotFound)
   //       : Result<Transfer>.Success(found);
   // }
   // #endregion
}

/*
Didaktik und Lernziele
   
   In diesem Domännen-Modell ist das Konto (Account) das zentrale Aggregate im 
   Zahlungsverkehr.
   
   Ein Account verwaltet:
   - seinen Kontostand (Balance)
   - seine Buchungen (Transactions)
   - seine Zahlungsempfänger (Beneficiaries)
   
   Alle Änderungen des Kontostands erfolgen ausschließlich über die fachlichen
   Operationen Debit (Belastung) und Credit (Gutschrift). Dabei wird immer gleichzeitig
   eine Transaction erzeugt. Dadurch bleiben Kontostand und Buchungshistorie konsistent.
   
   Eine Transaction beschreibt eine einzelne Buchung aus Sicht genau eines Kontos.
   Sie enthält den Betrag, den Typ (Debit oder Credit), den Kontostand nach der
   Buchung sowie Informationen über die Gegenpartei.
   
   Eine Überweisung zwischen zwei Konten erzeugt zwei Transactions:
   - eine Debit-Transaction auf dem Senderkonto
   - eine Credit-Transaction auf dem Empfängerkonto
   
   Der fachliche Zusammenhang dieser beiden Buchungen wird durch das Transfer-
   Aggregate hergestellt. Dadurch können Geschäftsvorfälle eindeutig referenziert
   und später beispielsweise durch eine Rückbuchung (Reversal) wieder abgewickelt
   werden.
   
   Das Beispiel zeigt ein zentrales Prinzip von Domain Driven Design:
   Aggregate schützen ihre eigenen Invarianten, während fachliche Prozesse
   (z. B. eine Überweisung) mehrere Aggregate koordinieren können.
 
 */ 