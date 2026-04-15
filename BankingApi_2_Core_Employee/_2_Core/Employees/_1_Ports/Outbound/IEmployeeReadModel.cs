using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.Employees._2_Application.Dtos;
namespace BankingApi._2_Core.Employees._1_Ports.Outbound;

// Read model interface for querying employee data.
// Used by the application layer to retrieve employee information
// without exposing the domain model or persistence details.
// Returns DTOs because this is a query/read use case.
public interface IEmployeeReadModel {
 
   // Returns the currently authenticated employee
   Task<Result<EmployeeDto>> FindMeAsync(
      CancellationToken ct = default
   );
   
   // Find employee by technical identifier
   Task<Result<EmployeeDto>> FindByIdAsync(
      Guid id, 
      CancellationToken ct = default
   );
   
   // Find employee by email address
   Task<Result<EmployeeDto>> FindByEmailAsync(
      string email, 
      CancellationToken ct = default
   );
   
   // Find all employees by %name$ with SQL Like
   public Task<Result<IEnumerable<EmployeeDto>>> SelectByNameAsync(
      string name,
      CancellationToken ct = default
   );
      
   // Return all employees
   Task<Result<IEnumerable<EmployeeDto>>> SelectAllAsync(
      CancellationToken ct = default
   );
}

/*
Didaktik
--------

Dieses Interface beschreibt ein ReadModel im Sinne von
Ports & Adapters / Clean Architecture.

Ein ReadModel wird für Abfragen (Queries) verwendet und liefert
DTOs statt Domain-Objekten zurück.

Wichtige Eigenschaften:

- Das Domain Model bleibt verborgen.
- Die Application Layer arbeitet nur mit diesem Port.
- Die konkrete Implementierung liegt in der Infrastructure.

Ein ReadModel ist bewusst von Repositories getrennt:

Repository
- arbeitet mit Aggregates
- wird für Commands / Domain-Logik verwendet

ReadModel
- arbeitet mit DTOs
- optimiert für Queries / Datenanzeige

Dadurch können Lesezugriffe unabhängig vom Domain Model
optimiert werden.


Lernziele
---------

- Unterschied zwischen Repository und ReadModel verstehen
- Trennung von Command- und Query-Zugriffen (CQRS light)
- Nutzung von Ports zur Entkopplung der Infrastruktur
- Verwendung von DTOs für Leseoperationen
*/
