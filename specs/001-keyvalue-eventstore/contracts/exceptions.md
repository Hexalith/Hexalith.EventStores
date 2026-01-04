# Contract: Exception Types

**Package**: `Hexalith.EventStores.Abstractions`
**Namespace**: `Hexalith.EventStores.Exceptions`

## Overview

Domain-specific exceptions for event store operations. All exceptions provide actionable context including store identifiers, session information, and version details.

---

## Exception Hierarchy

```
System.Exception
└── StoreNotOpenException
└── StoreSessionExpiredException
└── InvalidStoreSessionException
└── StoreVersionMismatchException
└── OpenStoreFailedException
└── StreamVersionNotFoundException
└── StreamSnapshotVersionNotFoundException
└── StreamIdempotencyIdNotFoundException
└── DuplicateEventStoreIdempotencyIdException
```

---

## Session Exceptions

### StoreNotOpenException

Thrown when an operation is attempted on a store that has not been opened.

| Property | Type | Description |
|----------|------|-------------|
| StoreId | string | Store identifier (database/container/entity) |
| SessionId | string? | Session ID if any was set |

**When Thrown**:
- Any operation called before `OpenAsync()`
- Any operation called after `Close()` or `Dispose()`

**Resolution**: Call `OpenAsync()` before performing operations.

---

### StoreSessionExpiredException

Thrown when the session timeout has been exceeded.

| Property | Type | Description |
|----------|------|-------------|
| StoreId | string | Store identifier |
| SessionId | string | The expired session ID |
| Expiration | DateTimeOffset | When the session expired |

**When Thrown**:
- Any operation after session timeout elapsed
- Session timeout checked on every operation

**Resolution**: Open a new session with `OpenAsync()`.

---

### InvalidStoreSessionException

Thrown when operations are attempted with a session that no longer owns the lock.

| Property | Type | Description |
|----------|------|-------------|
| StoreId | string | Store identifier |
| CurrentSessionId | string | This instance's session ID |
| ActiveSessionId | string | The session that currently holds the lock |

**When Thrown**:
- Another process acquired the lock after this session expired
- Lock was released by another mechanism

**Resolution**: Close this store and open a new one.

---

### OpenStoreFailedException

Thrown when the store cannot be opened within the specified timeout.

| Property | Type | Description |
|----------|------|-------------|
| StoreId | string | Store identifier |
| Timeout | TimeSpan | The open timeout that was exceeded |

**When Thrown**:
- Another session holds the lock
- Lock could not be acquired within `openTimeout`

**Resolution**:
- Wait for other session to release
- Increase `openTimeout` setting
- Check for crashed processes with orphaned locks

---

## Version Exceptions

### StoreVersionMismatchException

Thrown when an event's expected version conflicts with the current stream version.

| Property | Type | Description |
|----------|------|-------------|
| Metadata | Metadata | Full metadata of the conflicting event |
| ExpectedVersion | long | Version number in event metadata |
| ActualVersion | long | Current stream version |

**When Thrown**:
- `AddAsync()` with `Metadata.Context.SequenceNumber` set
- Expected version doesn't match current + 1

**Resolution**:
- Reload current version and resolve conflict
- Retry with correct sequence number
- Use optimistic concurrency handling

---

### StreamVersionNotFoundException

Thrown when a requested event version does not exist in the stream.

| Property | Type | Description |
|----------|------|-------------|
| StoreId | string | Store identifier |
| Version | long | The requested version |

**When Thrown**:
- `GetSliceAsync()` with version beyond stream end
- Specific version retrieval for non-existent event

**Resolution**: Check current version with `VersionAsync()` first.

---

## Snapshot Exceptions

### StreamSnapshotVersionNotFoundException

Thrown when a requested snapshot version does not exist.

| Property | Type | Description |
|----------|------|-------------|
| StoreId | string | Store identifier |
| Version | long | The requested snapshot version |

**When Thrown**:
- `ClearSnapshotAsync()` for non-existent snapshot
- Direct snapshot retrieval for non-existent version

**Resolution**: Check available snapshots with `SnapshotVersionsAsync()`.

---

## Idempotency Exceptions

### StreamIdempotencyIdNotFoundException

Thrown when a lookup by idempotency ID fails.

| Property | Type | Description |
|----------|------|-------------|
| StoreId | string | Store identifier |
| IdempotencyId | string | The idempotency ID that was not found |

**When Thrown**:
- Lookup operation for non-existent idempotency ID

**Resolution**: Verify idempotency ID was correctly stored.

---

### DuplicateEventStoreIdempotencyIdException

Thrown when attempting to store an event with a duplicate idempotency ID.

| Property | Type | Description |
|----------|------|-------------|
| StoreId | string | Store identifier |
| IdempotencyId | string | The duplicate idempotency ID |
| ExistingVersion | long | Version of existing event with same ID |

**When Thrown**:
- `AddAsync()` with idempotency ID that already exists

**Resolution**:
- This is expected for duplicate requests; return success
- Check if this is an actual duplicate or a conflict

---

## Exception Best Practices

### Catching Exceptions

```csharp
try
{
    await store.AddAsync(events, ct);
}
catch (StoreVersionMismatchException ex)
{
    // Handle optimistic concurrency conflict
    _logger.LogWarning("Version conflict: expected {Expected}, actual {Actual}",
        ex.ExpectedVersion, ex.ActualVersion);
    // Reload and retry
}
catch (StoreSessionExpiredException ex)
{
    // Session timeout - need new session
    _logger.LogWarning("Session {Session} expired at {Expiration}",
        ex.SessionId, ex.Expiration);
    // Reopen store and retry
}
catch (StoreNotOpenException)
{
    // Programming error - store not opened
    throw;
}
```

### Logging Context

All exceptions include sufficient context for debugging:

```csharp
catch (OpenStoreFailedException ex)
{
    _logger.LogError(ex,
        "Failed to open store {StoreId} within {Timeout}",
        ex.StoreId, ex.Timeout);
}
```

### Wrapping Underlying Exceptions

When KeyValueStorage throws exceptions, they are wrapped with domain context:

```csharp
// Original exception preserved as InnerException
catch (DomainException ex) when (ex.InnerException is KeyValueStorageException kvEx)
{
    // Access underlying storage exception if needed
    _logger.LogError(kvEx, "Storage layer error");
}
```
