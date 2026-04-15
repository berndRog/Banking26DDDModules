namespace BankingApi._2_Core.Payments._3_Domain.Enums;

public enum TransferStatus {
   Initiated = 1,   // angelegt
   Booked = 2,      // Debit + Credit erfolgreich
   Failed = 3,      // endgültig gescheitert
   Reversed = 4     // optional: storniert / revidiert
}


