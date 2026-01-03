// <copyright file="Program.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Text.Json;

using Hexalith.EventStores;
using Hexalith.EventStores.Example;
using Hexalith.EventStores.Example.Extensions;
using Hexalith.KeyValueStorages.Files;
using Hexalith.PolymorphicSerializations;

// Register the polymorphic mappers generated for the application events
HexalithEventStoresExampleSerialization.RegisterPolymorphicMappers();

const string BankAccountId = "1500530001";

JsonFileKeyValueProvider keyValueProvider = new(options: PolymorphicHelper.DefaultJsonSerializerOptions);

KeyValueEventStoreProvider provider = new(keyValueProvider);

IEventStore store = await provider.OpenStoreAsync("BankAccount", "123456000", CancellationToken.None).ConfigureAwait(false);

// Open a bank account with a 1500 EUR deposit
await store.AddAsync(
    [
    new BankAccountCreated(BankAccountId, "Joe Dalton", "EUR").CreateMessage(),
    new FundsDeposited(BankAccountId, 1500, DateTimeOffset.Now).CreateMessage(),
    ],
    CancellationToken.None).ConfigureAwait(false);

// Withdraw 500 EUR
await store.AddAsync(
    [new FundsWithdrawn(BankAccountId, 500, DateTimeOffset.Now).CreateMessage()],
    CancellationToken.None).ConfigureAwait(false);

// Deposit 1000 EUR
await store.AddAsync(
    [new FundsDeposited(BankAccountId, 1000, DateTimeOffset.Now).CreateMessage()],
    CancellationToken.None).ConfigureAwait(false);

// Withdraw 500 EUR with a sequence verification. Will fail if the sequence is not correct.
// The expected sequence in the message is the last sequence number in the store + 1.
await store.AddAsync(
    [new FundsWithdrawn(BankAccountId, 500, DateTimeOffset.Now).CreateMessage(5)],
    CancellationToken.None).ConfigureAwait(false);

// Write the store events to the console
foreach (EventMessage message in await store.GetAsync(CancellationToken.None).ConfigureAwait(false))
{
    Console.WriteLine(JsonSerializer.Serialize(message.Event, PolymorphicHelper.DefaultJsonSerializerOptions));
}