using System.Security.Claims;
using BankingApi.Configure;
using BankingApi._3_Infrastructure._3_Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
namespace BankingApi;

public static class DiAuthNAuthZ {
   
   public static IServiceCollection AddAuthNAuthZ(
      this IServiceCollection services,
      IConfiguration config
   ) {
      services.AddOptions<AuthOptions>()
         .Bind(config.GetSection("AuthServer")) 
         .Validate(o => !string.IsNullOrWhiteSpace(o.Authority), "AuthServer:Authority is required.")
         .ValidateOnStart();

      var auth = config.GetSection("AuthServer").Get<AuthOptions>()
         ?? throw new InvalidOperationException("Missing configuration section 'AuthServer'.");

      Console.WriteLine($"JWT Bearer Authority: {auth.Authority}");
      Console.WriteLine($"JWT Bearer Audience: {auth.Audience}");
      Console.WriteLine($"JWT Bearer ValidateAudience: {auth.ValidateAudience}");
      Console.WriteLine($"JWT Bearer RequireHttpsMetadata: {auth.RequireHttpsMetadata}");
      Console.WriteLine($"JWT Bearer ClockSkewSeconds: {auth.ClockSkewSeconds}");

      //--- AuthN JWT Bearer --------------------------------------------------------------------
      services
         .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
         .AddJwtBearer(opt => {
            opt.Authority = auth.Authority;
            opt.RequireHttpsMetadata = auth.RequireHttpsMetadata;
            opt.MapInboundClaims = false;

            if (!string.IsNullOrWhiteSpace(auth.Audience))
               opt.Audience = auth.Audience;

            opt.TokenValidationParameters = new TokenValidationParameters {
               ValidateAudience = auth.ValidateAudience,
               ClockSkew = TimeSpan.FromSeconds(auth.ClockSkewSeconds),
               NameClaimType = IdentityClaims.PreferredUsername,
               RoleClaimType = "role"
            };

            opt.Events = new JwtBearerEvents {
               OnAuthenticationFailed = ctx => {
                  var log = ctx.HttpContext.RequestServices
                     .GetRequiredService<ILoggerFactory>()
                     .CreateLogger("JWT");
                  log.LogError(ctx.Exception, "JWT auth failed");
                  return Task.CompletedTask;
               },
               OnChallenge = ctx => {
                  var log = ctx.HttpContext.RequestServices
                     .GetRequiredService<ILoggerFactory>()
                     .CreateLogger("JWT");
                  log.LogWarning("JWT challenge: error={Error}, desc={Desc}",
                     ctx.Error, ctx.ErrorDescription);
                  return Task.CompletedTask;
               }
            };
         });
      
      //--- AuthZ -------------------------------------------------------------------------------
      services.AddAuthorization(options => {
         // Customer/employee separation is expressed by the identity claim `admin_rights`
         // and still accepts the test auth handler roles for integration tests.
         options.AddPolicy("CustomersOnly", p => p.RequireAssertion(ctx =>
            IsCustomer(ctx.User)));
         options.AddPolicy("EmployeesOnly", p => p.RequireAssertion(ctx =>
            IsEmployee(ctx.User)));
         options.AddPolicy("CustomersOrEmployees", p => p.RequireAssertion(ctx =>
            IsCustomer(ctx.User) || IsEmployee(ctx.User)));
      });

      return services;
   }

   private static bool IsCustomer(ClaimsPrincipal user) =>
      user.Identity?.IsAuthenticated == true &&
      (user.IsInRole("Customer") || HasAdminRights(user, rights => rights == 0));

   private static bool IsEmployee(ClaimsPrincipal user) =>
      user.Identity?.IsAuthenticated == true &&
      (user.IsInRole("Employee") || HasAdminRights(user, rights => rights > 0));

   private static bool HasAdminRights(ClaimsPrincipal user, Func<int, bool> predicate) {
      var raw = user.FindFirstValue(IdentityClaims.AdminRights);
      return raw is not null
         && int.TryParse(raw, out var rights)
         && predicate(rights);
   }
}
