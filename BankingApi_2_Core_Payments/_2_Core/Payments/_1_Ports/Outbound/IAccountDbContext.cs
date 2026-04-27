using BankingApi._2_Core.Payments._3_Domain.Entities;
namespace BankingApi._2_Core.Payments._1_Ports.Outbound;

// Persistence context abstraction for the Accounts part of the Payments context.
// Provides query access to Account aggregates and their Beneficiaries.
// Used by repositories to interact with the database without exposing EF Core.
public interface IAccountDbContext {

   // Query access to Account aggregates
   IQueryable<Account> Accounts { get; }

   // Query access to beneficiaries belonging to accounts
   IQueryable<Beneficiary> Beneficiaries { get; }
   
   // Query access to transactions belonging to accounts
   IQueryable<Transaction> Transactions { get; }
   
   // Add a new entity to the persistence context
   void Add<T>(T entity) where T : class;
   void AddRange<T>(IEnumerable<T> entities) where T : class;
   
   // Update (mark as modified) an existing entity in the persistence context
   void Update<T>(T entity) where T : class;

   // Remove an entity from the persistence context
   void Remove(Beneficiary b);
}

/*
Didaktik
--------

Dieses Interface abstrahiert den Datenbankzugriff für
das Account-Aggregate im Payments-Bounded-Context.

Der Fokus liegt auf dem Aggregate:

Account
└── Beneficiary

Repositories greifen über dieses Interface auf die
Persistenz zu und laden oder speichern Aggregate.

Wichtig:

- Das Interface ist eine technische Abstraktion
- Die konkrete Implementierung liegt in der Infrastructure
- Typischerweise wird hier ein EF-Core-DbContext verwendet

Dadurch bleibt das Domainmodell unabhängig von der
Persistenztechnologie.


Lernziele
---------

- Verständnis eines Persistence Ports
- Trennung zwischen Domainmodell und ORM-Technologie
- Abbildung von Aggregates auf Persistence Interfaces
- Entkopplung von EF Core durch ein Interface
*/