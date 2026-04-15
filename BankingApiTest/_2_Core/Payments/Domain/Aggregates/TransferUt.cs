using BankingApi._2_Core.BuildingBlocks._1_Ports.Outbound;
using BankingApi._2_Core.Payments._3_Domain.Entities;
using BankingApi._2_Core.Payments._3_Domain.Enums;
using BankingApiTest.TestInfrastructure;
namespace BankingApiTest._2_Core.Core.Domain.Aggregates;

public sealed class TransferUt {
   private readonly TestSeed _seed;
   private readonly IClock _clock;
   private Account _fromAccount;
   private Account _toAccount;
   private Beneficiary _beneficiary;
   private Transfer _transfer;

   public TransferUt() {
      _seed = new TestSeed();
      _clock = _seed.Clock;
      // Account 1, Beneficary 1, Customer 1
      _fromAccount = _seed.Account1();
      _beneficiary = _seed.Beneficiary1();
      _toAccount = _seed.Account5();
      _transfer = _seed.Transfer1();
   }

   [Fact]
   public void CreateTransfer_valid_input_and_id_creates_transfer() {
      // Arrange

      // Act
      var result = Transfer.CreateBooked(
         debitAccountId: _transfer.DebitAccountId,
         creditAccountIbanVo: _transfer.CreditAccountIbanVo, 
         purpose: _transfer.Purpose,
         amountVo: _transfer.AmountVo,
         debitTransactionId: _transfer.DebitTransactionId,
         creditTransactionId: _transfer.CreditTransactionId,
         bookedAt: _clock.UtcNow,
         id: _transfer.Id.ToString()
      );

      // Assert
      True(result.IsSuccess);
      NotNull(result.Value);

      var actual = result.Value!;
      IsType<Transfer>(actual);
      Equal(_transfer.Id, actual.Id);
      Equal(_transfer.DebitAccountId, actual.DebitAccountId);
      Equal(_transfer.CreditAccountIbanVo, actual.CreditAccountIbanVo);
      Equal(_transfer.Purpose, actual.Purpose);
      Equal(_transfer.AmountVo, actual.AmountVo);
      Equal(_transfer.DebitTransactionId, actual.DebitTransactionId);
      Equal(_transfer.CreditTransactionId, actual.CreditTransactionId);
      Equal(TransferStatus.Booked, actual.Status);
   }

   /*
   [Fact]
   public void Create_without_id_generates_new_id() {
      // Arrange
      // Act
      var result = Transfer.CreateBooked(
         debitAccountId: _transfer.DebitAccountId,
         creditAccountIbanVo: _transfer.CreditAccountIbanVo, 
         purpose: _transfer.Purpose,
         amountVo: _transfer.AmountVo,
         debitTransactionId: _transfer.DebitTransactionId,
         creditTransactionId: _transfer.CreditTransactionId,
         bookedAt: _clock.UtcNow,
         id: null
      );

      // Assert
      True(result.IsSuccess);
      NotNull(result.Value);

      var actual = result.Value!;
      IsType<Transfer>(actual);
      NotEqual(Guid.Empty, actual.Id);
      NotEqual(Guid.Parse(_id), actual.Id);
      Equal(_fromAccount.Id, actual.DebitAccountId);
      Equal(_transfer.AmountVo, actual.AmountVo);
      Equal(_transfer.Purpose, actual.Purpose);
      Equal(TransferStatus.Booked, actual.Status);
   }

   [Fact]
   public void Create_with_invalid_id_fails() {
      // Arrange
      // Act
      var result = Transfer.CreateBooked(
         debitAccountId: _transfer.DebitAccountId,
         creditAccountIbanVo: _transfer.CreditAccountIbanVo, 
         purpose: _transfer.Purpose,
         amountVo: _transfer.AmountVo,
         debitTransactionId: _transfer.DebitTransactionId,
         creditTransactionId: _transfer.CreditTransactionId,
         bookedAt: _clock.UtcNow,
         id: "is-not-a-guid"
      );

      // Assert
      True(result.IsFailure);
      NotNull(result.Error);
   }

   [Fact]
   public void Create_is_deterministic_for_same_input_id() {
      // Act
      var result1 = Transfer.CreateBooked(
         debitAccountId: _transfer.DebitAccountId,
         creditAccountIbanVo: _transfer.CreditAccountIbanVo, 
         purpose: _transfer.Purpose,
         amountVo: _transfer.AmountVo,
         debitTransactionId: _transfer.DebitTransactionId,
         creditTransactionId: _transfer.CreditTransactionId,
         bookedAt: _clock.UtcNow,
         id: _transfer.Id.ToString()
      );
      var result2 = Transfer.CreateBooked(
         debitAccountId: _transfer.DebitAccountId,
         creditAccountIbanVo: _transfer.CreditAccountIbanVo, 
         purpose: _transfer.Purpose,
         amountVo: _transfer.AmountVo,
         debitTransactionId: _transfer.DebitTransactionId,
         creditTransactionId: _transfer.CreditTransactionId,
         bookedAt: _clock.UtcNow,
         id: _transfer.Id.ToString()
      );
      var transfer1 = result1.Value!;
      var transfer2 = result2.Value!;

      // Assert
      True(result1.IsSuccess);
      True(result2.IsSuccess);
      Equal(transfer1.Id, transfer2.Id);
      Equal(transfer1.DebitAccountId, transfer2.DebitAccountId);
      Equal(transfer1.AmountVo, transfer2.AmountVo);
      Equal(transfer1.Purpose, transfer2.Purpose);
      Equal(transfer1.Status, transfer2.Status);
   }
   */
}