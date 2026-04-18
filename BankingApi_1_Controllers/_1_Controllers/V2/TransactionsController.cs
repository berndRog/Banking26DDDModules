using Asp.Versioning;
using BankingApi._1_Controllers.Extensions;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
namespace BankingApi._1_Controllers.V2;

[ApiVersion("2.0")]
[Route("banking/v{version:apiVersion}")]
[ApiController]
public sealed class TransactionsController(
   IAccountReadModel readModel,
   ILogger<TransactionsController> logger
) : ControllerBase {

   /// <summary>
   /// Returns a transaction by Ids (accountId and transferId).
   /// </summary>
   /// <param name="accountId">Unique identifier of the account.</param>
   /// <param name="id">Unique identifier of the transaction.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The transaction resource if found.</returns>
   // [Authorize]
   [HttpGet("accounts/{accountId:guid}/transactions/{id:guid}", Name = nameof(GetTransactionByAccountIdAndByTransactionIdAsync))]
   [Produces("application/json")]
   [ProducesResponseType<TransactionDetailDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<TransactionDetailDto>> GetTransactionByAccountIdAndByTransactionIdAsync(
      [FromRoute] Guid accountId,
      [FromRoute] Guid id,
      CancellationToken ct
   ) {
      const string context = $"{nameof(TransactionsController)}.{nameof(GetTransactionByAccountIdAndByTransactionIdAsync)}";

      var result = await readModel.FindTransactionByAccountIdAndTransactionIdAsync(accountId, id, ct);

      return this.ToActionResult(result, logger, context, args: new { id });
   }
   
   /// <summary>
   /// Returns a transaction by Ids (accountId and transferId).
   /// </summary>
   /// <param name="accountId">Unique identifier of the account.</param>
   /// <param name="fromUtc">ISO 8061 start date</param>
   /// <param name="toUtc">ISO 8061 end date</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The transaction resources if found.</returns>
   // [Authorize]
   [HttpGet("accounts/{accountId:guid}/transactions", Name = nameof(SelectTransactionByAccountIdAndTimeperiodAsync))]
   [Produces("application/json")]
   [ProducesResponseType<IEnumerable<TransactionDetailDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<IEnumerable<TransactionDetailDto>>> SelectTransactionByAccountIdAndTimeperiodAsync(
      [FromRoute] Guid accountId,
      [FromQuery] DateTimeOffset fromUtc,
      [FromQuery] DateTimeOffset toUtc,
      CancellationToken ct
   ) {
      const string context = $"{nameof(TransactionsController)}.{nameof(SelectTransactionByAccountIdAndTimeperiodAsync)}";

      var result = await readModel.SelectTransactionsByAccountIdAndPeriodAsync(
         accountId: accountId,
         fromUtc: fromUtc,
         toUtc: toUtc,
         ct: ct
      );

      return this.ToActionResult(result, logger, context, args: null);
   }

   
   
}
