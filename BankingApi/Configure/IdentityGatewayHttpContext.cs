using System.Security.Claims;
using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._3_Infrastructure._3_Security;

namespace BankingApi.Configure;

public sealed class IdentityGatewayHttpContext(
   IHttpContextAccessor accessor
) : IIdentityGateway {

   private ClaimsPrincipal? User => accessor.HttpContext?.User;

   public string Subject =>
      User?.FindFirstValue(IdentityClaims.Subject)
      ?? throw new InvalidOperationException("Missing claim: sub");

   public string Username =>
      User?.FindFirstValue(IdentityClaims.PreferredUsername)
      ?? throw new InvalidOperationException("Missing claim: preferred_username");

   public DateTimeOffset CreatedAt {
      get {
         var v = User?.FindFirstValue(IdentityClaims.CreatedAt);
         return DateTimeOffset.TryParse(v, out var dt)
            ? dt
            : throw new InvalidOperationException("Missing claim: created_at");
      }
   }

   public int AdminRights =>
      int.TryParse(User?.FindFirstValue(IdentityClaims.AdminRights), out var adminRights)
         ? adminRights
         : throw new InvalidOperationException("Missing or invalid claim: admin_rights");
}