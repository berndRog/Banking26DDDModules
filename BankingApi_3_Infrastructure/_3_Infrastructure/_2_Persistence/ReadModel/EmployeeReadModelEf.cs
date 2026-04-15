using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._2_Application.Dtos;
using BankingApi._2_Core.Employees._2_Application.Mappings;
using BankingApi._2_Core.Employees._3_Domain.Errors;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.ReadModel;
internal sealed class EmployeeReadModelEf(
   IEmployeesDbContext employeesDbContext,
   IIdentityGateway identityGateway
) : IEmployeeReadModel {

   public async Task<Result<Guid>> FindMeProvisionedAsync(CancellationToken ct) {

      // subject required
      var subjectResult = SubjectCheck.Run(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<Guid>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // idempotent lookup (no tracking)
      var id = await employeesDbContext.Employees
         .AsNoTracking()
         .Where(o => o.Subject == subject)  // filter by subject
         .Select(o => o.Id)                 // project to Id only (map)
         .SingleOrDefaultAsync(ct);

      return id == Guid.Empty 
         ? Result<Guid>.Failure(EmployeeErrors.NotProvisioned) 
         : Result<Guid>.Success(id);
   }
   
   public async Task<Result<EmployeeDto>> FindMeAsync(CancellationToken ct) {
      
      // 1) Subject from Gateway
      var subjectResult = SubjectCheck.Run(identityGateway.Subject);
      if (subjectResult.IsFailure)
         return Result<EmployeeDto>.Failure(subjectResult.Error);
      var subject = subjectResult.Value;

      // 2) load Employee by subject (NO tracking, read-only)
      var employeeDto = await employeesDbContext.Employees
         .AsNoTracking()
         .Where(c => c.Subject == subject)   // filter by subject
         .Select(c => c.ToEmployeeDto())     // project to EmployeeDto (map)
         .SingleOrDefaultAsync(ct);
      
      return employeeDto is null
         ? Result<EmployeeDto>.Failure(EmployeeErrors.NotProvisioned)   
         : Result<EmployeeDto>.Success(employeeDto);
   }
   
   public async Task<Result<EmployeeDto>> FindByIdAsync(
      Guid id,
      CancellationToken ct
   ) {
      var employeeDto = await employeesDbContext.Employees
         .AsNoTracking()
         .Where(c => c.Id == id)  // filter by Id
         .Select(c => c.ToEmployeeDto())  // project to CustomerDto (map)
         .SingleOrDefaultAsync(ct);

      return employeeDto is null
         ? Result<EmployeeDto>.Failure(EmployeeErrors.NotFound)
         : Result<EmployeeDto>.Success(employeeDto);
   }
   
   public async Task<Result<EmployeeDto>> FindByEmailAsync(
      string email,
      CancellationToken ct
   ) {
      var resultEmail = EmailVo.Create(email);
      if (resultEmail.IsFailure)
         return Result<EmployeeDto>.Failure(resultEmail.Error);
      var emailVo = resultEmail.Value;
      
      var employeeDto = await employeesDbContext.Employees
         .AsNoTracking()
         .Where(c => c.EmailVo == emailVo)   // filter by email
         .Select(c => c.ToEmployeeDto()) // projection
         .SingleOrDefaultAsync( ct);
      
      return employeeDto is null
         ? Result<EmployeeDto>.Failure(EmployeeErrors.NotFound)
         : Result<EmployeeDto>.Success(employeeDto);
   }
   
   public async Task<Result<IEnumerable<EmployeeDto>>> SelectByNameAsync(
      string name,
      CancellationToken ct = default
   ) {
      var pattern = $"%{name}%";
      var employeeDtos = await employeesDbContext.Employees
         .Where(c =>
            EF.Functions.Like(
               c.Firstname + " " + c.Lastname,
               pattern))
         .Select(c => c.ToEmployeeDto())
         .ToListAsync(ct);
      return Result<IEnumerable<EmployeeDto>>.Success(employeeDtos);
   }
   
   public async Task<Result<IEnumerable<EmployeeDto>>> SelectAllAsync(
      CancellationToken ct
   ) {
      var ownerDtos = await employeesDbContext.Employees
         .AsNoTracking()
         .Select(c => c.ToEmployeeDto()) // project to CustomerDto (map)
         .ToListAsync(ct);
      return Result<IEnumerable<EmployeeDto>>.Success(ownerDtos);
   }
}
