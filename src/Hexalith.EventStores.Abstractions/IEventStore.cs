// <copyright file="IEventStore.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores;

/// <summary>
/// Persisted event store stream interface.
/// </summary>
public interface IEventStore : IAsyncDisposable
{
    /// <summary>
    /// Add new items to the event stream.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new stream version.</returns>
    Task<long> AddAsync(IEnumerable<EventMessage> items, CancellationToken cancellationToken);

    /// <summary>
    /// Add new items to the event stream and verify the version.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <param name="expectedVersion">The expected stream version.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The new stream version.</returns>
    Task<long> AddAsync(IEnumerable<EventMessage> items, long expectedVersion, CancellationToken cancellationToken);

    /// <summary>
    /// Clear the snapshot of the event stream at a specific version.
    /// </summary>
    /// <param name="version">The version of the snapshot to clear.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task.</returns>
    Task ClearSnapshotAsync(long version, CancellationToken cancellationToken);

    /// <summary>
    /// Clear the snapshot of the event stream.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task.</returns>
    Task ClearSnapshotsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets all stream items.
    /// </summary>
    /// <param name="useSnapshot">if set to <c>true</c> use a snapshot to avoid replaying all events.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of all items.</returns>
    Task<IEnumerable<EventMessage>> GetAsync(bool useSnapshot, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all stream items. Snapshots will be used if available to avoid replaying all events.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of all items.</returns>
    Task<IEnumerable<EventMessage>> GetAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets all stream items at a specific version.
    /// </summary>
    /// <param name="version">The version of the event stream to retrieve.</param>
    /// <param name="useSnapshot">if set to <c>true</c> use a snapshot to avoid replaying all events.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The list of all items at the specified version.</returns>
    Task<IEnumerable<EventMessage>> GetAsync(long version, bool useSnapshot, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a stream items slice.
    /// </summary>
    /// <param name="first">first item to retrieve.</param>
    /// <param name="last">last item to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stream data slice.</returns>
    Task<IEnumerable<EventMessage>> GetSliceAsync(long first, long last, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the version of the last snapshot.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns the version of the last snapshot.</returns>
    Task<long> LastSnapshotVersionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Take snapshots of the event stream at regular intervals. Sets a snapshot of the event stream to avoid replaying all events. The application is responsible for managing the snapshot.
    /// </summary>
    /// <param name="chunkSize">The number of events between each snapshot.</param>
    /// <param name="snapshot">The application function to calculate state of the event stream at the given version.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task.</returns>
    Task SnapshotAllAsync(int chunkSize, Func<IEnumerable<EventMessage>, EventMessage> snapshot, CancellationToken cancellationToken);

    /// <summary>
    /// Take a snapshot of the event stream. Sets a snapshot of the event stream to avoid replaying all events. The application is responsible for managing the snapshot.
    /// </summary>
    /// <param name="version">The version of the event stream of the snapshot.</param>
    /// <param name="snapshot">The calculated state of the event stream at the given version.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task.</returns>
    Task SnapshotAsync(long version, EventMessage snapshot, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the event stream version of the snapshot.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns the snapshot version.</returns>
    Task<IEnumerable<long>> SnapshotVersionsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the event stream version.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns the current version of the event stream.</returns>
    Task<long> VersionAsync(CancellationToken cancellationToken);
}