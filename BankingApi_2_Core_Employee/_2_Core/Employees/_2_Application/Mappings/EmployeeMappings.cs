using BankingApi._2_Core.BuildingBlocks._4_BcContracts._2_Application.Dtos;
using BankingApi._2_Core.Employees._2_Application.Dtos;
using BankingApi._2_Core.Employees._3_Domain.Entities;
namespace BankingApi._2_Core.Employees._2_Application.Mappings;

public static class EmployeeMappings {

   public static EmployeeCreateDto ToEmployeeCreateDto(this Employee employee) => new(
      Id: employee.Id,
      Firstname: employee.Firstname,
      Lastname: employee.Lastname,
      Email: employee.EmailVo.Value,
      Phone: employee.PhoneVo.Value,
      Subject: employee.Subject,
      PersonnelNumber: employee.PersonnelNumber,
      IsActive: employee.IsActive,
      AdminRightsInt: (int) employee.AdminRights
   );
   
   public static EmployeeDto ToEmployeeDto(this Employee employee) => new(
      Id: employee.Id,
      Firstname: employee.Firstname,
      Lastname: employee.Lastname,
      Email: employee.EmailVo.Value,
      Phone: employee.PhoneVo.Value,
      PersonnelNumber: employee.PersonnelNumber,
      IsActive: employee.IsActive,
      AdminRightsInt: (int) employee.AdminRights
   );
   
   public static EmployeeProvisionDto ToEmployeeProvisionDto(this Employee employee) => new(
      Id: employee.Id,
      WasCreated: true
   );
   
   public static EmployeeContractDto ToEmployeeContractDto(this Employee employee) => new(
      Id: employee.Id, 
      AdminRightsInt: (int) employee.AdminRights
   );
}
