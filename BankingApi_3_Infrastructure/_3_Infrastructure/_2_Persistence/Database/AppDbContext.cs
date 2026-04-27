using System.Runtime.CompilerServices;
using BankingApi._2_Core.Customers._3_Domain.Entities;
using BankingApi._2_Core.Employees._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._3_Infrastructure._2_Persistence.Configurations;
using BankingApi._3_Infrastructure._2_Persistence.Database.Converter;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Database;

public sealed class AppDbContext(
   DbContextOptions<AppDbContext> options
) : DbContext(options) {
   public DbSet<Employee> Employees => Set<Employee>();
   public DbSet<Customer> Customers => Set<Customer>();
   public DbSet<Account> Accounts => Set<Account>();
   public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
   public DbSet<Transfer> Transfers => Set<Transfer>();
   public DbSet<Transaction> Transactions => Set<Transaction>();

   protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);

      // ------------------------------------------------------------
      // Reuse converter instances (stateless, deterministic).
      // This keeps mapping code explicit without pushing converters
      // into DI just for EF.
      // ------------------------------------------------------------
      var dtConv = new DateTimeOffsetToIsoStringConverter();
      var dtConvNul = new DateTimeOffsetToIsoStringConverterNullable();
      // ------------------------------------------------------------
      // Apply entity mappings (aggregate roots first).
      // ------------------------------------------------------------
      modelBuilder.ApplyConfiguration(new ConfigCustomer(dtConv, dtConvNul));
      modelBuilder.ApplyConfiguration(new ConfigEmployee(dtConv, dtConvNul));
      
      modelBuilder.ApplyConfiguration(new ConfigAccount(dtConv));
      modelBuilder.ApplyConfiguration(new ConfigBeneficiary());
      
      modelBuilder.ApplyConfiguration(new ConfigTransfer(dtConv));
      modelBuilder.ApplyConfiguration(new ConfigTransaction(dtConv));
   }

   /*
   ==========================================================
   Didaktik & Lernziele (Deutsch)
   ==========================================================

   1) Composition Root für Persistenz-Konfiguration
   -----------------------------------------------
   OnModelCreating dient als zentraler Ort, an dem alle EF-Core-
   Konfigurationen zusammenlaufen. Das entspricht dem Prinzip
   "Composition Root": hier wird verdrahtet, nicht in Domain oder
   Application.

   → Lernziel: Verstehen, dass Mapping/Infrastructure im
      Composition Root zusammengeführt wird.

   2) Konverter: explizit, aber ohne DI
   -----------------------------------
   Die ValueConverter sind zustandslos und werden nur von EF genutzt.
   Wir erzeugen eine Instanz und reichen sie an mehrere Configs weiter.

   → Lernziel: Technische Hilfsklassen müssen nicht immer in DI.
      Explizite Konstruktion kann einfacher und didaktisch klarer sein.

   3) Wiederverwendung und Konsistenz
   ---------------------------------
   Alle AggregateRoots verwenden dieselbe Konverter-Instanz.
   Damit ist garantiert, dass Zeitstempel (CreatedAt/UpdatedAt/
   DeactivatedAt) überall identisch persistiert werden.

   → Lernziel: Konsistenz ist wichtiger als "schöne" Einzel-Lösungen.

   4) Aggregate Root vs Child Entity
   ---------------------------------
   AggregateRoots (Customer, Account) werden separat konfiguriert.
   Child Entities (Beneficiary) können eine eigene EF-Config haben,
   ohne die Aggregate-Grenze fachlich zu verletzen.

   → Lernziel: Aggregate-Grenzen sind Domain-Regeln, nicht die Anzahl
      der EF-Konfigurationsklassen.

   ==========================================================
   */
}