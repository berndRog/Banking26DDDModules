using BankingApi._2_Core.Employees._3_Domain.Entities;
namespace BankingApi._2_Core.Employees._1_Ports.Outbound;

// Database context abstraction used by the Employees module.
// Provides minimal persistence access required by repositories.
// The concrete implementation typically wraps an EF Core DbContext.
public interface IEmployeesDbContext {

   // Query access to Employee aggregates
   IQueryable<Employee> Employees { get; }

   // Add a new entity to the persistence context
   void Add(Employee employee);
   void AddRange(IEnumerable<Employee> employees);

   // Update an entity in the persistence context
   void Update(Employee employee);
}

/*
Didaktik
--------

Dieses Interface abstrahiert den Datenbankzugriff für
den Employees-Bounded-Context.

Es handelt sich nicht um ein Repository, sondern um eine
technische Abstraktion über den konkreten ORM-Context
(z.B. EF Core DbContext).

Repositories verwenden dieses Interface, um auf die
Persistenz zuzugreifen.

Dadurch entsteht eine klare Trennung:

Application / Domain
→ arbeitet mit Repository-Interfaces

Infrastructure
→ implementiert DbContext und Repositories

Der Vorteil dieser Abstraktion:

- Entkopplung von EF Core
- bessere Testbarkeit
- klar definierte Datenzugriffsgrenze


Lernziele
---------

- Unterschied zwischen Repository und DbContext verstehen
- Rolle eines Persistence Ports in Clean Architecture
- Entkopplung der Domain von konkreten ORM-Technologien
- Verbesserung der Testbarkeit durch Abstraktion
*/