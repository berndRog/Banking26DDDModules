using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._2_Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
namespace BankingApi._2_Core.Payments;

public static class DiPaymentModule {
   
   public static IServiceCollection AddPaymentModule(
      this IServiceCollection services
   ) {
      // Inbound ports / Use Cases
      services.AddScoped<AccountUcCreate>();
      services.AddScoped<AccountUcDeactivate>();
      services.AddScoped<AccountUcBeneficiaryAdd>();
      services.AddScoped<AccountUcBeneficiaryRemove>();
      services.AddScoped<IAccountUseCases, AccountUseCases>();      
      
      services.AddScoped<TransferUcSendMoney>();
      //services.AddScoped<TransferUcReverse>();
      services.AddScoped<ITransferUseCases, TransferUseCases>();      
      
      // Policies
      return services;
   }
}