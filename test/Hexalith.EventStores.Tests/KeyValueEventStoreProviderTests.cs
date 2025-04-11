// <copyright file="KeyValueEventStoreProviderTests.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.EventStores.Tests;

using Hexalith.EventStores.Configurations;
using Hexalith.KeyValueStorages;

using Microsoft.Extensions.Options;

using Moq;

using Shouldly;

/// <summary>
/// Unit tests for the <see cref="KeyValueEventStoreProvider"/> class.
/// </summary>
public class KeyValueEventStoreProviderTests
{
    /// <summary>
    /// Tests that constructor throws an ArgumentNullException when options is null.
    /// </summary>
    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var storage = new Mock<IKeyValueProvider>();

        // Act & Assert
        _ = Should.Throw<ArgumentNullException>(() => new KeyValueEventStoreProvider(null!, storage.Object, null));
    }

    /// <summary>
    /// Tests that constructor throws an ArgumentNullException when storage is null.
    /// </summary>
    [Fact]
    public void Constructor_NullStorage_ThrowsArgumentNullException()
    {
        // Arrange
        var options = new OptionsWrapper<EventStoreSettings>(new EventStoreSettings
        {
            DefaultDatabase = "TestDb",
            DefaultOpenTimeout = TimeSpan.FromSeconds(5),
            DefaultSessionTimeout = TimeSpan.FromMinutes(10),
        });

        // Act & Assert
        _ = Should.Throw<ArgumentNullException>(() => new KeyValueEventStoreProvider(options, null!, null));
    }

    /// <summary>
    /// Tests that OpenStoreAsync with database, name and id correctly initializes and opens an event store.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task OpenStoreAsync_WithDatabaseNameAndId_ReturnsOpenedEventStore()
    {
        // Arrange
        var options = new OptionsWrapper<EventStoreSettings>(new EventStoreSettings
        {
            DefaultDatabase = "DefaultDb",
            DefaultOpenTimeout = TimeSpan.FromSeconds(5),
            DefaultSessionTimeout = TimeSpan.FromMinutes(10),
        });

        var eventStore = new Mock<IKeyValueStore<long, EventState>>();
        var snapshotStore = new Mock<IKeyValueStore<long, EventState>>();
        var snapshotCollectionStore = new Mock<IKeyValueStore<string, State<IEnumerable<long>>>>();

        var storageProvider = new Mock<IKeyValueProvider>();
        _ = storageProvider
            .Setup(p => p.Create<long, EventState>("CustomDb", "TestName", "TestId"))
            .Returns(eventStore.Object);
        _ = storageProvider
            .Setup(p => p.Create<long, EventState>("CustomDb__Snapshot", "TestName", "TestId"))
            .Returns(snapshotStore.Object);
        _ = storageProvider
            .Setup(p => p.Create<string, State<IEnumerable<long>>>("CustomDb__Snapshot_Index", "TestName", "TestId"))
            .Returns(snapshotCollectionStore.Object);

        var provider = new KeyValueEventStoreProvider(options, storageProvider.Object, null);

        // Act
        IEventStore store = await provider.OpenStoreAsync("CustomDb", "TestName", "TestId", CancellationToken.None);

        // Assert
        _ = store.ShouldNotBeNull();
        _ = store.ShouldBeOfType<KeyValueEventStore>();
    }

    /// <summary>
    /// Tests that OpenStoreAsync with metadata correctly initializes and opens an event store.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task OpenStoreAsync_WithMetadata_ReturnsOpenedEventStore()
    {
        // Arrange
        var options = new OptionsWrapper<EventStoreSettings>(new EventStoreSettings
        {
            DefaultDatabase = "DefaultDb",
            DefaultOpenTimeout = TimeSpan.FromSeconds(5),
            DefaultSessionTimeout = TimeSpan.FromMinutes(10),
        });

        var eventStore = new Mock<IKeyValueStore<long, EventState>>();
        var snapshotStore = new Mock<IKeyValueStore<long, EventState>>();
        var snapshotCollectionStore = new Mock<IKeyValueStore<string, State<IEnumerable<long>>>>();

        var storageProvider = new Mock<IKeyValueProvider>();
        _ = storageProvider
            .Setup(p => p.Create<long, EventState>("TestPartition", "TestDomain", "TestDomainId"))
            .Returns(eventStore.Object);
        _ = storageProvider
            .Setup(p => p.Create<long, EventState>("TestPartition__Snapshot", "TestDomain", "TestDomainId"))
            .Returns(snapshotStore.Object);
        _ = storageProvider
            .Setup(p => p.Create<string, State<IEnumerable<long>>>("TestPartition__Snapshot_Index", "TestDomain", "TestDomainId"))
            .Returns(snapshotCollectionStore.Object);

        var provider = new KeyValueEventStoreProvider(options, storageProvider.Object, null);

        var ev = new TestMessage("123", "Test Message 123", false);
        EventMessage eventMessage = ev.CreateMessage();

        // Act
        IEventStore store = await provider.OpenStoreAsync(eventMessage.Metadata, CancellationToken.None);

        // Assert
        _ = store.ShouldNotBeNull();
        _ = store.ShouldBeOfType<KeyValueEventStore>();
    }

    /// <summary>
    /// Tests that OpenStoreAsync with name and id correctly initializes and opens an event store.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task OpenStoreAsync_WithNameAndId_ReturnsOpenedEventStore()
    {
        // Arrange
        var options = new OptionsWrapper<EventStoreSettings>(new EventStoreSettings
        {
            DefaultDatabase = "TestDb",
            DefaultOpenTimeout = TimeSpan.FromSeconds(5),
            DefaultSessionTimeout = TimeSpan.FromMinutes(10),
        });

        var eventStore = new Mock<IKeyValueStore<long, EventState>>();
        var snapshotStore = new Mock<IKeyValueStore<long, EventState>>();
        var snapshotCollectionStore = new Mock<IKeyValueStore<string, State<IEnumerable<long>>>>();

        var storageProvider = new Mock<IKeyValueProvider>();
        _ = storageProvider
            .Setup(p => p.Create<long, EventState>("TestDb", "TestName", "TestId"))
            .Returns(eventStore.Object);
        _ = storageProvider
            .Setup(p => p.Create<long, EventState>("TestDb__Snapshot", "TestName", "TestId"))
            .Returns(snapshotStore.Object);
        _ = storageProvider
            .Setup(p => p.Create<string, State<IEnumerable<long>>>("TestDb__Snapshot_Index", "TestName", "TestId"))
            .Returns(snapshotCollectionStore.Object);

        var provider = new KeyValueEventStoreProvider(options, storageProvider.Object, null);

        // Act
        IEventStore store = await provider.OpenStoreAsync("TestName", "TestId", CancellationToken.None);

        // Assert
        _ = store.ShouldNotBeNull();
        _ = store.ShouldBeOfType<KeyValueEventStore>();
    }
}