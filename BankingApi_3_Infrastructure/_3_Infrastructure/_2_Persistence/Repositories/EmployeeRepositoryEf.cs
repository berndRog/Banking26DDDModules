using System.Runtime.CompilerServices;
using BankingApi._2_Core.BuildingBlocks._3_Domain.Enums;
using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._3_Domain.Entities;
using BankingApi._3_Infrastructure._2_Persistence.Database;
using Microsoft.EntityFrameworkCore;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Repositories;

internal sealed class EmployeeRepositoryEf(
   AppDbContext dbContext
) : IEmployeeRepository {

   public async Task<Employee?> FindByIdAsync(
      Guid customerId, 
      CancellationToken ct
   ) => await dbContext.Employees
         .FirstOrDefaultAsync(o => o.Id == customerId, ct);

   public async Task<Employee?> FindByIdentitySubjectAsync(
      string subject,
      CancellationToken ct
   ) => await dbContext.Employees
         .FirstOrDefaultAsync(c => c.Subject == subject, ct);
   
   public async Task<Employee?> FindByEmailAsync(
      EmailVo emailVo,
      CancellationToken ct
   ) => await dbContext.Employees
         .FirstOrDefaultAsync(c => c.EmailVo == emailVo, ct);
   
   public async Task<Employee?> FindByPersonnelNumberAsync(
      string personnelNumber,
      CancellationToken ct
   ) => await dbContext.Employees
      .FirstOrDefaultAsync(e => e.PersonnelNumber == personnelNumber, ct);
   
   public async Task<IReadOnlyList<Employee>> SelectAdminsAsync(CancellationToken ct) 
      => await dbContext.Employees
         .Where(e => e.AdminRights != AdminRights.None)
         .OrderBy(e => e.Lastname)
         .ToListAsync(ct);

   public void Add(Employee employee) 
      => dbContext.Employees.Add(employee);

   public void AddRange(IEnumerable<Employee> employees) 
      => dbContext.Employees.AddRange(employees);
}