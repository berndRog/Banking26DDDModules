using BankingApi._2_Core.BuildingBlocks;
using BankingApi._2_Core.BuildingBlocks._3_Domain;
using BankingApi._2_Core.Customers._2_Application.Dtos;
using BankingApi._2_Core.Customers._3_Domain.Enum;

namespace BankingApi._2_Core.Customers._1_Ports.Inbound;

// Application port defining all command use cases for the Customers bounded context.
// Represents the write side of the application (CQRS command side).
// Used by API controllers to trigger state changes in the Customer domain.
public interface ICustomerUseCases {

   // Create a fully initialized customer
   // And also create the first account
   Task<Result<CustomerDto>> CreateAsync(
      CustomerCreateDto customerCreateDto,
      CancellationToken ct = default
   );

   // Provision a customer on first login using identity data plus required business profile data.
   Task<Result<CustomerProvisionDto>> CreateProvisionAsync(
      CustomerDto customerDto,
      CancellationToken ct = default
   );

   // Update the customer profile after provisioning to add missing data (name, address etc.)
   Task<Result<CustomerDto>> UpdateProfileAsync(
      CustomerDto dto,
      CancellationToken ct = default
   );
   
   // Employee action: activate a customer after review of the registration details
   // and create the first account during activation
   Task<Result> ActivateAsync(
      Guid customerId,
      string? accountId,
      string? iban,
      decimal? balance,
      CancellationToken ct = default
   );

   // Employee action: reject a customer registration
   Task<Result> RejectAsync(
      Guid customerId,
      CustomerRejectCode customerRejectCode,
      CancellationToken ct = default
   );

   // Employee action: deactivate an existing customer
   Task<Result> DeactivateAsync(
      Guid customerId,
      CancellationToken ct = default
   );
   
   // Change the customer's profile data
   Task<Result> UpdateAsync(
      CustomerDto customerDto,
      CancellationToken ct = default
   );
}

/*
Didaktik
--------

ICustomerUseCases ist der Inbound Port für alle
zustandsverändernden Anwendungsfälle im Customers-Bounded-Context.

Dieses Interface gehört zur WRITE-Seite der Anwendung
(CQRS Command Side).

Controller rufen diese Methoden auf, um fachliche Aktionen
auszuführen, die den Zustand eines Customer Aggregates ändern.

Typische Anwendungsfälle:

- Customer registrieren
- Customer provisionieren (z.B. durch Identity-System)
- Customer-Profil vervollständigen
- Customer aktivieren oder ablehnen
- Customer deaktivieren

Ein wichtiger Aspekt ist die Trennung zwischen:

Provisioning
→ Identity-Daten plus verpflichtende Profildaten inklusive Adresse

Profile Update
→ Änderung der vollständigen Kundendaten (Name, Adresse etc.)

Dadurch wird der typische Onboarding-Prozess sauber modelliert.


Lernziele
---------

- Verständnis von UseCase-Ports in Clean Architecture
- Trennung zwischen Commands und Queries (CQRS light)
- Modellierung von Onboarding-Prozessen
- Trennung von Provisioning und Profilpflege
- Entkopplung von API und Domainlogik
*/