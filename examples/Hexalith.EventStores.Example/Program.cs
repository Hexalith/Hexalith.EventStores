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

var keyValueProvider = new JsonFileKeyValueProvider(options: PolymorphicHelper.DefaultJsonSerializerOptions);

var provider = new KeyValueEventStoreProvider(keyValueProvider);

IEventStore store = provider.GetStore("BankAccount", "123456000");

// Open a bank account with a 1500 EUR deposit
await store.AddAsync(
    [
    new BankAccountCreated("1500530001", "Joe Dalton", "EUR").CreateMessage(),
    new FundsDeposited("1500530001", 1500, DateTimeOffset.Now).CreateMessage(),
    ],
    CancellationToken.None);

// Withdraw 500 EUR
await store.AddAsync(
    [new FundsWithdrawn("1500530001", 500, DateTimeOffset.Now).CreateMessage()],
    CancellationToken.None);

// Deposit 1000 EUR
await store.AddAsync(
    [new FundsDeposited("1500530001", 1000, DateTimeOffset.Now).CreateMessage()],
    CancellationToken.None);

// Withdraw 500 EUR with a sequence verification. Will fail if the sequence is not correct.
// The expected sequence in the message is the last sequence number in the store + 1.
await store.AddAsync(
    [new FundsWithdrawn("1500530001", 500, DateTimeOffset.Now).CreateMessage(5)],
    CancellationToken.None);

// Write the store events to the console
foreach (EventMessage message in await store.GetAsync(CancellationToken.None))
{
    Console.WriteLine(JsonSerializer.Serialize(message.Event, PolymorphicHelper.DefaultJsonSerializerOptions));
}