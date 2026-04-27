using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Enums;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
namespace BankingApi._2_Core.Payments._3_Domain.Entities;

public sealed class Transaction : Entity {

   //--- Properties ------------------------------------------------------------
   // debit or credit from perspective of this account
   public TransactionType Type { get; private set; }

   // payment reference / purpose
   public string Purpose { get; private set; } = string.Empty;
   // booked amount
   public MoneyVo AmountVo { get; private set; } = default!;
   // balance after this transaction
   public MoneyVo BalanceAfterVo { get; private set; } = default!;

   // booking timestamp
   public DateTimeOffset BookedAt { get; private set; }
   
   // debit Account
   // Transaction --> Account  [?] : [1]  (owning account)
   public Guid AccountId { get; private set; }

   // snapshot data for detail transaction display
   public string OtherAccountName { get; private set; } = string.Empty;
   public IbanVo OtherAccountIbanVo { get; private set; } = default!;
   
   // Transaction --> Tranfer  [?] : [0..1]
   public Guid? TransferId { get; private set; }
   

   //--- Constructors ----------------------------------------------------------
   // EF Core ctor
   private Transaction() { }

   // Domain ctor
   private Transaction(
      Guid id,
      Guid accountId,
      string otherAccountName,
      IbanVo otherAccountIbanVo,
      TransactionType type,
      MoneyVo amountVo,
      MoneyVo balanceAfterVo,
      string purpose,
      DateTimeOffset bookedAt
   ) {
      Id = id;
      AccountId = accountId;
      OtherAccountName = otherAccountName;
      OtherAccountIbanVo = otherAccountIbanVo;
      Type = type;
      AmountVo = amountVo;
      BalanceAfterVo = balanceAfterVo;
      Purpose = purpose;
      BookedAt = bookedAt;
   }

   //--- Static Factories ------------------------------------------------------
   public static Result<Transaction> CreateDebit(
      Guid accountId,              // DebitAccount
      string creditAccountName,    // CreditAccount
      IbanVo creditAccountIbanVo,  // CreditAccount 
      string purpose,
      MoneyVo amountVo,
      MoneyVo balanceAfterVo,
      DateTimeOffset bookedAt,
      string? id = null
   ) {
      var idResult = Resolve(id, TransactionErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Transaction>.Failure(idResult.Error);
      var transactionId = idResult.Value;
      
      var transaction =  new Transaction(
         id: transactionId,
         accountId: accountId,        // Debit
         otherAccountName: creditAccountName,
         otherAccountIbanVo: creditAccountIbanVo,
         type: TransactionType.Debit,
         purpose: purpose,
         amountVo: amountVo,
         balanceAfterVo: balanceAfterVo,
         bookedAt: bookedAt
      );
      
      return Result<Transaction>.Success(transaction);
   }

   public static Result<Transaction> CreateCredit(
      Guid accountId,              // CreditAccountId
      string debitAccountName,     // DebitName
      IbanVo debitAccountIbanVo,   // DebitIban
      string purpose,
      MoneyVo amountVo,
      MoneyVo balanceAfterVo,
      DateTimeOffset bookedAt,
      string? id = null
   ) {
      
      var idResult = Resolve(id, TransactionErrors.InvalidId);
      if (idResult.IsFailure)
         return Result<Transaction>.Failure(idResult.Error);
      var transactionId = idResult.Value;
      
      var transaction = new Transaction(
         id: transactionId,
         accountId: accountId,
         otherAccountName: debitAccountName,
         otherAccountIbanVo: debitAccountIbanVo,
         type: TransactionType.Credit,
         purpose: purpose,
         amountVo: amountVo,
         balanceAfterVo: balanceAfterVo,
         bookedAt:bookedAt
      );
      return Result<Transaction>.Success(transaction);
   }
   
   //--- Domain operations -----------------------------------------------------
   internal void AttachTransfer(Guid transferId) {
      TransferId = transferId;
   }
}
/*
 
 Didaktik und Lernziele
   
   In diesem Modell existieren zwei Aggregate im Payment-Kontext:
   
   1. Account
      Das Account-Aggregate verwaltet den Kontostand und die komplette
      Buchungshistorie eines Kontos. Jede Änderung des Kontostands erfolgt
      ausschließlich über PostDebit oder PostCredit. Dabei wird gleichzeitig 
      eine Transaction erzeugt.
   
   2. Transfer
      Das Transfer-Aggregate modelliert den fachlichen Geschäftsvorfall einer
      Überweisung. Ein Transfer verbindet genau zwei Transactions:
      - eine Debit-Transaction beim Senderkonto
      - eine Credit-Transaction beim Empfängerkonto
   
      Der Transfer speichert nur Referenzen auf diese Buchungen. Dadurch bleibt
      die Buchungshistorie im Account-Aggregate konsistent, während der Transfer
      den übergeordneten Geschäftsvorgang beschreibt.
   
   Dieses Modell trennt zwei fachliche Perspektiven:
   
   - Konto-Perspektive: Transactions beschreiben, was auf einem Konto passiert.
   - Geschäftsvorfall-Perspektive: Transfer beschreibt die Überweisung als
     zusammenhängenden Zahlungsvorgang.
   
   Die Trennung ermöglicht außerdem eine saubere Modellierung von Rückbuchungen,
   da eine Reversal-Überweisung eindeutig auf den ursprünglichen Transfer
   referenzieren kann.
 */
