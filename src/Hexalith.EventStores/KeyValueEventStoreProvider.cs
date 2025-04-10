// <copyright file="EventStoreProvider.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores;

using System;
using System.Diagnostics.CodeAnalysis;

using Hexalith.Commons.Metadatas;
using Hexalith.KeyValueStorages;

public class KeyValueEventStoreProvider : IEventStoreProvider
{
    private readonly IKeyValueProvider _storage;

    public KeyValueEventStoreProvider([NotNull] IKeyValueProvider storage)
    {
        ArgumentNullException.ThrowIfNull(storage);
        _storage = storage;
    }

    /// <inheritdoc/>
    public IEventStore GetStream(string name, string id)
        => new KeyValueEventStore(_storage.Create<long, EventState>(name, id));

    public IEventStore GetStream(Metadata metadata)
        => new KeyValueEventStore(_storage.Create<long, EventState>(
            metadata.Context.PartitionId,
            $"{metadata.Message.Domain.Name}.{metadata.Message.Domain.Id}"));
}