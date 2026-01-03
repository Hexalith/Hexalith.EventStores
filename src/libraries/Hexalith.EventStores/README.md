# Hexalith.EventStores Implementation

This package provides the concrete implementation of the Hexalith.EventStores.Abstractions interfaces using a key-value storage backend.

## Key Components

### KeyValueEventStore

The `KeyValueEventStore` is the core implementation of the `IEventStore` interface that provides:

- Event persistence using a key-value storage mechanism
- Snapshot management for performance optimization
- Concurrent access control with session-based locking
- Thread-safe operations with cancellation support

#### Key Features

- **Versioned Events**: Each event is stored with an incremental version number
- **Session Management**: Prevents concurrent access to the same event stream
- **Snapshot Support**: Improves read performance by avoiding full event replay
- **Async Operations**: Full async/await support with cancellation tokens

### KeyValueEventStoreProvider

The `KeyValueEventStoreProvider` implements the `IEventStoreProvider` interface and acts as a factory for `KeyValueEventStore` instances:

- Creates and manages event store instances
- Configures the underlying key-value stores for events and snapshots
- Handles naming and partitioning of event streams

### EventState

The `EventState` record encapsulates the state of an event within the store, containing:

- The event message with its data and metadata
- Additional internal state information

## Usage Examples

### Creating and Using a Store

```csharp
// Create the store provider with your key-value storage implementation
IKeyValueStorage storage = new YourKeyValueStorage();
IEventStoreProvider provider = new KeyValueEventStoreProvider(storage);

// Get or create a specific event stream
IEventStore store = await provider.GetOrCreateStoreAsync(
    "MyAggregateId",
    CancellationToken.None);

// Open the store with custom timeouts
await store.OpenAsync(
    TimeSpan.FromMinutes(5),   // Session timeout
    TimeSpan.FromSeconds(10),  // Open timeout
    CancellationToken.None);

try
{
    // Add events to the store
    var events = new List<EventMessage> { /* your events */ };
    long version = await store.AddAsync(events, CancellationToken.None);
    
    // Retrieve all events
    IEnumerable<EventMessage> allEvents = await store.GetAsync(CancellationToken.None);
    
    // Create a snapshot
    await store.SnapshotAsync(
        version,
        CalculateSnapshot(allEvents),  // Your snapshot creation logic
        CancellationToken.None);
}
finally
{
    // Always close the store when done
    store.Close();
}
```

## Implementation Details

### Storage Structure

The implementation uses three separate key-value stores:

1. **Event Store**: Stores individual events with version numbers as keys
2. **Snapshot Store**: Stores snapshots with version numbers as keys
3. **Snapshot Collection Store**: Tracks available snapshot versions

### Concurrency Control

The implementation uses a static concurrent dictionary to track active sessions, with:

- Session IDs to identify unique access tokens
- Expiration timestamps to handle session timeouts
- Lock checking on all operations to ensure exclusive access

### Snapshot Management

Snapshots are managed through several methods:

- `SnapshotAsync`: Creates a snapshot at a specific version
- `SnapshotAllAsync`: Creates snapshots at regular intervals
- `ClearSnapshotAsync`: Removes a specific snapshot
- `ClearSnapshotsAsync`: Removes all snapshots

## Implementation Notes

- The key-value store implementation is pluggable, allowing for different storage backends
- The store uses optimistic concurrency by verifying event sequence numbers
- Resource cleanup is handled through both synchronous and asynchronous disposal
- All public methods are thread-safe
