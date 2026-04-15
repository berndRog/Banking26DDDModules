using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Employees._3_Domain.Entities;

namespace BankingApi._2_Core.Employees._1_Ports.Outbound;

// Repository port for accessing Employee aggregates.
// Used by application use cases to load and persist employees.
// Implemented in the Infrastructure layer (e.g. EF Core).
public interface IEmployeeRepository {

   // Queries (0..1)
   // Load an employee aggregate by identifier
   Task<Employee?> FindByIdAsync(
      Guid id,
      CancellationToken ct = default
   );

   // Load employee using the identity subject (IdP reference)
   Task<Employee?> FindByIdentitySubjectAsync(
      string subject,
      CancellationToken ct = default
   );

   // Load employee by email value object
   Task<Employee?> FindByEmailAsync(
      EmailVo emailVo,
      CancellationToken ct = default
   );

   // Load employee by personnel number
   Task<Employee?> FindByPersonnelNumberAsync(
      string personnelNumber,
      CancellationToken ct = default
   );

   // Queries (0..n)
   Task<IReadOnlyList<Employee>> SelectAdminsAsync(
      CancellationToken ct
   );

   // Commands
   void Add(Employee employee);
   void AddRange(IEnumerable<Employee> employees);
}

/* =====================================================================
 * Deutsche Architektur- und Didaktik-Erläuterung
 * =====================================================================
 *
 * Was ist IEmployeeRepository?
 * ----------------------------
 * IEmployeeRepository ist das Repository-Interface für das
 * Employee-Aggregate im Employees-Bounded-Context.
 *
 * Es kapselt den Zugriff auf Employee-Aggregate und liefert
 * TRACKING-Entitäten (z.B. EF Core), damit UseCases Zustandsänderungen
 * am Aggregate durchführen und danach speichern können.
 *
 *
 * Warum ExistsPersonnelNumberAsync / ExistsEmailAsync?
 * ----------------------------------------------------
 * Diese beiden Methoden werden im UseCase "Employee anlegen" benötigt,
 * um fachliche Eindeutigkeitsregeln durchzusetzen:
 * - PersonnelNumber muss eindeutig sein
 * - Email muss eindeutig sein
 *
 * Alternative wäre:
 * - FindByPersonnelNumberAsync / FindByEmailAsync
 * aber Exists* ist oft effizienter (nur bool, keine Entity).
 *
 *
 * Was ist IEmployeeRepository NICHT?
 * ----------------------------------
 * - Kein ReadModel (keine DTOs, keine Projektionen, kein Paging)
 * - Kein UseCase (keine Orchestrierung)
 * - Kein Domain Service
 *
 * Für Listen/Filter/Details gilt:
 * → IEmployeeReadModel (AsNoTracking + Projektionen)
 *
 *
 * Abgrenzung zu anderen Schichten:
 * --------------------------------
 * - Fachliche Regeln & Zustandsautomaten:
 *   → Employee Aggregate (Domain Layer)
 *
 * - Persistenzdetails (EF Core):
 *   → Infrastructure (EmployeeRepositoryEf)
 *
 * - Lesen/Listen/Filtern:
 *   → Application ReadModel
 *
 * =====================================================================
 */