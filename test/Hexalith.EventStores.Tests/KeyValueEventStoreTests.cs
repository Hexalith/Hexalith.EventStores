// <copyright file="KeyValueEventStoreTests.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores.Tests;

using System.Collections.Generic;

using Hexalith.EventStores;
using Hexalith.EventStores.Exceptions;
using Hexalith.KeyValueStorages;

using Moq;

using Shouldly;

/// <summary>
/// Unit tests for the <see cref="KeyValueEventStore"/> class.
/// </summary>
public class KeyValueEventStoreTests
{
    /// <summary>
    /// Tests that adding events increments the version and adds the events to the store.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddAsync_AddsEventsAndReturnsCorrectVersion()
    {
        // Arrange
        var mockTimeProvider = new MockTimeProvider(DateTimeOffset.UtcNow);
        var eventStore = new Mock<IKeyValueStore<long, EventState>>();
        SetupStoreProperties(eventStore, "database", "container", "entity");

        _ = eventStore
            .Setup(s => s.ExistsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((long v, CancellationToken _) => v <= 2);

        _ = eventStore
            .Setup(s => s.AddAsync(It.IsAny<long>(), It.IsAny<EventState>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("123");

        var snapshotStore = new Mock<IKeyValueStore<long, EventState>>();
        var snapshotCollectionStore = new Mock<IKeyValueStore<string, State<IEnumerable<long>>>>();

        var store = new KeyValueEventStore(
            eventStore.Object,
            snapshotStore.Object,
            snapshotCollectionStore.Object,
            mockTimeProvider);

        // Open the store before using it
        await store.OpenAsync(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1), CancellationToken.None);

        var events = new List<EventMessage>
        {
            CreateEventMessage(3),
            CreateEventMessage(4),
        };

        // Act
        long version = await store.AddAsync(events, CancellationToken.None);

        // Assert
        version.ShouldBe(4);
        eventStore.Verify(s => s.AddAsync(3, It.IsAny<EventState>(), It.IsAny<CancellationToken>()), Times.Once);
        eventStore.Verify(s => s.AddAsync(4, It.IsAny<EventState>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that adding events increments the version and adds the events to the store.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AddAsync_AddsEventsInEmptyStoreAndReturnsCorrectVersion()
    {
        // Arrange
        var mockTimeProvider = new MockTimeProvider(DateTimeOffset.UtcNow);
        var eventStore = new Mock<IKeyValueStore<long, EventState>>();
        SetupStoreProperties(eventStore, "database", "container", "entity");

        _ = eventStore
            .Setup(s => s.ExistsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _ = eventStore
            .Setup(s => s.AddAsync(It.IsAny<long>(), It.IsAny<EventState>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("123");

        var snapshotStore = new Mock<IKeyValueStore<long, EventState>>();
        var snapshotCollectionStore = new Mock<IKeyValueStore<string, State<IEnumerable<long>>>>();

        var store = new KeyValueEventStore(
            eventStore.Object,
            snapshotStore.Object,
            snapshotCollectionStore.Object,
            mockTimeProvider);

        // Open the store before using it
        await store.OpenAsync(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1), CancellationToken.None);

        var events = new List<EventMessage>
        {
            CreateEventMessage(1),
            CreateEventMessage(2),
        };

        // Act
        long version = await store.AddAsync(events, CancellationToken.None);

        // Assert
        version.ShouldBe(2);
        eventStore.Verify(s => s.AddAsync(1, It.IsAny<EventState>(), It.IsAny<CancellationToken>()), Times.Once);
        eventStore.Verify(s => s.AddAsync(2, It.IsAny<EventState>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that clearing snapshots works correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ClearSnapshotsAsync_RemovesAllSnapshots()
    {
        // Arrange
        var mockTimeProvider = new MockTimeProvider(DateTimeOffset.UtcNow);
        var eventStore = new Mock<IKeyValueStore<long, EventState>>();
        SetupStoreProperties(eventStore, "database", "container", "entity");

        var snapshotStore = new Mock<IKeyValueStore<long, EventState>>();
        _ = snapshotStore
            .Setup(s => s.RemoveAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var snapshotCollectionStore = new Mock<IKeyValueStore<string, State<IEnumerable<long>>>>();

        var snapshotVersions = new List<long> { 1, 3, 5 };
        _ = snapshotCollectionStore
            .Setup(s => s.TryGetAsync("Versions", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new State<IEnumerable<long>>(snapshotVersions, null, null));

        _ = snapshotCollectionStore
            .Setup(s => s.RemoveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var store = new KeyValueEventStore(
            eventStore.Object,
            snapshotStore.Object,
            snapshotCollectionStore.Object,
            mockTimeProvider);

        // Open the store before using it
        await store.OpenAsync(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1), CancellationToken.None);

        // Act
        await store.ClearSnapshotsAsync(CancellationToken.None);

        // Assert
        snapshotCollectionStore.Verify(
            s => s.RemoveAsync("Versions", null, It.IsAny<CancellationToken>()),
            Times.Once);

        snapshotStore.Verify(
            s => s.RemoveAsync(1, null, It.IsAny<CancellationToken>()),
            Times.Once);

        snapshotStore.Verify(
            s => s.RemoveAsync(3, null, It.IsAny<CancellationToken>()),
            Times.Once);

        snapshotStore.Verify(
            s => s.RemoveAsync(5, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that the constructor throws an exception when the event store is null.
    /// </summary>
    [Fact]
    public void Constructor_NullEventStore_ThrowsArgumentNullException()
    {
        // Arrange
        var snapshotStore = new Mock<IKeyValueStore<long, EventState>>();
        var snapshotCollectionStore = new Mock<IKeyValueStore<string, State<IEnumerable<long>>>>();

        // Act & Assert
        _ = Should.Throw<ArgumentNullException>(() =>
            new KeyValueEventStore(
                null!,
                snapshotStore.Object,
                snapshotCollectionStore.Object,
                null));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when the snapshot collection store is null.
    /// </summary>
    [Fact]
    public void Constructor_NullSnapshotCollectionStore_ThrowsArgumentNullException()
    {
        // Arrange
        var eventStore = new Mock<IKeyValueStore<long, EventState>>();
        var snapshotStore = new Mock<IKeyValueStore<long, EventState>>();

        // Act & Assert
        _ = Should.Throw<ArgumentNullException>(() =>
            new KeyValueEventStore(
                eventStore.Object,
                snapshotStore.Object,
                null!,
                null));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when the snapshot store is null.
    /// </summary>
    [Fact]
    public void Constructor_NullSnapshotStore_ThrowsArgumentNullException()
    {
        // Arrange
        var eventStore = new Mock<IKeyValueStore<long, EventState>>();
        var snapshotCollectionStore = new Mock<IKeyValueStore<string, State<IEnumerable<long>>>>();

        // Act & Assert
        _ = Should.Throw<ArgumentNullException>(() =>
            new KeyValueEventStore(
                eventStore.Object,
                null!,
                snapshotCollectionStore.Object,
                null));
    }

    /// <summary>
    /// Tests that disposing the event store works correctly.
    /// </summary>
    [Fact]
    public void Dispose_ClosesSessionAndDisposesResources()
    {
        // Arrange
        var eventStore = new Mock<IKeyValueStore<long, EventState>>();
        _ = eventStore.As<IDisposable>().Setup(d => d.Dispose());
        SetupStoreProperties(eventStore, "database", "container", "entity");

        var snapshotStore = new Mock<IKeyValueStore<long, EventState>>();
        _ = snapshotStore.As<IDisposable>().Setup(d => d.Dispose());

        var snapshotCollectionStore = new Mock<IKeyValueStore<string, State<IEnumerable<long>>>>();
        _ = snapshotCollectionStore.As<IDisposable>().Setup(d => d.Dispose());

        var store = new KeyValueEventStore(
            eventStore.Object,
            snapshotStore.Object,
            snapshotCollectionStore.Object,
            null);

        // Act
        store.Dispose();

        // Assert
        eventStore.As<IDisposable>().Verify(d => d.Dispose(), Times.Once);
        snapshotStore.As<IDisposable>().Verify(d => d.Dispose(), Times.Once);
        snapshotCollectionStore.As<IDisposable>().Verify(d => d.Dispose(), Times.Once);
    }

    /// <summary>
    /// Tests that getting events returns all events up to the current version.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task GetAsync_ReturnsAllEvents()
    {
        // Arrange
        var mockTimeProvider = new MockTimeProvider(DateTimeOffset.UtcNow);
        var eventStore = new Mock<IKeyValueStore<long, EventState>>();
        SetupStoreProperties(eventStore, "database", "container", "entity");

        _ = eventStore
            .Setup(s => s.ExistsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((long v, CancellationToken _) => v <= 3);

        EventMessage event1 = CreateEventMessage(1);
        EventMessage event2 = CreateEventMessage(2);
        EventMessage event3 = CreateEventMessage(3);

        _ = eventStore
            .Setup(s => s.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventState(event1));

        _ = eventStore
            .Setup(s => s.GetAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventState(event2));

        _ = eventStore
            .Setup(s => s.GetAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventState(event3));

        var snapshotStore = new Mock<IKeyValueStore<long, EventState>>();
        var snapshotCollectionStore = new Mock<IKeyValueStore<string, State<IEnumerable<long>>>>();

        _ = snapshotCollectionStore
            .Setup(s => s.TryGetAsync("Versions", It.IsAny<CancellationToken>()))
            .ReturnsAsync((State<IEnumerable<long>>?)null);

        var store = new KeyValueEventStore(
            eventStore.Object,
            snapshotStore.Object,
            snapshotCollectionStore.Object,
            mockTimeProvider);

        // Open the store before using it
        await store.OpenAsync(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1), CancellationToken.None);

        // Act
        IEnumerable<EventMessage> events = await store.GetAsync(CancellationToken.None);

        // Assert
        _ = events.ShouldNotBeNull();
        events.Count().ShouldBe(3);
        events.ElementAt(0).ShouldBe(event1);
        events.ElementAt(1).ShouldBe(event2);
        events.ElementAt(2).ShouldBe(event3);
    }

    /// <summary>
    /// Tests that attempting to perform operations on a store that hasn't been opened
    /// throws a <see cref="StoreNotOpenException"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Methods_StoreNotOpened_ThrowsStoreNotOpenException()
    {
        // Arrange
        var eventStore = new Mock<IKeyValueStore<long, EventState>>();
        SetupStoreProperties(eventStore, "database", "container", "entity");

        var snapshotStore = new Mock<IKeyValueStore<long, EventState>>();
        var snapshotCollectionStore = new Mock<IKeyValueStore<string, State<IEnumerable<long>>>>();

        var store = new KeyValueEventStore(
            eventStore.Object,
            snapshotStore.Object,
            snapshotCollectionStore.Object,
            null);

        // Act & Assert
        _ = await Should.ThrowAsync<StoreNotOpenException>(() =>
            store.AddAsync([], CancellationToken.None));

        _ = await Should.ThrowAsync<StoreNotOpenException>(() =>
            store.GetAsync(CancellationToken.None));

        _ = await Should.ThrowAsync<StoreNotOpenException>(() =>
            store.VersionAsync(CancellationToken.None));
    }

    /// <summary>
    /// Tests that taking a snapshot and retrieving events with snapshot enabled works correctly.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SnapshotAsync_CreatesAndUsesSnapshot()
    {
        // Arrange
        var mockTimeProvider = new MockTimeProvider(DateTimeOffset.UtcNow);
        var eventStore = new Mock<IKeyValueStore<long, EventState>>();
        SetupStoreProperties(eventStore, "database", "container", "entity");

        _ = eventStore
            .Setup(s => s.ExistsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        EventMessage event1 = CreateEventMessage(1);
        EventMessage event2 = CreateEventMessage(2);
        EventMessage event3 = CreateEventMessage(3);
        EventMessage snapshotEvent = CreateEventMessage(2, true);

        _ = eventStore
            .Setup(s => s.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventState(event1));

        _ = eventStore
            .Setup(s => s.GetAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventState(event2));

        _ = eventStore
            .Setup(s => s.GetAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventState(event3));

        var snapshotStore = new Mock<IKeyValueStore<long, EventState>>();
        _ = snapshotStore
            .Setup(s => s.AddOrUpdateAsync(2, It.IsAny<EventState>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("123");

        _ = snapshotStore
            .Setup(s => s.GetAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventState(snapshotEvent));

        var snapshotCollectionStore = new Mock<IKeyValueStore<string, State<IEnumerable<long>>>>();

        var snapshotVersions = new List<long> { 2 };
        _ = snapshotCollectionStore
            .Setup(s => s.TryGetAsync("Versions", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new State<IEnumerable<long>>(snapshotVersions, null, null));

        _ = snapshotCollectionStore
            .Setup(s => s.SetAsync(It.IsAny<string>(), It.IsAny<State<IEnumerable<long>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("123");

        var store = new KeyValueEventStore(
            eventStore.Object,
            snapshotStore.Object,
            snapshotCollectionStore.Object,
            mockTimeProvider);

        // Open the store before using it
        await store.OpenAsync(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1), CancellationToken.None);

        // Act - Create snapshot at version 2
        await store.SnapshotAsync(2, snapshotEvent, CancellationToken.None);

        // Get events with snapshot
        IEnumerable<EventMessage> events = await store.GetAsync(useSnapshot: true, CancellationToken.None);

        // Assert
        _ = events.ShouldNotBeNull();
        events.Count().ShouldBe(2);
        events.ElementAt(0).ShouldBe(snapshotEvent);
        events.ElementAt(1).ShouldBe(event3);

        // Verify snapshot store was properly accessed
        snapshotStore.Verify(s => s.GetAsync(2, It.IsAny<CancellationToken>()), Times.Once);
        eventStore.Verify(s => s.GetAsync(1, It.IsAny<CancellationToken>()), Times.Never);
        eventStore.Verify(s => s.GetAsync(2, It.IsAny<CancellationToken>()), Times.Never);
        eventStore.Verify(s => s.GetAsync(3, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Creates a test event message.
    /// </summary>
    /// <param name="version">The version of the event.</param>
    /// <param name="snapshot">Indicates if the event is used as a snapshot.</param>
    /// <returns>A test event message.</returns>
    private static EventMessage CreateEventMessage(long version, bool snapshot = false)
    {
        var ev = new TestMessage($"EV{version}", $"The event number {version}", snapshot);
        return ev.CreateMessage(version);
    }

    /// <summary>
    /// Sets up mock properties for a store.
    /// </summary>
    /// <param name="store">The store to set up.</param>
    /// <param name="database">The database name.</param>
    /// <param name="container">The container name.</param>
    /// <param name="entity">The entity name.</param>
    private static void SetupStoreProperties(
        Mock<IKeyValueStore<long, EventState>> store,
        string database,
        string container,
        string entity)
    {
        _ = store.Setup(s => s.Database).Returns(database);
        _ = store.Setup(s => s.Container).Returns(container);
        _ = store.Setup(s => s.Entity).Returns(entity);
    }

    /// <summary>
    /// A mock time provider for testing.
    /// </summary>
    /// <param name="currentTime">The current time.</param>
    private class MockTimeProvider(DateTimeOffset currentTime) : TimeProvider
    {
        /// <inheritdoc/>
        public override DateTimeOffset GetUtcNow() => currentTime;
    }
}