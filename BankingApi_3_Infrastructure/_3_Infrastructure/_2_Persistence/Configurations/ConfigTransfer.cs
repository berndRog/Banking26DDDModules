using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.ValueObjects;
using BankingApi._3_Infrastructure._2_Persistence.Database.Converter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingApi._3_Infrastructure._2_Persistence.Configurations;

public sealed class ConfigTransfer(
   DateTimeOffsetToIsoStringConverter dtConv
) : IEntityTypeConfiguration<Transfer> {

   public void Configure(EntityTypeBuilder<Transfer> builder) {
      builder.ToTable("Transfers");

      // key
      builder.HasKey(t => t.Id);
      builder.Property(t => t.Id)
         .ValueGeneratedNever()
         .HasColumnName("Id")
         .HasColumnOrder(0);
      
      // business fields
      builder.Property(t => t.Purpose)
         .HasMaxLength(80)
         .HasColumnName("Purpose")
         .HasColumnOrder(1)
         .IsRequired();

      // amount value object
      builder.ComplexProperty(a => a.AmountVo, money => {
         money.Property(m => m.Amount)
            .HasColumnName("Amount")
            .HasColumnOrder(2)
            .HasPrecision(18, 2)
            .IsRequired();

         money.Property(m => m.Currency)
            .HasColumnName("Currency")
            .HasColumnOrder(3)
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsRequired();
      });
      
      // account references
      builder.Property(t => t.DebitAccountId)
         .HasColumnName("DebitAccountId")
         .HasColumnOrder(4)
         .IsRequired();
      builder.HasIndex(t => t.DebitAccountId);
      
      builder.Property(a => a.CreditAccountIbanVo)
         .HasConversion(vo => vo.Value, s => IbanVo.FromPersisted(s))
         .IsRequired()
         .HasColumnName("CreditAccountIban")
         .HasColumnOrder(5)
         .HasMaxLength(50);
      builder.HasIndex(c => c.CreditAccountIbanVo);
      
      // status and booking time
      builder.Property(t => t.Status)
         .HasConversion<int>()
         .HasColumnName("Status")
         .HasColumnOrder(6)
         .IsRequired();

      builder.Property(t => t.BookedAt)
         .HasConversion(dtConv)
         .HasColumnName("BookedAt")
         .HasColumnOrder(7)
         .IsRequired();

      // transaction references
      builder.Property(t => t.DebitTransactionId)
         .HasColumnName("DebitTransactionId")
         .HasColumnOrder(8)
         .IsRequired();
      builder.HasIndex(t => t.DebitTransactionId);

      builder.Property(t => t.CreditTransactionId)
         .HasColumnName("CreditTransactionId")
         .HasColumnOrder(9)
         .IsRequired();
      builder.HasIndex(t => t.CreditTransactionId);

      // reversal relation
      builder.Property(t => t.ReversedByTransferId)
         .HasColumnName("ReversedByTransferId")
         .HasColumnOrder(10)
         .IsRequired(false);
      builder.HasIndex(t => t.ReversedByTransferId)
         .IsUnique();
      
      // audit fields
      builder.Property(t => t.CreatedAt)
         .HasConversion(dtConv)
         .HasColumnName("CreatedAt")
         .HasColumnOrder(11)
         .IsRequired();

      builder.Property(t => t.UpdatedAt)
         .HasConversion(dtConv)
         .HasColumnName("UpdatedAt")
         .HasColumnOrder(12)
         .IsRequired();

      builder.HasIndex(t => t.BookedAt);
   }
}