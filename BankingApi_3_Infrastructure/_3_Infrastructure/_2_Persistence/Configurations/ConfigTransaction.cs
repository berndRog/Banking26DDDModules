using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using BankingApi._3_Infrastructure._2_Persistence.Database.Converter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingApi._3_Infrastructure._2_Persistence.Configurations;

public sealed class ConfigTransaction(
   DateTimeOffsetToIsoStringConverter dtConv
) : IEntityTypeConfiguration<Transaction> {
   public void Configure(EntityTypeBuilder<Transaction> builder) {
      builder.ToTable("Transactions");

      // key
      builder.HasKey(t => t.Id);
      builder.Property(t => t.Id)
         .ValueGeneratedNever()
         .HasColumnName("Id")
         .HasColumnOrder(0);
      
      // debit / credit
      builder.Property(t => t.Type)
         .HasConversion<int>()
         .HasColumnName("Type")
         .HasColumnOrder(1)
         .IsRequired();

      // business data
      builder.Property(t => t.Purpose)
         .HasMaxLength(200)
         .HasColumnName("Purpose")
         .HasColumnOrder(2);
      
      builder.ComplexProperty(x => x.AmountVo, money => {
         money.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .HasColumnName("Amount")
            .HasColumnOrder(3)
            .IsRequired();

         money.Property(x => x.Currency)
            .HasConversion<string>()
            .HasMaxLength(3)
            .HasColumnName("AmCurrency")
            .HasColumnOrder(4)
            .IsRequired();
      });

      builder.ComplexProperty(x => x.BalanceAfterVo, money => {
         money.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .HasColumnName("BalanceAfter")
            .HasColumnOrder(5)
            .IsRequired();

         money.Property(x => x.Currency)
            .HasConversion<string>()
            .HasMaxLength(3)
            .HasColumnName("BaCurrency")
            .HasColumnOrder(6)
            .IsRequired();
      });

      // booking timestamp
      builder.Property(t => t.BookedAt)
         .HasConversion(dtConv)
         .HasColumnName("BookedAt")
         .HasColumnOrder(7)
         .IsRequired();

      // other account
      builder.Property(t => t.OtherAccountName)
         .HasMaxLength(160)
         .HasColumnName("OtherAccountName")
         .HasColumnOrder(8)
         .IsRequired();

      // other account
      builder.Property(t => t.OtherAccountIbanVo)
         .HasConversion(vo => vo.Value, s => IbanVo.FromPersisted(s))
         .IsRequired()
         .HasColumnName("OtherAccountIban")
         .HasColumnOrder(9)
         .HasMaxLength(50);
      builder.HasIndex(c => c.OtherAccountIbanVo);
      
      // account
      builder.Property(t => t.AccountId)
         .HasColumnName("AccountId")
         .HasColumnOrder(10)
         .IsRequired();
      
      // optional reference to transfer aggregate
      builder.Property(t => t.TransferId)
         .HasColumnName("TransferId")
         .HasColumnOrder(11)
         .IsRequired(false);
      

      // query indexes
      builder.HasIndex(t => t.AccountId);
      builder.HasIndex(t => t.TransferId);
      builder.HasIndex(t => t.BookedAt);
      builder.HasIndex(t => new { t.AccountId, t.BookedAt });
   }
}

/*
Didaktik und Lernziele

Die EF-Core-Konfiguration muss die aktuelle Struktur der Domänenklasse exakt
abbilden. Wenn sich das Domänenmodell ändert, muss die Persistenzkonfiguration
mitgezogen werden.

Bei Transaction sind zwei Punkte besonders wichtig:

1. TransferId ist optional
   Nicht jede Transaction muss zwingend zu einem Transfer gehören.
   Deshalb ist TransferId nullable und darf in der Konfiguration nicht als
   IsRequired() modelliert werden.

2. BalanceAfterVo ist ein eigener Value Object
   Nach jeder Buchung soll der Kontostand nach der Buchung gespeichert werden.
   Deshalb muss neben AmountVo auch BalanceAfterVo explizit gemappt werden.

Das Modell zeigt außerdem sehr gut die Trennung zwischen:
- fachlichem Geschäftsvorfall Transfer
- kontobezogener Buchung Transaction

Die Transaction gehört fachlich zum Account-Aggregate, kann aber optional eine
Referenz auf den übergeordneten Transfer tragen. Dadurch werden fachliche
Zusammenhänge für Queries, Audit und Rückbuchungen leichter nachvollziehbar.
*/