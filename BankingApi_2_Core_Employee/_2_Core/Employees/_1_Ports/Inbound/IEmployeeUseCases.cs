using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.Employees._2_Application.Dtos;
namespace BankingApi._2_Core.Employees._1_Ports.Inbound;

// Application port defining all command use cases for the Employees bounded context.
// Represents the write side of the application (CQRS command side).
// Used by API controllers to trigger state changes in the Employee domain.
public interface IEmployeeUseCases {

   // Create a new employee aggregate
   // Validates input and persists the new employee
   Task<Result<EmployeeDto>> CreateAsync(
      EmployeeCreateDto employeeCreateDto,
      CancellationToken ct = default
   );

   // Create provisioning information for a new employee
   // Typically used during identity or onboarding workflows
   Task<Result<EmployeeProvisionDto>> CreateProvisionAsync(
      string? id,
      CancellationToken ct = default
   );

   // Update profile information of an existing employee
   Task<Result<EmployeeDto>> UpdateProfileAsync(
      EmployeeDto dto,
      CancellationToken ct = default
   );

   // Deactivate an employee and prevent further administrative actions
   Task<Result> DeactivateAsync(
      Guid employeeId,
      DateTimeOffset deactivatedAt,
      CancellationToken ct = default
   );

   // Grant or update administrative rights
   // Rights are represented as a flag enum
   Task<Result> SetAdminRightsAsync(
      Guid employeeId,
      AdminRights adminRights,
      CancellationToken ct = default
   );
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist IEmployeeUseCases?
 * --------------------------
 * IEmployeeUseCases ist der Inbound Port (Eingangsschnittstelle)
 * für alle zustandsverändernden Anwendungsfälle im
 * Employees-Bounded-Context.
 *
 * Es handelt sich um die WRITE-Seite des Systems (CQRS).
 *
 *
 * Welche User Stories werden hier abgebildet?
 * -------------------------------------------
 * - EM-1: Employee anlegen
 *   → CreateAsync
 *
 * - EM-2: Employee deaktivieren
 *   → DeactivateAsync
 *
 * - ST-3: AdminRights setzen / ändern
 *   → SetAdminRightsAsync
 *
 *
 * Was gehört bewusst NICHT hierher?
 * ---------------------------------
 * - Anzeigen von Employee-Details
 * - Anzeigen von Listen
 * - Filtern / Suchen
 *
 * Diese Fälle gehören in das ReadModel:
 * → IEmployeeReadModel
 *
 *
 * Warum eine UseCase-Schnittstelle?
 * ---------------------------------
 * - Controller hängen nur von Interfaces ab
 * - Klare Trennung zwischen API und Anwendungslogik
 * - Einfache Austauschbarkeit der Implementierung
 *
 *
 * Abgrenzung zu anderen Schichten:
 * --------------------------------
 * - Fachliche Regeln & Invarianten:
 *   → Employee Aggregate (Domain Layer)
 *
 * - Persistenz:
 *   → EmployeeRepository (Infrastructure)
 *
 * - Lesen / UI-Abfragen:
 *   → EmployeeReadModel
 *
 *
 * Merksatz:
 * ---------
 * UseCases verändern Zustand.
 * ReadModels liefern Projektionen.
 *
 * =====================================================================
 */