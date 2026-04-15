using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._4_IntegrationContracts._1_Ports;
using BankingApi._2_Core.BuildingBlocks.Utils;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using BankingApi._2_Core.Payments._2_Application.Mappings;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Enums;
using BankingApi._2_Core.Payments._3_Domain.Errors;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.Payments._2_Application.UseCases;

public sealed class TransferUcSendMoney(
   ICustomerContract customerContract,
   IAccountRepository accountRepository,
   ITransferRepository transferRepository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<TransferUcSendMoney> logger
) {
   public async Task<Result<TransferDto>> ExecuteAsync(
      SendMoneyDto dto,
      CancellationToken ct = default
   ) {
      // 1) Validate input -----------------------------------------------------
      // Read the raw amount from the request DTO.
      var amount = dto.Amount;
      // Guard clause: a transfer amount must be greater than zero.
      if (amount <= 0)
         return Result<TransferDto>.Failure(TransferErrors.InvalidAmount);

      // Convert the primitive amount into a validated Money value object.
      var resultVo = MoneyVo.Create(amount, (Currency) dto.Currency);
      if (resultVo.IsFailure)
         return Result<TransferDto>.Failure(resultVo.Error);
      var amountVo = resultVo.Value;

      // 2) Load required domain objects ---------------------------------------
      // Load the debit account together with its beneficiaries.
      // The selected beneficiary must belong to this sender account.
      var debitAccount = 
         await accountRepository.FindAccountByIdWithBeneficiariesAsync(dto.DebitAccountId, ct);
      if (debitAccount is null)
         return Result<TransferDto>.Failure(TransferErrors.DebitAccountNotFound);
      
      // find customer Displayname
      var resultDebitName = await customerContract.FindCustomerNameAsync(debitAccount.CustomerId, ct);
      if(resultDebitName.IsFailure)
         return Result<TransferDto>.Failure(resultDebitName.Error);
      var debitName = resultDebitName.Value;
      
      // Resolve the beneficiary from the debit account.
      var resultBeneficiary = debitAccount.FindBeneficiary(dto.BeneficiaryId);
      if (resultBeneficiary.IsFailure)
         return Result<TransferDto>.Failure(resultBeneficiary.Error);
      var beneficiary = resultBeneficiary.Value;
      var creditName = beneficiary.Name;
      var creditIbanVo = beneficiary.IbanVo;

      // Resolve the credit account via the beneficiary's IBAN.
      var creditAccount = await accountRepository.FindByIbanAsync(creditIbanVo, ct);
      if (creditAccount is null)
         return Result<TransferDto>.Failure(TransferErrors.CreditAccountNotFound);

      // Prevent transfers from an account to itself.
      if (creditAccount.Id == debitAccount.Id)
         return Result<TransferDto>.Failure(TransferErrors.SameAccountNotAllowed);

      // Use the injected clock to avoid direct dependency on system time.
      var bookedAt = dto.BookedAt ?? clock.UtcNow;

      // 3) Execute domain logic -----------------------------------------------
      // Post the debit transaction on the sender account.
      var resultDebit = debitAccount.PostDebit(
         creditName: creditName,
         creditIbanVo: creditIbanVo,
         amountVo: amountVo,
         purpose: dto.Purpose,
         bookedAt: bookedAt,
         id: dto.DebitId
      );
      if (resultDebit.IsFailure)
         return Result<TransferDto>.Failure(resultDebit.Error);
      var debitTransaction = resultDebit.Value;

      // Post the credit transaction on the receiver account.
      var resultCredit = creditAccount.PostCredit(
         debitName: debitName,
         debitIbanVo: debitAccount.IbanVo,
         amountVo: amountVo,
         purpose: dto.Purpose,
         bookedAt: bookedAt,
         id: dto.CreditId
      );
      if (resultCredit.IsFailure)
         return Result<TransferDto>.Failure(resultCredit.Error);
      var creditTransaction = resultCredit.Value;

      // Create the transfer entity that represents the complete business operation.
      // It links sender account, receiver account, debit booking, and credit booking.
      var transferResult = Transfer.CreateBooked(
         debitAccountId: debitAccount.Id,
         creditAccountIbanVo: creditAccount.IbanVo,
         amountVo: amountVo,
         purpose: dto.Purpose,
         debitTransactionId: debitTransaction.Id,
         creditTransactionId: creditTransaction.Id,
         bookedAt: bookedAt,
         id: dto.Id.ToString()
      );
      if (transferResult.IsFailure)
         return Result<TransferDto>.Failure(transferResult.Error);
      var transfer = transferResult.Value;

      // Add backward references from both transactions to the transfer.
      // This supports later navigation from a booking entry to the transfer.
      debitTransaction.AttachTransfer(transfer.Id);
      creditTransaction.AttachTransfer(transfer.Id);

      // // Register the transfer on the sender account aggregate.
      // debitAccount.AddTransfer(
      //    transfer: transfer,
      //    updatedAt: bookedAt
      // );

      // 4) Persist changes ----------------------------------------------------
      // Add the transfer to the repository so it becomes part of persistence.
      transferRepository.Add(transfer);

      // Persist all accumulated changes in a single unit of work.
      await unitOfWork.SaveAllChangesAsync("Send money", ct);

      // 5) Log and return result ----------------------------------------------
      // Write a log entry for diagnostics and traceability.
      logger.LogInformation(
         "Transfer booked ({TransferId}) Debit:({debit}) Credit:({credit}) amount ({Amount})",
         transfer.Id.To8(),
         debitAccount.Id.To8(),
         creditAccount.Id.To8(),
         amount
      );

      // Return the mapped DTO to the caller.
      return Result<TransferDto>.Success(transfer.ToTransferDto());
   }
}

/*
Didaktik:
Diese Version des Use Case ist bewusst in klar erkennbare Phasen gegliedert.
Dadurch wird sichtbar, dass ein Application Use Case typischerweise nicht „alles selbst macht“,
sondern einen Ablauf strukturiert. Die fünf Schritte sind hier:
1. Eingabe validieren
2. benötigte Domain-Objekte laden
3. fachliche Operationen ausführen
4. Änderungen persistent machen
5. Ergebnis zurückgeben

Das ist didaktisch wertvoll, weil Studierende so leichter erkennen, welche Verantwortung
in welcher Schicht liegt. Der Use Case ist also kein Ersatz für die Domain, sondern ein
Koordinator des Geschäftsablaufs. Die eigentlichen fachlichen Entscheidungen liegen weiterhin
in Methoden wie PostDebit, PostCredit oder Transfer.CreateBooked.

Lernziele:
- Studierende verstehen die typische Struktur eines Use Case in der Application-Schicht.
- Sie lernen, dass ein Anwendungsfall in logisch getrennte Phasen zerlegt werden kann.
- Sie erkennen den Unterschied zwischen Orchestrierung und fachlicher Regelprüfung.
- Sie sehen, wie Primitive aus einem DTO zuerst in Domain Value Objects überführt werden.
- Sie verstehen, wie Repositories, Unit of Work, Logger und Clock gemeinsam
  in einem Anwendungsfall zusammenwirken.
- Sie lernen, warum Result<T> für erwartbare fachliche Fehler oft besser geeignet ist
  als Exceptions mit Try/Catch.
- Sie erkennen, dass gute Code-Struktur nicht nur technisch, sondern auch didaktisch
  für Wartbarkeit, Lesbarkeit und Vermittlung wichtig ist.
*/