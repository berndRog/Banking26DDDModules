using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Employees._3_Domain.Entities;
using BankingApi._3_Infrastructure._2_Persistence.Database.Converter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// falls Email/Address/Phone hier liegen

namespace BankingApi._3_Infrastructure._2_Persistence.Configurations;

public sealed class ConfigEmployee(
   DateTimeOffsetToIsoStringConverter dtConv,
   DateTimeOffsetToIsoStringConverterNullable dtConvNul
) : IEntityTypeConfiguration<Employee> {

   public void Configure(EntityTypeBuilder<Employee> builder) {

      builder.ToTable("Employees");
      
      // Primary Key
      builder.HasKey(x => x.Id);
      builder.Property(x => x.Id).ValueGeneratedNever();
      
      // Profile data
      builder.Property(x => x.Firstname)
         .HasMaxLength(80)
         .IsRequired();
      builder.Property(x => x.Lastname)
         .HasMaxLength(80)
         .IsRequired();
      
      builder.Property(x => x.Subject)
         .HasMaxLength(200)
         .IsRequired();
      builder.HasIndex(x => x.Subject).IsUnique();
      
      // Scalar properties (Employee-specific)
      builder.Property(x => x.PersonnelNumber)
         .HasMaxLength(32)
         .IsRequired();
      builder.HasIndex(x => x.PersonnelNumber).IsUnique();

      // AdminRights enum -> int (SQLite friendly)
      builder.Property(x => x.AdminRights)
         .HasConversion<int>()
         .IsRequired();
      // IsAdmin is computed => not persisted
      builder.Ignore(x => x.IsAdmin);

      builder.Property(x => x.IsActive)
         .IsRequired();
      builder.Property(x => x.CreatedAt)
         .HasConversion(dtConv)
         .IsRequired();
      builder.Property(x => x.DeactivatedAt)
         .HasConversion(dtConvNul)
         .IsRequired(false);
      
      builder.Property(e => e.EmailVo)
         .HasConversion(vo => vo.Value, s => EmailVo.FromPersisted(s))
         .IsRequired()
         .HasColumnName("Email") // Die Spalte heißt "Email"
         .HasMaxLength(254);
      builder.HasIndex(c => c.EmailVo).IsUnique();
      
      // Phone
      builder.Property(e => e.PhoneVo)
         .HasConversion(vo => vo.Value, s => PhoneVo.FromPersisted(s))
         .IsRequired()
         .HasColumnName("Phone") // Die Spalte heißt "Email"
         .HasMaxLength(64);
   }
}