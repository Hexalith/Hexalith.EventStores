// <copyright file="KeyValueEventStore.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using Hexalith.Commons.UniqueIds;
using Hexalith.EventStores.Exceptions;
using Hexalith.KeyValueStorages;

/// <summary>
/// Represents an event store that uses a key-value store for persistence.
/// </summary>
public class KeyValueEventStore : IEventStore, IDisposable, IAsyncDisposable
{
    private const string _snapshotVersions = "Versions";
    private static readonly ConcurrentDictionary<string, (string SessionId, DateTimeOffset Expiration)> _storeLocks = [];
    private readonly IKeyValueStore<long, EventState> _eventStore;
    private readonly IKeyValueStore<string, State<IEnumerable<long>>> _snapshotCollectionStore;
    private readonly IKeyValueStore<long, EventState> _snapshotStore;
    private readonly TimeProvider _timeProvider;
    private bool _disposed;
    private string? _sessionId;
    private string? _storeId;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValueEventStore"/> class.
    /// </summary>
    /// <param name="eventStore">The key-value store to use for event persistence.</param>
    /// <param name="snapshotStore">The key-value store to use for snapshot persistence.</param>
    /// <param name="snapshotCollectionStore">The key-value store to use for snapshot collection persistence.</param>
    /// <param name="timeProvider">The time provider to use for generating timestamps.</param>
    public KeyValueEventStore(
        [NotNull] IKeyValueStore<long, EventState> eventStore,
        [NotNull] IKeyValueStore<long, EventState> snapshotStore,
        [NotNull] IKeyValueStore<string, State<IEnumerable<long>>> snapshotCollectionStore,
        TimeProvider? timeProvider)
    {
        ArgumentNullException.ThrowIfNull(eventStore);
        ArgumentNullException.ThrowIfNull(snapshotStore);
        ArgumentNullException.ThrowIfNull(snapshotCollectionStore);

        _eventStore = eventStore;
        _snapshotStore = snapshotStore;
        _snapshotCollectionStore = snapshotCollectionStore;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    private string StoreId => _storeId ??= $"{_eventStore.Entity}/{_eventStore.Container}/{_eventStore.Database}";

    /// <inheritdoc/>
    public async Task<long> AddAsync(IEnumerable<EventMessage> items, CancellationToken cancellationToken)
    {
        CheckSession();
        ArgumentNullException.ThrowIfNull(items);
        long version = await VersionAsync(cancellationToken).ConfigureAwait(false);
        foreach (EventMessage item in items)
        {
            if (item.Metadata.Context.SequenceNumber != null && item.Metadata.Context.SequenceNumber != ++version)
            {
                throw new StoreVersionMismatchException(item.Metadata, version);
            }

            _ = await _eventStore.AddAsync(version, new EventState(item), cancellationToken).ConfigureAwait(false);
        }

        return version;
    }

    /// <inheritdoc/>
    public async Task ClearSnapshotAsync(long version, CancellationToken cancellationToken)
    {
        CheckSession();
        _ = await _snapshotStore
            .RemoveAsync(version, null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ClearSnapshotsAsync(CancellationToken cancellationToken)
    {
        CheckSession();
        State<IEnumerable<long>>? snapshots = await _snapshotCollectionStore.TryGetAsync(_snapshotVersions, cancellationToken).ConfigureAwait(false);
        if (snapshots != null)
        {
            List<Task<bool>> tasks = [];
            tasks.Add(_snapshotCollectionStore.RemoveAsync(_snapshotVersions, null, cancellationToken));
            foreach (long version in snapshots.Value)
            {
                tasks.Add(_snapshotStore.RemoveAsync(version, null, cancellationToken));
            }

            _ = await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public void Close()
    {
        if (!string.IsNullOrEmpty(_sessionId))
        {
            _ = _storeLocks.TryRemove(StoreId, out _);
            _sessionId = null;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventMessage>> GetAsync(CancellationToken cancellationToken)
    {
        CheckSession();
        return await GetAsync(useSnapshot: true, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventMessage>> GetAsync(bool useSnapshot, CancellationToken cancellationToken)
    {
        CheckSession();
        long version = await VersionAsync(cancellationToken).ConfigureAwait(false);
        return await GetAsync(version, useSnapshot, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventMessage>> GetAsync(long version, bool useSnapshot, CancellationToken cancellationToken)
    {
        CheckSession();
        var events = new List<EventMessage>();
        long startVersion = 1;

        if (useSnapshot)
        {
            long snapshotVersion = await LastSnapshotVersionAsync(cancellationToken).ConfigureAwait(false);
            if (snapshotVersion > 0)
            {
                EventState? snapshot = await _snapshotStore.GetAsync(snapshotVersion, cancellationToken).ConfigureAwait(false);
                if (snapshot != null)
                {
                    events.Add(snapshot.Value);
                    startVersion = snapshotVersion + 1;
                }
            }
        }

        for (long i = startVersion; i <= version; i++)
        {
            EventState? state = await _eventStore.GetAsync(i, cancellationToken).ConfigureAwait(false);
            if (state != null)
            {
                events.Add(state.Value);
            }
        }

        return events;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventMessage>> GetSliceAsync(long first, long last, CancellationToken cancellationToken)
    {
        CheckSession();
        var events = new List<EventMessage>();
        for (long i = first; i <= last; i++)
        {
            EventState? state = await _eventStore.GetAsync(i, cancellationToken).ConfigureAwait(false);
            if (state != null)
            {
                events.Add(state.Value);
            }
        }

        return events;
    }

    /// <inheritdoc/>
    public async Task<long> LastSnapshotVersionAsync(CancellationToken cancellationToken)
    {
        CheckSession();
        return (await _snapshotCollectionStore.TryGetAsync(_snapshotVersions, cancellationToken).ConfigureAwait(false))
            ?.Value.Max() ?? 0;
    }

    /// <inheritdoc/>
    public async Task OpenAsync(TimeSpan sessionTimeout, TimeSpan openTimeout, CancellationToken cancellationToken)
    {
        var retryWait = TimeSpan.FromMilliseconds(1);
        TimeSpan totalWait = TimeSpan.Zero;
        while (!LockStore(sessionTimeout))
        {
            if (totalWait < sessionTimeout)
            {
                throw new OpenStoreFailedException(StoreId, sessionTimeout);
            }

            await Task.Delay(retryWait, cancellationToken);
            totalWait += retryWait;
            retryWait += TimeSpan.FromMilliseconds(1);
        }
    }

    /// <inheritdoc/>
    public async Task SnapshotAllAsync(int chunkSize, Func<IEnumerable<EventMessage>, EventMessage> snapshot, CancellationToken cancellationToken)
    {
        CheckSession();
        ArgumentNullException.ThrowIfNull(snapshot);
        if (chunkSize <= 0)
        {
            throw new ArgumentException("Chunk size must be greater than zero.", nameof(chunkSize));
        }

        long version = 0;
        List<EventMessage> events = [];
        List<Task> tasks = [];
        while (true)
        {
            EventState? ev = await _eventStore.TryGetAsync(++version, cancellationToken).ConfigureAwait(false);
            if (ev == null)
            {
                break;
            }

            if (version % chunkSize == 0)
            {
                _ = snapshot(events);
                tasks.Add(SnapshotAsync(version, snapshot(events), cancellationToken));
            }
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task SnapshotAsync(long version, EventMessage snapshot, CancellationToken cancellationToken)
    {
        CheckSession();
        ArgumentNullException.ThrowIfNull(snapshot);
        _ = await _snapshotStore.AddOrUpdateAsync(
            version,
            new EventState(snapshot),
            cancellationToken)
            .ConfigureAwait(false);
        State<IEnumerable<long>>? snapshots = await _snapshotCollectionStore.TryGetAsync(_snapshotVersions, cancellationToken).ConfigureAwait(false);
        if (snapshots == null)
        {
            _ = await _snapshotCollectionStore.AddAsync(
                _snapshotVersions,
                new State<IEnumerable<long>>(snapshots?.Value ?? [], null, null),
                cancellationToken).ConfigureAwait(false);
        }
        else
        {
            _ = await _snapshotCollectionStore.SetAsync(
            _snapshotVersions,
            new State<IEnumerable<long>>(snapshots.Value.Append(version).Distinct().Order(), null, null),
            cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<long>> SnapshotVersionsAsync(CancellationToken cancellationToken)
    {
        CheckSession();
        return (await _snapshotCollectionStore.TryGetAsync(_snapshotVersions, cancellationToken))?.Value ?? [];
    }

    /// <inheritdoc/>
    public async Task<long> VersionAsync(CancellationToken cancellationToken)
    {
        CheckSession();
        long version = 0;
        do
        {
            version++;
        }
        while (await _eventStore.ExistsAsync(version, cancellationToken).ConfigureAwait(false));

        return version - 1;
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Dispose managed state (managed objects)
            if (_eventStore is IDisposable eventStore)
            {
                eventStore.Dispose();
            }

            if (_snapshotStore is IDisposable snapshotStore)
            {
                snapshotStore.Dispose();
            }

            if (_snapshotCollectionStore is IDisposable snapshotCollectionStore)
            {
                snapshotCollectionStore.Dispose();
            }

            Close();
        }

        _disposed = true;
    }

    /// <summary>
    /// Performs asynchronous cleanup of resources.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_disposed)
        {
            return;
        }

        // Dispose async managed state (managed objects)
        if (_eventStore is IAsyncDisposable eventStore)
        {
            await eventStore.DisposeAsync().ConfigureAwait(false);
        }

        if (_snapshotStore is IAsyncDisposable snapshotStore)
        {
            await snapshotStore.DisposeAsync().ConfigureAwait(false);
        }

        if (_snapshotCollectionStore is IAsyncDisposable snapshotCollectionStore)
        {
            await snapshotCollectionStore.DisposeAsync().ConfigureAwait(false);
        }

        // Release the lock on the store if we have a session
        if (!string.IsNullOrEmpty(_sessionId) && !string.IsNullOrEmpty(StoreId))
        {
            _ = _storeLocks.TryRemove(StoreId, out _);
        }

        _disposed = true;
    }

    private void CheckSession()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(KeyValueEventStore));

        if (string.IsNullOrEmpty(_sessionId))
        {
            throw new StoreNotOpenException(StoreId, _sessionId);
        }

        if (_storeLocks.TryGetValue(StoreId, out (string SessionId, DateTimeOffset Expiration) session))
        {
            if (session.Expiration < DateTimeOffset.UtcNow)
            {
                _ = _storeLocks.Remove(StoreId, out _);
                throw new StoreSessionExpiredException(StoreId, session.SessionId, session.Expiration);
            }

            if (session.SessionId != _sessionId)
            {
                throw new InvalidStoreSessionException(StoreId, _sessionId, session.SessionId);
            }
        }
        else
        {
            throw new StoreNotOpenException(StoreId, _sessionId);
        }
    }

    private bool LockStore(TimeSpan lockTimeout)
    {
        if (_storeLocks.TryGetValue(StoreId, out (string SessionId, DateTimeOffset Expiration) session))
        {
            if (session.Expiration < _timeProvider.GetUtcNow())
            {
                return false;
            }

            _ = _storeLocks.Remove(StoreId, out _);
        }

        string sessionId = UniqueIdHelper.GenerateUniqueStringId();
        _storeLocks[StoreId] = (sessionId, _timeProvider.GetUtcNow().Add(lockTimeout));
        _sessionId = sessionId;
        return true;
    }
}