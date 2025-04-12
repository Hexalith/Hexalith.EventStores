// <copyright file="KeyValueEventStoreProvider.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using Hexalith.Commons.Configurations;
using Hexalith.Commons.Metadatas;
using Hexalith.EventStores.Configurations;
using Hexalith.KeyValueStorages;

using Microsoft.Extensions.Options;

/// <summary>
/// Provides an implementation of <see cref="IEventStoreProvider"/> that uses a key-value storage provider.
/// </summary>
public class KeyValueEventStoreProvider : IEventStoreProvider
{
    private const string _snapshotIndexSuffix = "__Snapshot_Index";
    private const string _snapshotSuffix = "__Snapshot";
    private readonly string _defaultDatabase;
    private readonly TimeSpan _openTimeout;
    private readonly TimeSpan _sessionTimeout;
    private readonly IKeyValueProvider _storage;
    private readonly TimeProvider? _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValueEventStoreProvider"/> class.
    /// </summary>
    /// <param name="options">The options for the event store settings.</param>
    /// <param name="storage">The key-value storage provider.</param>
    /// <param name="timeProvider">The time provider for generating timestamps.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="storage"/> is null.</exception>
    public KeyValueEventStoreProvider([NotNull] IOptions<EventStoreSettings> options, [NotNull] IKeyValueProvider storage, TimeProvider? timeProvider)
    {
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentNullException.ThrowIfNull(options);
        SettingsException<EventStoreSettings>.ThrowIfUndefined(options.Value.DefaultOpenTimeout);
        SettingsException<EventStoreSettings>.ThrowIfUndefined(options.Value.DefaultSessionTimeout);
        SettingsException<EventStoreSettings>.ThrowIfUndefined(options.Value.DefaultDatabase);
        _storage = storage;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _openTimeout = options.Value.DefaultOpenTimeout;
        _sessionTimeout = options.Value.DefaultSessionTimeout;
        _defaultDatabase = options.Value.DefaultDatabase;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyValueEventStoreProvider"/> class.
    /// </summary>
    /// <param name="storage">The key-value storage provider.</param>
    /// <param name="timeProvider">The time provider for generating timestamps.</param>
    public KeyValueEventStoreProvider([NotNull] IKeyValueProvider storage, TimeProvider? timeProvider = null)
        : this(new OptionsWrapper<EventStoreSettings>(new EventStoreSettings()), storage, timeProvider)
    {
    }

    /// <inheritdoc/>
    public async Task<IEventStore> OpenStoreAsync(
        string database,
        string name,
        string id,
        CancellationToken cancellationToken)
    {
        var store = new KeyValueEventStore(
            _storage.Create<long, EventState>(database, name, id),
            _storage.Create<long, EventState>(database + _snapshotSuffix, name, id),
            _storage.Create<string, State<IEnumerable<long>>>(database + _snapshotIndexSuffix, name, id),
            _timeProvider);
        await store.OpenAsync(_sessionTimeout, _openTimeout, cancellationToken).ConfigureAwait(false);
        return store;
    }

    /// <inheritdoc/>
    public async Task<IEventStore> OpenStoreAsync(Metadata metadata, CancellationToken cancellationToken)
    {
        IKeyValueStore<long, EventState> eventStore = _storage.Create<long, EventState>(
                metadata.Context.PartitionId,
                metadata.Message.Domain.Name,
                metadata.Message.Domain.Id);
        IKeyValueStore<long, EventState> snapShotStore = _storage.Create<long, EventState>(
                metadata.Context.PartitionId + _snapshotSuffix,
                metadata.Message.Domain.Name,
                metadata.Message.Domain.Id);
        IKeyValueStore<string, State<IEnumerable<long>>> snapshotCollectionStore = _storage.Create<string, State<IEnumerable<long>>>(
                metadata.Context.PartitionId + _snapshotIndexSuffix,
                metadata.Message.Domain.Name,
                metadata.Message.Domain.Id);
        var store = new KeyValueEventStore(eventStore, snapShotStore, snapshotCollectionStore, _timeProvider);
        await store.OpenAsync(_sessionTimeout, _openTimeout, cancellationToken).ConfigureAwait(false);
        return store;
    }

    /// <inheritdoc/>
    public Task<IEventStore> OpenStoreAsync(string name, string id, CancellationToken cancellationToken)
        => OpenStoreAsync(
            _defaultDatabase,
            name,
            id,
            cancellationToken);
}