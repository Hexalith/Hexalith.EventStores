# Data Model: KeyValue Event Store

**Feature**: 001-keyvalue-eventstore
**Date**: 2026-01-03
**Status**: Complete

## Overview

This document defines the data entities, their relationships, validation rules, and state transitions for the KeyValue Event Store implementation.

---

## Entity Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           EventStoreProvider                                 │
│  (Factory for creating event stores)                                        │
├─────────────────────────────────────────────────────────────────────────────┤
│  - IKeyValueProvider storage                                                │
│  - EventStoreSettings settings                                              │
│  - TimeProvider timeProvider                                                │
└───────────────────────────────┬─────────────────────────────────────────────┘
                                │ creates
                                ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              EventStore                                      │
│  (Manages single aggregate's event stream with session control)             │
├─────────────────────────────────────────────────────────────────────────────┤
│  - IKeyValueStore<long, EventState> eventStore                              │
│  - IKeyValueStore<long, EventState> snapshotStore                           │
│  - IKeyValueStore<string, State<IEnumerable<long>>> snapshotCollectionStore │
│  - string sessionId                                                          │
│  - TimeProvider timeProvider                                                 │
└───────────────────────────────┬─────────────────────────────────────────────┘
                                │ stores
                                ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                             EventMessage                                     │
│  (Container for domain event + metadata)                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│  - Polymorphic Event        (the domain event, polymorphically serialized) │
│  - Metadata Metadata        (message and context metadata)                  │
└───────────────────────────────┬─────────────────────────────────────────────┘
                                │ contains
                                ▼
┌─────────────────────────┐                    ┌─────────────────────────────┐
│      Metadata           │                    │      Polymorphic            │
├─────────────────────────┤                    ├─────────────────────────────┤
│  - MessageMetadata      │                    │  (Type discriminator +      │
│  - ContextMetadata      │                    │   serialized event data)    │
└─────────────────────────┘                    └─────────────────────────────┘
```

---

## Entities

### 1. EventMessage

**Description**: Immutable container for a domain event with its associated metadata.

**Package**: `Hexalith.EventStores.Abstractions`

**Definition**:
```csharp
[DataContract]
public sealed record EventMessage(
    [property: DataMember(Order = 1)] Polymorphic Event,
    [property: DataMember(Order = 2)] Metadata Metadata);
```

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| Event | Polymorphic | Yes | The domain event, serialized polymorphically |
| Metadata | Metadata | Yes | Message and context metadata |

**Validation Rules**:
- Event must not be null
- Metadata must not be null
- Metadata.Context.SequenceNumber must match expected version (if provided)

**Relationships**:
- Contains one `Metadata` instance
- Stored as `EventState` wrapper in key-value store

---

### 2. EventState

**Description**: State wrapper for EventMessage to integrate with KeyValueStorages.

**Package**: `Hexalith.EventStores`

**Definition**:
```csharp
public record EventState(EventMessage Value) : State<EventMessage>(Value, null, null);
```

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| Value | EventMessage | Yes | The wrapped event message |
| Etag | string? | No | Inherited from State<T>, used for concurrency |
| TimeToLive | TimeSpan? | No | Inherited from State<T>, not used |

**Relationships**:
- Wraps one `EventMessage`
- Stored with numeric version key in key-value store

---

### 3. EventStoreSettings

**Description**: Configuration for event store behavior.

**Package**: `Hexalith.EventStores.Abstractions`

**Definition**:
```csharp
public class EventStoreSettings : ISettings
{
    public string DefaultDatabase { get; set; } = "EventStores";
    public TimeSpan DefaultOpenTimeout { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan DefaultSessionTimeout { get; set; } = TimeSpan.FromMinutes(1);
}
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| DefaultDatabase | string | "EventStores" | Default database/partition name |
| DefaultOpenTimeout | TimeSpan | 5 seconds | Maximum wait time to acquire lock |
| DefaultSessionTimeout | TimeSpan | 1 minute | Session expiration duration |

**Validation Rules**:
- DefaultDatabase must not be null or whitespace
- DefaultOpenTimeout must be positive
- DefaultSessionTimeout must be positive

---

### 4. Session (Internal Concept)

**Description**: Lock representing exclusive access to an event store with expiration.

**Storage**: `ConcurrentDictionary<string, (string SessionId, DateTimeOffset Expiration)>`

| Property | Type | Description |
|----------|------|-------------|
| StoreId | string | Composite key: `{entity}/{container}/{database}` |
| SessionId | string | Unique session identifier (GUID) |
| Expiration | DateTimeOffset | When the session expires |

**State Transitions**:
```
[Not Locked] --OpenAsync()--> [Locked]
[Locked] --Close()--> [Not Locked]
[Locked] --Expiration--> [Not Locked]
[Locked] --Dispose()--> [Not Locked]
```

**Validation Rules**:
- Only one session can hold a lock per store
- Session must be checked on every operation
- Expired sessions are automatically released

---

### 5. Snapshot (Stored as EventMessage)

**Description**: Point-in-time state capture with version reference.

**Storage Structure**:
- Snapshot data: `{database}__Snapshot/{container}/{entity}/{version}` -> EventState
- Snapshot index: `{database}__Snapshot_Index/{container}/{entity}/Versions` -> long[]

| Property | Type | Description |
|----------|------|-------------|
| Version | long | The event stream version this snapshot represents |
| Snapshot | EventMessage | The snapshot state as an EventMessage |

**Relationships**:
- One event stream can have multiple snapshots at different versions
- Snapshot versions are tracked in a separate collection

---

## Key-Value Storage Schema

### Event Storage

| Key | Value | Description |
|-----|-------|-------------|
| `{database}/{container}/{entity}/1` | EventState | First event |
| `{database}/{container}/{entity}/2` | EventState | Second event |
| `{database}/{container}/{entity}/N` | EventState | Nth event |

### Snapshot Storage

| Key | Value | Description |
|-----|-------|-------------|
| `{database}__Snapshot/{container}/{entity}/100` | EventState | Snapshot at version 100 |
| `{database}__Snapshot/{container}/{entity}/200` | EventState | Snapshot at version 200 |

### Snapshot Index Storage

| Key | Value | Description |
|-----|-------|-------------|
| `{database}__Snapshot_Index/{container}/{entity}/Versions` | State<long[]> | [100, 200, ...] |

---

## State Transitions

### EventStore Lifecycle

```
                    ┌──────────────────┐
                    │     Created      │
                    │  (not opened)    │
                    └────────┬─────────┘
                             │ OpenAsync()
                             ▼
                    ┌──────────────────┐
        ┌──────────│     Opened       │◄───────────┐
        │          │  (session active) │            │
        │          └────────┬─────────┘            │
        │                   │                       │
        │    AddAsync()     │     GetAsync()       │ valid operation
        │    SnapshotAsync()│     VersionAsync()   │
        │    GetSliceAsync()│     etc.             │
        │                   │                       │
        │                   ▼                       │
        │          ┌──────────────────┐            │
        │          │    Operating     │────────────┘
        │          └──────────────────┘
        │
        │ Close() / Dispose() / Expiration
        ▼
┌──────────────────┐
│      Closed      │
│  (lock released) │
└──────────────────┘
```

### Error States

| Current State | Trigger | Result State | Exception |
|---------------|---------|--------------|-----------|
| Not Opened | Any operation | Error | StoreNotOpenException |
| Opened | Session expired | Error | StoreSessionExpiredException |
| Opened | Different session | Error | InvalidStoreSessionException |
| Opened | Version conflict | Error | StoreVersionMismatchException |
| Not Locked | Open timeout | Error | OpenStoreFailedException |

---

## Metadata Structure

### Metadata (from Hexalith.Commons.Metadatas)

```csharp
public record Metadata(
    MessageMetadata Message,
    ContextMetadata Context);
```

### MessageMetadata

| Property | Type | Description |
|----------|------|-------------|
| Id | string | Unique message identifier |
| Name | string | Event type name (polymorphic discriminator) |
| Version | int | Event schema version |
| Domain | DomainMetadata | Aggregate type and ID |
| CreatedDate | DateTimeOffset | When the message was created |

### ContextMetadata

| Property | Type | Description |
|----------|------|-------------|
| CorrelationId | string | Correlation ID for tracing |
| UserId | string | User who triggered the event |
| PartitionId | string | Partition/database identifier |
| Timestamp | DateTimeOffset | Context timestamp |
| SessionId | string? | Session identifier |
| SequenceNumber | long? | Expected version for concurrency |
| CausationId | string? | ID of causing message |
| IdempotencyId | string? | Idempotency key |
| Scopes | string[] | Security/context scopes |

---

## Invariants

1. **Event Ordering**: Events are always stored and retrieved in sequential version order (1, 2, 3...).

2. **Version Uniqueness**: Each version number within a stream is unique; no duplicate versions.

3. **Session Exclusivity**: At most one active session can hold a lock on any given store.

4. **Immutability**: Once stored, events cannot be modified (append-only).

5. **Snapshot Consistency**: A snapshot at version N represents the aggregate state after applying events 1..N.

6. **Key Structure**: Store identity is always `{database}/{container}/{entity}` where:
   - database = partition/tenant
   - container = aggregate type name
   - entity = aggregate ID
