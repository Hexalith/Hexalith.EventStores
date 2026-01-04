# Contract: IEventStore Interface

**Package**: `Hexalith.EventStores.Abstractions`
**Namespace**: `Hexalith.EventStores`

## Overview

Core interface for interacting with a single aggregate's event stream. Provides operations for storing, retrieving, and snapshotting events with session-based concurrency control.

## Interface Definition

```csharp
public interface IEventStore
{
    // Session Management
    Task OpenAsync(CancellationToken cancellationToken);
    Task OpenAsync(TimeSpan sessionTimeout, TimeSpan openTimeout, CancellationToken cancellationToken);
    void Close();

    // Event Operations
    Task<long> AddAsync(IEnumerable<EventMessage> items, CancellationToken cancellationToken);
    Task<IEnumerable<EventMessage>> GetAsync(CancellationToken cancellationToken);
    Task<IEnumerable<EventMessage>> GetAsync(bool useSnapshot, CancellationToken cancellationToken);
    Task<IEnumerable<EventMessage>> GetAsync(long version, bool useSnapshot, CancellationToken cancellationToken);
    Task<IEnumerable<EventMessage>> GetSliceAsync(long first, long last, CancellationToken cancellationToken);

    // Version Operations
    Task<long> VersionAsync(CancellationToken cancellationToken);

    // Snapshot Operations
    Task SnapshotAsync(long version, EventMessage snapshot, CancellationToken cancellationToken);
    Task SnapshotAllAsync(int chunkSize, Func<IEnumerable<EventMessage>, EventMessage> snapshot, CancellationToken cancellationToken);
    Task ClearSnapshotAsync(long version, CancellationToken cancellationToken);
    Task ClearSnapshotsAsync(CancellationToken cancellationToken);
    Task<long> LastSnapshotVersionAsync(CancellationToken cancellationToken);
    Task<IEnumerable<long>> SnapshotVersionsAsync(CancellationToken cancellationToken);
}
```

## Method Contracts

### Session Management

#### OpenAsync

```csharp
Task OpenAsync(CancellationToken cancellationToken);
Task OpenAsync(TimeSpan sessionTimeout, TimeSpan openTimeout, CancellationToken cancellationToken);
```

| Aspect | Description |
|--------|-------------|
| Precondition | Store is not already opened by this instance |
| Postcondition | Session lock acquired; store ready for operations |
| Default sessionTimeout | 1 minute |
| Default openTimeout | 5 seconds |
| Throws | `OpenStoreFailedException` if lock cannot be acquired within timeout |

#### Close

```csharp
void Close();
```

| Aspect | Description |
|--------|-------------|
| Precondition | None (safe to call multiple times) |
| Postcondition | Session lock released; store no longer operational |
| Thread Safety | Safe to call from any thread |

---

### Event Operations

#### AddAsync

```csharp
Task<long> AddAsync(IEnumerable<EventMessage> items, CancellationToken cancellationToken);
```

| Aspect | Description |
|--------|-------------|
| Input | Collection of EventMessage objects to append |
| Output | New stream version after all events added |
| Precondition | Store is opened with valid session |
| Postcondition | Events persisted with sequential versions |
| Version Check | If EventMessage.Metadata.Context.SequenceNumber is set, must match expected |
| Throws | `StoreVersionMismatchException` on version conflict |
| Throws | `StoreNotOpenException` if store not opened |
| Throws | `StoreSessionExpiredException` if session expired |

#### GetAsync

```csharp
Task<IEnumerable<EventMessage>> GetAsync(CancellationToken cancellationToken);
Task<IEnumerable<EventMessage>> GetAsync(bool useSnapshot, CancellationToken cancellationToken);
Task<IEnumerable<EventMessage>> GetAsync(long version, bool useSnapshot, CancellationToken cancellationToken);
```

| Aspect | Description |
|--------|-------------|
| Output | Ordered collection of events (oldest to newest) |
| useSnapshot=true | Returns latest snapshot + events after snapshot |
| useSnapshot=false | Returns all events from version 1 |
| version parameter | Get events up to specific version |
| Empty stream | Returns empty collection (not null) |
| Precondition | Store is opened with valid session |

#### GetSliceAsync

```csharp
Task<IEnumerable<EventMessage>> GetSliceAsync(long first, long last, CancellationToken cancellationToken);
```

| Aspect | Description |
|--------|-------------|
| Input | first: starting version (inclusive), last: ending version (inclusive) |
| Output | Events in specified range, ordered by version |
| Precondition | Store is opened with valid session |
| Postcondition | Returns events where version >= first AND version <= last |

---

### Version Operations

#### VersionAsync

```csharp
Task<long> VersionAsync(CancellationToken cancellationToken);
```

| Aspect | Description |
|--------|-------------|
| Output | Current stream version (0 if empty) |
| Precondition | Store is opened with valid session |
| Algorithm | Scans forward until first missing version |

---

### Snapshot Operations

#### SnapshotAsync

```csharp
Task SnapshotAsync(long version, EventMessage snapshot, CancellationToken cancellationToken);
```

| Aspect | Description |
|--------|-------------|
| Input | version: stream version this snapshot represents |
| Input | snapshot: aggregate state as EventMessage |
| Precondition | Store is opened with valid session |
| Postcondition | Snapshot stored; version added to index |

#### SnapshotAllAsync

```csharp
Task SnapshotAllAsync(int chunkSize, Func<IEnumerable<EventMessage>, EventMessage> snapshot, CancellationToken cancellationToken);
```

| Aspect | Description |
|--------|-------------|
| Input | chunkSize: events between snapshots (e.g., 100) |
| Input | snapshot: function to compute state from events |
| Precondition | Store is opened with valid session |
| Postcondition | Snapshots created at intervals (100, 200, 300...) |
| Throws | `ArgumentException` if chunkSize <= 0 |

#### ClearSnapshotAsync / ClearSnapshotsAsync

```csharp
Task ClearSnapshotAsync(long version, CancellationToken cancellationToken);
Task ClearSnapshotsAsync(CancellationToken cancellationToken);
```

| Aspect | Description |
|--------|-------------|
| ClearSnapshotAsync | Removes snapshot at specific version |
| ClearSnapshotsAsync | Removes all snapshots and index |
| Precondition | Store is opened with valid session |

#### LastSnapshotVersionAsync / SnapshotVersionsAsync

```csharp
Task<long> LastSnapshotVersionAsync(CancellationToken cancellationToken);
Task<IEnumerable<long>> SnapshotVersionsAsync(CancellationToken cancellationToken);
```

| Aspect | Description |
|--------|-------------|
| LastSnapshotVersionAsync | Returns highest snapshot version (0 if none) |
| SnapshotVersionsAsync | Returns all snapshot versions (empty if none) |
| Precondition | Store is opened with valid session |

---

## Error Handling

| Exception | Condition |
|-----------|-----------|
| `StoreNotOpenException` | Operation attempted before OpenAsync |
| `StoreSessionExpiredException` | Session timeout exceeded |
| `InvalidStoreSessionException` | Different session holds the lock |
| `StoreVersionMismatchException` | Expected version doesn't match current |
| `ArgumentNullException` | Required parameter is null |

---

## Usage Example

```csharp
// Open store
await using IEventStore store = await provider.OpenStoreAsync("Orders", orderId, ct);

// Add events
var events = new[]
{
    orderPlaced.CreateMessage(sequenceNumber: 1),
    paymentReceived.CreateMessage(sequenceNumber: 2)
};
long newVersion = await store.AddAsync(events, ct);

// Get all events
IEnumerable<EventMessage> history = await store.GetAsync(ct);

// Get with snapshot optimization
IEnumerable<EventMessage> optimized = await store.GetAsync(useSnapshot: true, ct);

// Create snapshot
await store.SnapshotAsync(newVersion, stateSnapshot, ct);

// Query version
long currentVersion = await store.VersionAsync(ct);

// Store automatically closes on dispose
```
