// <copyright file="KeyValueEventStore.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.KeyValueStorages;

/// <summary>
/// Represents an event store that uses a key-value store for persistence.
/// </summary>
public class KeyValueEventStore : IEventStore
{
    private readonly IKeyValueStore<long, EventState> _keyValueStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValueEventStore"/> class.
    /// </summary>
    /// <param name="keyValueStore">The key-value store to use for event persistence.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="keyValueStore"/> is null.</exception>
    public KeyValueEventStore([NotNull] IKeyValueStore<long, EventState> keyValueStore)
    {
        ArgumentNullException.ThrowIfNull(keyValueStore);
        _keyValueStore = keyValueStore;
    }

    /// <inheritdoc/>
    public Task<long> AddAsync(IEnumerable<EventMessage> items, CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<long> AddAsync(IEnumerable<EventMessage> items, long expectedVersion, CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task ClearSnapshotAsync(long version, CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task ClearSnapshotsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<EventMessage>> GetAsync(bool useSnapshot, CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<EventMessage>> GetAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<EventMessage>> GetAsync(long version, bool useSnapshot, CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<EventMessage>> GetSliceAsync(long first, long last, CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<long> LastSnapshotVersionAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task SnapshotAllAsync(int chunkSize, Func<IEnumerable<EventMessage>, EventMessage> snapshot, CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task SnapshotAsync(long version, EventMessage snapshot, CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<IEnumerable<long>> SnapshotVersionsAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<long> VersionAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}