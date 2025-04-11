// <copyright file="KeyValueEventStoreProvider.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores;

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using Hexalith.Commons.Metadatas;
using Hexalith.KeyValueStorages;

/// <summary>
/// Provides an implementation of <see cref="IEventStoreProvider"/> that uses a key-value storage provider.
/// </summary>
public class KeyValueEventStoreProvider : IEventStoreProvider
{
    private static readonly ConcurrentDictionary<string, Lock> _locks = new();
    private readonly IKeyValueProvider _storage;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValueEventStoreProvider"/> class.
    /// </summary>
    /// <param name="storage">The key-value storage provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="storage"/> is null.</exception>
    public KeyValueEventStoreProvider([NotNull] IKeyValueProvider storage)
    {
        ArgumentNullException.ThrowIfNull(storage);
        _storage = storage;
    }

    /// <summary>
    /// Gets an event store for the specified database, name, and identifier.
    /// </summary>
    /// <param name="database">The database name.</param>
    /// <param name="name">The container name.</param>
    /// <param name="id">The entity identifier.</param>
    /// <returns>An instance of <see cref="IEventStore"/>.</returns>
    public IEventStore GetStore(string database, string name, string id)
        => new KeyValueEventStore(_storage.Create<long, EventState>(database, name, id, GetLock(database, name, id)));

    /// <inheritdoc/>
    public IEventStore GetStore(string name, string id)
        => new KeyValueEventStore(_storage.Create<long, EventState>(null, name, id));

    /// <summary>
    /// Gets an event store for the specified metadata.
    /// </summary>
    /// <param name="metadata">The metadata containing context and domain information.</param>
    /// <returns>An instance of <see cref="IEventStore"/>.</returns>
    public IEventStore GetStore(Metadata metadata)
        => new KeyValueEventStore(_storage.Create<long, EventState>(
            metadata.Context.PartitionId,
            metadata.Message.Domain.Name,
            metadata.Message.Domain.Id));
    }

    private Lock GetLock(string database, string container, string entity)
    {
        string lockId = $"{entity}/{container}/{database}";
        if (_locks.TryGetValue(lockId, out Lock? lockInfo))
        {
            if (lockInfo.Expiration > DateTimeOffset.UtcNow)
            {
                lockInfo.Lock.Exit();
            }
        }

        _locks[lockId] = (new Lock(), DateTimeOffset.UtcNow.Add(_timeout));
    }
}