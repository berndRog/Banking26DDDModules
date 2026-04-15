// using BankingApi._2_Core.BuildingBlocks;
// using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
// using BankingApi._2_Core.Payments._1_Ports.Outbound;
// using BankingApi._2_Core.Payments._2_Application.Dtos;
// using BankingApi._2_Core.Payments._2_Application.Mappings;
// using BankingApi._2_Core.Payments._3_Domain.Entities;
// using BankingApi._2_Core.Payments._3_Domain.Errors;
// namespace BankingApi._2_Core.Payments._2_Application.UseCases;
//
// public sealed class TransferUcReverse(
//    IAccountRepository accountRepository,
//    ITransferRepository transferRepository,
//    IUnitOfWork unitOfWork,
//    IClock clock,
//    ILogger<TransferUcReverse> logger
// ) {
//
//    public async Task<Result<TransferDto>> ExecuteAsync(
//       Guid accountId,
//       Guid transferId,
//       string purpose,
//       CancellationToken ct = default
//    ) { 
//       // 1) Load original transfer ---------------------------------------------
//       
//       // Load the original transfer that should be reversed.
//       var originalTransfer = await transferRepository.FindByIdAsync(transferId, ct);
//       if (originalTransfer is null)
//          return Result<TransferDto>.Failure(TransferErrors.OriginalTransferNotFound);
//
//       // 2) Load participating accounts ----------------------------------------
//       // For a reversal, the booking direction is inverted:
//       // original debitAccount becomes creditAccount
//       // original credditAccount becomes debitAccoutn
//       var creditAccount = await accountRepository.FindAccountByIdWithBeneficiariesAsync(
//          accountId: originalTransfer.DebitAccountId,
//          ct: ct
//       );
//       var debitAccount = await accountRepository.FindByIbanAsync(
//          ibanVo: originalTransfer.CreditAccountIbanVo,
//          ct
//       );
//
//       if (debitAccount is null)
//          return Result<TransferDto>.Failure(TransferErrors.DebitAccountNotFound);
//       if (creditAccount is null)
//          return Result<TransferDto>.Failure(TransferErrors.CreditAccountNotFound);
//
//       // 3) Validate reversal preconditions ------------------------------------
//       // Ensure that the requested account matches the original sender account.
//       // Only the original sender side should be allowed to reference this transfer here.
//       if (originalTransfer.DebitAccountId != accountId)
//          return Result<TransferDto>.Failure(TransferErrors.FromAccountIdMismatch);
//
//       // Use the injected clock for deterministic and testable timestamps.
//       var bookedAt = clock.UtcNow;
//
//       // 4) Execute reversal bookings ------------------------------------------
//       
//       // Post the debit on the account that originally received the money.
//       var resultDebit = debitAccount.PostDebit(
//          amountVo: originalTransfer.AmountVo,
//          purpose: purpose,
//          bookedAt: bookedAt
//       );
//       if (resultDebit.IsFailure)
//          return Result<TransferDto>.Failure(resultDebit.Error);
//       var debitTransaction = resultDebit.Value;
//
//       // Post the credit on the account that originally sent the money.
//       var resultCredit = creditAccount.PostCredit(
//          amountVo: originalTransfer.AmountVo,
//          purpose: purpose,
//          bookedAt: bookedAt
//       );
//       if (resultCredit.IsFailure)
//          return Result<TransferDto>.Failure(resultCredit.Error);
//       var creditTransaction = resultCredit.Value!;
//
//       // 5) Create and link reversal transfer ----------------------------------
//       // Create the reversal transfer based on the original transfer.
//       var reversalResult = Transfer.CreateReversalFromOriginal(
//          originalTransfer: originalTransfer,
//          reversalPurpose: $"Reversal tranfer for: {purpose}",
//          debitAccountId: debitAccount.Id,
//          creditAccountIbanVo: creditAccount.IbanVo,
//          reversalDebitTransactionId: debitTransaction.Id,
//          reversalCreditTransactionId: creditTransaction.Id,
//          bookedAt: bookedAt
//       );
//       if (reversalResult.IsFailure)
//          return Result<TransferDto>.Failure(reversalResult.Error);
//       var reversalTransfer = reversalResult.Value;
//       
//       // Add backward references from both transactions to the transfer.
//       // This supports later navigation from a booking entry to the transfer.
//       debitTransaction.AttachTransfer(reversalTransfer.Id);
//       creditTransaction.AttachTransfer(reversalTransfer.Id);
//
//       // Register the transfer on the sender account aggregate.
//       debitAccount.AddTransfer(
//          transfer: reversalTransfer,
//          updatedAt: bookedAt
//       );
//       
//       // Mark the original transfer as reversed and link it to the reversal transfer.
//       var markResult = originalTransfer.MarkAsReversed(
//          reversalTransfer.Id,
//          bookedAt
//       );
//       if (markResult.IsFailure)
//          return Result<TransferDto>.Failure(markResult.Error);
//
//       // 6) Persist changes ----------------------------------------------------
//
//       // Add the reversal transfer so it becomes part of persistence.
//       transferRepository.Add(reversalTransfer);
//
//       // Persist all accumulated changes in one unit of work.
//       await unitOfWork.SaveAllChangesAsync("Reverse transfer", ct);
//
//       // 7) Log and return result ----------------------------------------------
//
//       // Write a log entry for diagnostics and auditability.
//       logger.LogInformation(
//          "Transfer reversed ({TransferId}) by ({ReversalTransferId})",
//          originalTransfer.Id,
//          reversalTransfer.Id
//       );
//
//       // Return the reversal transfer as DTO.
//       return Result<TransferDto>.Success(reversalTransfer.ToTransferDto());
//    }
// }
//
// /*
// Didaktik:
// Dieser Use Case zeigt sehr anschaulich, dass eine Rückbuchung fachlich nicht einfach
// das Löschen einer Überweisung ist. Stattdessen wird eine neue fachliche Operation erzeugt,
// die die ursprüngliche Buchung in Gegenrichtung ausgleicht. Das ist ein sehr typisches
// Muster in Domänen mit Buchungslogik, weil fachlich relevante Vorgänge nachvollziehbar,
// auditierbar und historisch erhalten bleiben müssen.
//
// Außerdem wird hier deutlich, dass die Application-Schicht nicht die eigentliche Fachlogik
// „erfindet“, sondern den Ablauf eines Anwendungsfalls strukturiert:
// - Originaltransfer laden
// - beteiligte Konten ermitteln
// - Vorbedingungen prüfen
// - Gegenbuchungen ausführen
// - Rückbuchung fachlich erzeugen
// - Original und Rückbuchung miteinander verknüpfen
// - alles in einer Transaktion speichern
//
// Lernziele:
// - Studierende verstehen, dass eine Rückbuchung fachlich meist als neue Gegenbuchung
//   modelliert wird und nicht als Löschen historischer Daten.
// - Sie lernen, dass ein Use Case mehrere Aggregate bzw. Entitäten koordinieren kann,
//   ohne selbst das Domain Model zu ersetzen.
// - Sie erkennen, wie wichtig fachliche Vorbedingungen sind, bevor eine Zustandsänderung
//   durchgeführt wird.
// - Sie verstehen die Rolle von Result<T> bei erwartbaren fachlichen Fehlern.
// - Sie lernen, warum IClock, Repository und UnitOfWork wichtige Bausteine für
//   testbare und sauber strukturierte Application-Logik sind.
// - Sie erkennen, dass Nachvollziehbarkeit und Historisierung in Finanzdomänen
//   zentrale Anforderungen an das Modell sind.
// */