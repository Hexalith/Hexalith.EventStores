# Hexalith.EventStores.Abstractions

This package defines the core abstractions and interfaces for the Hexalith.EventStores library, providing a contract for event store implementations.

## Core Components

### IEventStore

The `IEventStore` interface is the primary contract for event stores, defining methods for:

- Adding events to a stream
- Retrieving events with or without snapshots
- Creating and managing snapshots
- Managing store lifecycle (open/close)
- Handling versioning

```csharp
public interface IEventStore
{
    Task<long> AddAsync(IEnumerable<EventMessage> items, CancellationToken cancellationToken);
    Task<IEnumerable<EventMessage>> GetAsync(CancellationToken cancellationToken);
    Task<IEnumerable<EventMessage>> GetAsync(bool useSnapshot, CancellationToken cancellationToken);
    Task<IEnumerable<EventMessage>> GetAsync(long version, bool useSnapshot, CancellationToken cancellationToken);
    Task SnapshotAsync(long version, EventMessage snapshot, CancellationToken cancellationToken);
    Task<long> VersionAsync(CancellationToken cancellationToken);
    Task OpenAsync(CancellationToken cancellationToken);
    void Close();
    // Additional methods...
}
```

### IEventStoreProvider

The `IEventStoreProvider` interface defines the contract for event store factory services:

```csharp
public interface IEventStoreProvider
{
    Task<IEventStore> GetOrCreateStoreAsync(string id, CancellationToken cancellationToken);
    Task<bool> StoreExistsAsync(string id, CancellationToken cancellationToken);
    Task DeleteStoreAsync(string id, CancellationToken cancellationToken);
    // Additional methods...
}
```

### EventMessage

The `EventMessage` record encapsulates the core data structure for events:

```csharp
public record EventMessage(
    object Data,
    Metadata Metadata)
{
    // Methods and properties...
}
```

### EventStoreHelper

The `EventStoreHelper` class provides utility methods for working with event stores:

- Converting between event streams and domain objects
- Managing event metadata
- Helper methods for common event store operations

## Configurations

The configurations namespace contains classes for configuring event stores:

- Connection settings
- Performance options
- Feature toggles

## Exceptions

Custom exceptions for error handling in event store operations:

- `EventStoreException`: Base exception for all event store errors
- `StoreVersionMismatchException`: Thrown when event sequence numbers don't match
- `StoreNotOpenException`: Thrown when operations are attempted on a closed store
- `InvalidStoreSessionException`: Thrown when session validation fails
- Additional specialized exceptions

## Using the Abstractions

These abstractions can be used to:

1. Implement custom event store providers for different storage technologies
2. Create mock implementations for testing
3. Build higher-level abstractions for specific domain scenarios
4. Create adapters for third-party event store technologies

## Design Principles

The abstractions follow these design principles:

- **Separation of Concerns**: Clear distinction between storage and domain logic
- **Interface Segregation**: Focused interfaces with specific responsibilities
- **Dependency Inversion**: High-level modules depend on abstractions
- **Asynchronous First**: All long-running operations are async with cancellation support
- **Immutability**: Events are immutable records
