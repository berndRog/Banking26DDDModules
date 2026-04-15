using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._3_Domain.Errors;
using Microsoft.Extensions.Logging;
namespace BankingApi._2_Core.Employees._2_Application.UseCases;

/// <summary>
/// Use case: Set or change admin rights (ST-3).
///
/// Flow:
/// 1) Load employee aggregate (tracked)
/// 2) Apply domain operation (SetAdminRights)
/// 3) Commit via UnitOfWork
///
/// Notes:
/// - Rights are represented as an int bitmask (flags).
/// - Validation of the bitmask (allowed bits) should live in the domain.
/// </summary>
public sealed class EmployeeUcSetAdminRights(
   IEmployeeRepository repository,
   IUnitOfWork unitOfWork,
   IClock clock,
   ILogger<EmployeeUcSetAdminRights> logger
) {

   public async Task<Result> ExecuteAsync(
      Guid employeeId,
      AdminRights adminRights,
      CancellationToken ct
   ) {
      if (employeeId == Guid.Empty) 
         return Result.Failure(EmployeeErrors.InvalidId)
            .LogIfFailure(logger, "EmployeeUcSetAdminRights.InvalidId", new { employeeId });

      var employee = await repository.FindByIdAsync(employeeId, ct);
      if (employee is null) 
         return Result.Failure(EmployeeErrors.NotFound)
            .LogIfFailure(logger, "EmployeeUcSetAdminRights.NotFound", new { employeeId });

      var result = employee.SetAdminRights(
         adminRights: adminRights, 
         updatedAt: clock.UtcNow
      );
      if (result.IsFailure) 
         return result.LogIfFailure(logger, "EmployeeUcSetAdminRights.DomainRejected",
            new { employeeId, adminRights });

      await unitOfWork.SaveAllChangesAsync("Employee admin rights updated", ct);
      
      logger.LogInformation("EmployeeUcSetAdminRights done employeeId={employeeId}", employeeId);
      return Result.Success();
   }
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Hinweise (für alle EmployeeUc*)
 * =====================================================================
 *
 * Was sind die konkreten UseCases (EmployeeUcCreate/Deactivate/SetAdminRights)?
 * --------------------------------------------------------------------------
 * Das sind die eigentlichen Implementierungen der WRITE-Anwendungsfälle
 * im Employees-Bounded-Context. Jeder UseCase:
 * - lädt Aggregate über ein Repository (Tracking)
 * - ruft eine Domain-Operation auf (reine Fachlogik)
 * - persistiert über UnitOfWork (ein Commit)
 *
 *
 * Warum drei getrennte Klassen?
 * -----------------------------
 * - Single Responsibility: jeder UseCase hat genau einen Zweck
 * - bessere Testbarkeit (pro UseCase eigene Unit-Tests)
 * - saubere Struktur der Application Layer
 *
 *
 * Logging-Policy:
 * --------------
 * - Fachliche Fehler (Result-Failure) werden über LogIfFailure geloggt
 * - Technische Fehler (Exceptions) gehören in Middleware / globale Handler
 * - CancellationToken wird nur durchgereicht, nicht als Fehler behandelt
 *
 *
 * Abgrenzung:
 * ----------
 * - Lesen (Details/Liste/Filter) ist kein UseCase → IEmployeeReadModel
 * - Persistenzdetails (EF Core, SQL) sind nicht hier → Infrastructure
 * - Fachliche Regeln/Validierung liegen im Domain Model → Employee Aggregate
 *
 * =====================================================================
 */

