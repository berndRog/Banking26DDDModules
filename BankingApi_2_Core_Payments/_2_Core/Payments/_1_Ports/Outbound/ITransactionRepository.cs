using BankingApi._2_Core.Payments._3_Domain.Entities;
namespace BankingApi._2_Core.Payments._1_Ports.Outbound;

// Repository port for accessing Transaction entities.
// Used by application use cases to retrieve and persist transactions.
// Implemented in the Infrastructure layer (e.g. EF Core).
public interface ITransactionRepository {



   // Add a new transaction to the persistence context
   void Add(Transaction transaction);
}

/*
Didaktik
--------

Dieses Interface beschreibt das Repository für Transactions
im Payments-Bounded-Context.

Transactions repräsentieren einzelne Buchungen auf einem Konto
(z.B. Debit oder Credit).

Das Repository kapselt den Zugriff auf diese Entitäten und stellt
fachlich sinnvolle Abfragen bereit, z.B.:

- Laden einer einzelnen Transaktion
- Laden aller Transaktionen eines Kontos
- Laden von Transaktionen innerhalb eines Zeitraums

Die konkrete Implementierung befindet sich in der Infrastructure
(z.B. EF Core Repository).

Wichtig:

Das Repository arbeitet mit Domain-Objekten (Transaction)
und nicht mit DTOs.


Lernziele
---------

- Rolle eines Repositories im Domain Model verstehen
- Strukturierung von Datenzugriff nach fachlichen Abfragen
- Trennung zwischen Domainmodell und Persistenztechnologie
- Einsatz von Ports zur Entkopplung der Infrastruktur
*/