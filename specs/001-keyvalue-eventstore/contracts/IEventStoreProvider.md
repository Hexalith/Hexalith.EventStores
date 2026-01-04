# Contract: IEventStoreProvider Interface

**Package**: `Hexalith.EventStores.Abstractions`
**Namespace**: `Hexalith.EventStores`

## Overview

Factory interface for creating and opening event stores. Provides multiple overloads for different identification strategies (metadata-based, name/id-based, or fully qualified).

## Interface Definition

```csharp
public interface IEventStoreProvider
{
    Task<IEventStore> OpenStoreAsync(Metadata metadata, CancellationToken cancellationToken);
    Task<IEventStore> OpenStoreAsync(string name, string id, CancellationToken cancellationToken);
    Task<IEventStore> OpenStoreAsync(string database, string name, string id, CancellationToken cancellationToken);
}
```

## Method Contracts

### OpenStoreAsync (Metadata)

```csharp
Task<IEventStore> OpenStoreAsync(Metadata metadata, CancellationToken cancellationToken);
```

| Aspect | Description |
|--------|-------------|
| Input | metadata: Contains partition, domain name, and domain ID |
| Output | Opened IEventStore ready for operations |
| Mapping | database = metadata.Context.PartitionId |
| Mapping | name = metadata.Message.Domain.Name |
| Mapping | id = metadata.Message.Domain.Id |
| Postcondition | Store is opened with session lock acquired |
| Throws | `ArgumentNullException` if metadata is null |
| Throws | `OpenStoreFailedException` if lock cannot be acquired |

**Remarks**: This overload is useful when processing messages that already contain full metadata, such as command handlers creating events from received commands.

---

### OpenStoreAsync (name, id)

```csharp
Task<IEventStore> OpenStoreAsync(string name, string id, CancellationToken cancellationToken);
```

| Aspect | Description |
|--------|-------------|
| Input | name: Aggregate type name (e.g., "Order", "Customer") |
| Input | id: Aggregate instance identifier |
| Output | Opened IEventStore ready for operations |
| Database | Uses DefaultDatabase from EventStoreSettings |
| Postcondition | Store is opened with session lock acquired |
| Throws | `ArgumentException` if name or id is null/empty |
| Throws | `OpenStoreFailedException` if lock cannot be acquired |

**Remarks**: Simplified overload for single-tenant/single-partition scenarios.

---

### OpenStoreAsync (database, name, id)

```csharp
Task<IEventStore> OpenStoreAsync(string database, string name, string id, CancellationToken cancellationToken);
```

| Aspect | Description |
|--------|-------------|
| Input | database: Partition/tenant identifier |
| Input | name: Aggregate type name |
| Input | id: Aggregate instance identifier |
| Output | Opened IEventStore ready for operations |
| Postcondition | Store is opened with session lock acquired |
| Throws | `ArgumentException` if any parameter is null/empty |
| Throws | `OpenStoreFailedException` if lock cannot be acquired |

**Remarks**: Full control overload for multi-tenant scenarios where explicit partition control is needed.

---

## Store Identification

The three parameters map to the underlying key-value storage as follows:

| Parameter | KeyValueStore Property | Purpose |
|-----------|------------------------|---------|
| database | Database | Tenant/partition isolation |
| name | Container | Aggregate type grouping |
| id | Entity | Aggregate instance identity |

### Storage Keys Generated

For a store opened with `("TenantA", "Order", "ord-123")`:

```
Events:          TenantA/Order/ord-123/{version}
Snapshots:       TenantA__Snapshot/Order/ord-123/{version}
Snapshot Index:  TenantA__Snapshot_Index/Order/ord-123/Versions
```

---

## Configuration

The provider reads configuration from `EventStoreSettings`:

| Setting | Default | Description |
|---------|---------|-------------|
| DefaultDatabase | "EventStores" | Used when database not specified |
| DefaultOpenTimeout | 5 seconds | Maximum wait to acquire lock |
| DefaultSessionTimeout | 1 minute | Session expiration duration |

Configuration section name: `Hexalith:EventStores`

```json
{
  "Hexalith": {
    "EventStores": {
      "DefaultDatabase": "MyTenant",
      "DefaultOpenTimeout": "00:00:10",
      "DefaultSessionTimeout": "00:05:00"
    }
  }
}
```

---

## Usage Examples

### Using Metadata

```csharp
// From command handler with existing metadata
public async Task HandleAsync(PlaceOrder command, Metadata metadata, CancellationToken ct)
{
    await using IEventStore store = await _provider.OpenStoreAsync(metadata, ct);

    var events = new[] { new OrderPlaced(command.OrderId, command.Items) };
    await store.AddAsync(events.Select(e => e.CreateMessage()), ct);
}
```

### Using Name and ID

```csharp
// Simple single-tenant scenario
await using IEventStore store = await _provider.OpenStoreAsync("Order", orderId, ct);
```

### Using Full Qualification

```csharp
// Multi-tenant scenario
await using IEventStore store = await _provider.OpenStoreAsync(
    tenantId,    // database
    "Order",     // name
    orderId,     // id
    ct);
```

---

## Dependency Injection

```csharp
// Register in DI container
services.Configure<EventStoreSettings>(configuration.GetSection("Hexalith:EventStores"));
services.AddSingleton<IEventStoreProvider, KeyValueEventStoreProvider>();
```

---

## Error Handling

| Exception | Condition |
|-----------|-----------|
| `ArgumentNullException` | Required parameter is null |
| `ArgumentException` | String parameter is empty or whitespace |
| `OpenStoreFailedException` | Lock cannot be acquired within timeout |
| `SettingsException` | Required settings not configured |

---

## Thread Safety

- The provider is thread-safe and can be shared across threads
- Each OpenStoreAsync call returns an independent store instance
- Store instances should not be shared between threads
