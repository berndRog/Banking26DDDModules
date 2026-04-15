using Asp.Versioning;
using BankingApi._1_Controllers.Extensions;
using BankingApi._2_Core.Payments._1_Ports.Inbound;
using BankingApi._2_Core.Payments._1_Ports.Outbound;
using BankingApi._2_Core.Payments._2_Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
namespace BankingApi._1_Controllers.V2;

[ApiVersion("2.0")]
[Route("banking/v{version:apiVersion}")]
[ApiController]
public sealed class TransfersController(
   ITransferReadModel transferReadModel,
   ITransferUseCases transferUseCases,
   ILogger<TransfersController> logger
) : ControllerBase {

   /// <summary>
   /// Send money from a given account.
   /// </summary>
   /// <remarks>
   /// This endpoint creates a new transfer and returns <c>201 Created</c> on success.
   /// The response contains the created transfer resource and a Location header
   /// pointing to the newly created transfer.
   /// </remarks>
   /// <param name="accountId">Unique identifier of the sender account.</param>
   /// <param name="sendMoneyDto">Transfer data required to execute the money transfer.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The created transfer resource.</returns>
   // [Authorize(Policy = "CustomersOrEmployees")]
   [HttpPost("accounts/{accountId:guid}/transfers", Name = nameof(SendMoneyAsync))]
   [Consumes("application/json")]
   [Produces("application/json")]
   [ProducesResponseType<TransferDto>(StatusCodes.Status201Created)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<TransferDto>> SendMoneyAsync(
      [FromRoute] Guid accountId,
      [FromBody] SendMoneyDto sendMoneyDto,
      CancellationToken ct
   ) {
      const string context = $"{nameof(TransfersController)}.{nameof(SendMoneyAsync)}";

      var result = await transferUseCases.SendMoneyAsync(
         sendMoneyDto: sendMoneyDto,
         ct: ct
      );

      return this.ToCreatedAtRoute(
         routeName: nameof(GetTransferByAccountIdAndTransferIdAsync),
         routeValues: new {
            accountId = accountId,
            id = result.Value.Id
         },
         result,
         logger,
         context,
         args: new { accountId, sendMoneyDto.Id }
      );
   }
   
   /// <summary>
   /// Returns all transfers of a given account.
   /// </summary>
   /// <remarks>
   /// The account identifier refers to the account whose transfer history is queried.
   /// Depending on the domain rules, the returned transfers may represent outgoing,
   /// incoming, or all booked transfers visible from the perspective of this account.
   /// </remarks>
   /// <param name="accountId">Unique identifier of the account.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>A collection of transfers belonging to the given account.</returns>
   // [Authorize(Policy = "CustomersOrEmployees")]
   [HttpGet("accounts/{accountId:guid}/transfers", Name = nameof(GetTransfersByAccountIdAsync))]
   [ProducesResponseType<IReadOnlyList<TransferDto>>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<IReadOnlyList<TransferDto>>> GetTransfersByAccountIdAsync(
      [FromRoute] Guid accountId,
      CancellationToken ct = default
   ) {
      const string context = $"{nameof(TransfersController)}.{nameof(GetTransfersByAccountIdAsync)}";

      var result = await transferReadModel.SelectTransfersByAccountIdAsync(accountId, ct);

      return this.ToActionResult(result, logger, context, args: new { accountId });
   }

   /// <summary>
   /// Returns a single transfer by account id and transfer id.
   /// </summary>
   /// <remarks>
   /// The account id identifies the account from whose perspective the transfer is queried.
   /// The transfer id identifies the concrete transfer resource.
   /// </remarks>
   /// <param name="accountId">Unique identifier of the account.</param>
   /// <param name="id">Unique identifier of the transfer.</param>
   /// <param name="ct">Cancellation token.</param>
   /// <returns>The transfer resource if found.</returns>
   //[Authorize(Policy = "CustomersOrEmployees")]
   [HttpGet("accounts/{accountId:guid}/transfers/{id:guid}", Name = nameof(GetTransferByAccountIdAndTransferIdAsync))]
   [ProducesResponseType<TransferDto>(StatusCodes.Status200OK)]
   [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
   public async Task<ActionResult<TransferDto>> GetTransferByAccountIdAndTransferIdAsync(
      [FromRoute] Guid accountId,
      [FromRoute] Guid id,
      CancellationToken ct = default
   ) {
      const string context = $"{nameof(TransfersController)}.{nameof(GetTransferByAccountIdAndTransferIdAsync)}";

      var result = await transferReadModel.FindTransferByAccountIdAndTransferIdAsync(accountId, id, ct);

      return this.ToActionResult(result, logger, context, args: new { accountId, id });
   }


}