using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._3_Infrastructure._2_Persistence;
using BankingApi._3_Infrastructure._2_Persistence.Database;
namespace BankingApi;

public static class SeedDatabase {
   public static async Task EmployeeDataAsync(IServiceProvider serviceProvider) {
      using var scope = serviceProvider.CreateScope();
      var services = scope.ServiceProvider;
      var db = services.GetRequiredService<AppDbContext>();
      var unitOfWork = services.GetRequiredService<IUnitOfWork>();
      var clock = services.GetRequiredService<IClock>();

      // Ensure database is created
      await db.Database.EnsureCreatedAsync();

      // Seed if empty
      if (!db.Employees.Any()) {
         var seed = new Seed(clock);
         var employees = seed.Employees;
         db.Employees.AddRange(employees);
         await unitOfWork.SaveAllChangesAsync("Seed Employees");
         unitOfWork.ClearChangeTracker();
      }
   }

   public static async Task AllDataAsync(IServiceProvider serviceProvider) {
      using var scope = serviceProvider.CreateScope();
      var services = scope.ServiceProvider;
      var db = services.GetRequiredService<AppDbContext>();
      var unitOfWork = services.GetRequiredService<IUnitOfWork>();
      var clock = services.GetRequiredService<IClock>();

      // Ensure database is created
      await db.Database.EnsureCreatedAsync();

      // Seed if empty
      if (!db.Customers.Any()) {
         var seed = new Seed(clock);

         // var employees = seed.Employees;
         // db.Employees.AddRange(employees);
         // unitOfWork.LogChangeTracker("Seeding Employees");
         //
         // await unitOfWork.SaveAllChangesAsync("Seed Employees");
         unitOfWork.ClearChangeTracker();

         db.Customers.AddRange(seed.Customers);
         await unitOfWork.SaveAllChangesAsync("Seed Customers");

         var accounts = seed.AddBeneficiariesToAccounts();
         db.Accounts.AddRange(accounts);
         await unitOfWork.SaveAllChangesAsync("Seed Accounts");

         db.Transactions.AddRange(seed.Transactions);
         await unitOfWork.SaveAllChangesAsync("Seed Transactions");

         db.Transfers.AddRange(seed.Transfers);
         await unitOfWork.SaveAllChangesAsync("");
      }
   }
}