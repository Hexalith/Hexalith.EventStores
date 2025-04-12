# Hexalith.EventStores.Example

This example application demonstrates how to use Hexalith.EventStores to implement event sourcing for a simple bank account application.

## Overview

The application showcases:
- Creating and using an event store with file-based persistence
- Implementing domain events with polymorphic serialization
- Adding events to the store with proper sequencing
- Retrieving and displaying events from the store

## Project Dependencies

The example relies on the following packages:
- `Hexalith.EventStores` - Core event sourcing functionality
- `Hexalith.KeyValueStorages.Files` - File-based storage implementation
- `Hexalith.PolymorphicSerializations` - For polymorphic event serialization/deserialization
- `Hexalith.PolymorphicSerializations.CodeGenerators` - For generating serialization code

## Domain Model

The application models a simple bank account domain with the following events:

1. `BankAccountEvent` - Base abstract record for all bank account events
2. `BankAccountCreated` - Event for account creation with initial details
3. `FundsDeposited` - Event for depositing funds into an account
4. `FundsWithdrawn` - Event for withdrawing funds from an account

All events use C# 9+ record types with primary constructors and are decorated with the `[PolymorphicSerialization]` attribute for proper serialization.

## Code Example

Here's how the example demonstrates event store usage:

```csharp
// Setup the event store with file-based storage
var keyValueProvider = new JsonFileKeyValueProvider(options: PolymorphicHelper.DefaultJsonSerializerOptions);
var provider = new KeyValueEventStoreProvider(keyValueProvider);
IEventStore store = await provider.OpenStoreAsync("BankAccount", "123456000", CancellationToken.None);

// Add events to the store
await store.AddAsync(
    [
    new BankAccountCreated("1500530001", "Joe Dalton", "EUR").CreateMessage(),
    new FundsDeposited("1500530001", 1500, DateTimeOffset.Now).CreateMessage(),
    ],
    CancellationToken.None);

// Retrieve and display events
foreach (EventMessage message in await store.GetAsync(CancellationToken.None))
{
    Console.WriteLine(JsonSerializer.Serialize(message.Event, PolymorphicHelper.DefaultJsonSerializerOptions));
}
```

## Key Features Demonstrated

1. **Event Registration**: The example registers polymorphic mappers for correct serialization/deserialization
2. **Event Store Creation**: Creating a file-based event store for a specific aggregate
3. **Adding Events**: Adding single and multiple events in a transaction
4. **Event Sequencing**: Demonstrating sequence verification to ensure event order integrity
5. **Event Retrieval**: Retrieving and displaying all events from the store

## Running the Example

To run the example:
1. Ensure .NET 9.0 SDK is installed
2. Build the solution
3. Run the Hexalith.EventStores.Example project

The example will create a bank account, perform various transactions, and display the event history.
