using System.Runtime.CompilerServices;
using BankingApi._2_Core.Employees._1_Ports.Outbound;
using BankingApi._2_Core.Employees._3_Domain.Entities;
[assembly: InternalsVisibleTo("BankingApiTest")]
namespace BankingApi._3_Infrastructure._2_Persistence.Database;

internal sealed class EmployeeDbContextEf(
   AppDbContext db
) : IEmployeesDbContext {

   public IQueryable<Employee> Employees 
      => db.Set<Employee>();
  
   public void Add(Employee employee) 
      => db.Set<Employee>().Add(employee);
   public void AddRange(IEnumerable<Employee> employees) 
      => db.Set<Employee>().AddRange(employees);

   public void Update(Employee employee) 
      => db.Set<Employee>().Update(employee);
   
}