# Research: KeyValue Event Store Implementation

**Feature**: 001-keyvalue-eventstore
**Date**: 2026-01-03
**Status**: Complete

## Overview

This research document consolidates findings for the KeyValue Event Store implementation, validating design decisions and documenting best practices discovered during the planning phase.

---

## Research Areas

### 1. Key-Value Storage Pattern for Event Sourcing

**Decision**: Use key-per-event storage model with composite keys (database/container/entity/version)

**Rationale**:
- Enables efficient retrieval of individual events by version number
- Supports slicing operations without loading entire stream
- Leverages existing Hexalith.KeyValueStorages abstraction for pluggable backends
- Composite key structure (aggregate type + aggregate ID + version) allows for:
  - Partition isolation via database parameter
  - Aggregate type grouping via container parameter
  - Individual aggregate identification via entity parameter
  - Event ordering via numeric version key

**Alternatives Considered**:
| Alternative | Rejected Because |
|-------------|------------------|
| Single key with event array | Poor performance for large streams; no slicing support |
| Event streaming database (EventStoreDB) | Adds external dependency; overkill for single-aggregate access patterns |
| Document store with embedded events | Limited query capabilities; difficult to slice |

---

### 2. Session-Based Concurrency Control

**Decision**: Use in-memory session locks with ConcurrentDictionary for concurrent access control

**Rationale**:
- Prevents concurrent modifications to the same event stream
- Session timeout mechanism releases orphaned locks from crashed processes
- Simple implementation using `ConcurrentDictionary<string, (SessionId, Expiration)>`
- Fail-fast behavior when store is already locked (immediate error, no waiting by default)
- Session validation on every operation ensures data integrity

**Implementation Details**:
```csharp
// Lock structure: storeId -> (sessionId, expiration)
static ConcurrentDictionary<string, (string SessionId, DateTimeOffset Expiration)> _storeLocks

// Lock acquisition with timeout
LockStore(TimeSpan lockTimeout) -> bool
OpenAsync(sessionTimeout, openTimeout, cancellationToken) -> retry loop
```

**Alternatives Considered**:
| Alternative | Rejected Because |
|-------------|------------------|
| Distributed locking (Redis, etc.) | Over-engineering for single-process scenarios; can add later |
| Optimistic concurrency only | Insufficient for session-based access patterns |
| Database-level locking | Depends on backend implementation; not abstract |

---

### 3. Snapshot Storage Strategy

**Decision**: Separate key-value stores for snapshots with version index tracking

**Rationale**:
- Snapshots stored in separate key-value store (`{database}__Snapshot`)
- Snapshot version index stored in third store (`{database}__Snapshot_Index`)
- Enables efficient "get events from last snapshot" queries
- Snapshot versions tracked as ordered collection for multi-snapshot support
- Application-controlled snapshot creation via callback function

**Storage Structure**:
```
EventStore:     database/container/entity/1..N -> EventState
SnapshotStore:  database__Snapshot/container/entity/version -> EventState
SnapshotIndex:  database__Snapshot_Index/container/entity/"Versions" -> [version1, version2, ...]
```

**Alternatives Considered**:
| Alternative | Rejected Because |
|-------------|------------------|
| Store snapshots inline with events | Complex retrieval logic; pollutes event stream |
| Single latest snapshot only | Limits recovery options; prevents point-in-time queries |
| No snapshot support | Performance degradation for long-lived aggregates |

---

### 4. Version Management and Conflict Detection

**Decision**: Sequential versioning with explicit mismatch detection

**Rationale**:
- Events stored with auto-incrementing version numbers (1, 2, 3...)
- VersionAsync() scans for first missing version to determine current
- AddAsync() validates expected sequence number if provided in metadata
- StoreVersionMismatchException thrown on conflicts with detailed context

**Version Discovery Algorithm**:
```csharp
// Scan forward until event not found
while (await _eventStore.ExistsAsync(++version, ct)) { }
return version - 1;
```

**Alternatives Considered**:
| Alternative | Rejected Because |
|-------------|------------------|
| Stored version counter | Adds write on every operation; potential consistency issues |
| ETags for concurrency | KeyValueStorage ETags are per-record, not stream-level |
| UUID-based ordering | Loses sequential numbering; complicates slicing |

---

### 5. Exception Hierarchy Design

**Decision**: Domain-specific exceptions wrapping underlying KeyValueStore failures

**Rationale**:
- Clear separation of concerns between storage and domain layers
- Actionable error messages with aggregate context (store ID, session ID, version)
- Original exceptions preserved as InnerException for debugging
- Comprehensive exception types for all failure scenarios

**Exception Types**:
| Exception | Scenario |
|-----------|----------|
| `StoreVersionMismatchException` | Expected version doesn't match current |
| `StoreNotOpenException` | Operations attempted before OpenAsync |
| `StoreSessionExpiredException` | Session timeout exceeded |
| `InvalidStoreSessionException` | Session ID mismatch (another process took lock) |
| `OpenStoreFailedException` | Could not acquire lock within timeout |
| `StreamVersionNotFoundException` | Requested version doesn't exist |
| `StreamSnapshotVersionNotFoundException` | Requested snapshot version doesn't exist |
| `StreamIdempotencyIdNotFoundException` | Idempotency ID lookup failed |
| `DuplicateEventStoreIdempotencyIdException` | Duplicate idempotency ID detected |

---

### 6. Logging Integration

**Decision**: Standard .NET logging via ILogger (to be implemented)

**Rationale**:
- Follows .NET ecosystem conventions
- Structured logging with event data (store ID, version, session ID)
- Log levels: Debug for operations, Information for key events, Warning for recoverable issues, Error for failures
- Enables correlation with application tracing

**Log Events** (recommended):
| Event | Level | Context |
|-------|-------|---------|
| Store opened | Information | storeId, sessionId |
| Store closed | Information | storeId, sessionId |
| Events added | Debug | storeId, count, newVersion |
| Snapshot created | Information | storeId, version |
| Session expired | Warning | storeId, sessionId, expiration |
| Version mismatch | Warning | storeId, expected, actual |
| Open failed | Error | storeId, timeout |

---

### 7. Atomicity and Rollback

**Decision**: Session-based atomicity with best-effort rollback

**Rationale**:
- Multi-event appends are not atomic at storage level (each event written separately)
- Session lock prevents concurrent modifications during append
- On failure mid-append: partial events may exist but session remains locked
- Application is responsible for retry/recovery using session mechanism
- Future enhancement: batch write support if KeyValueStorage adds it

**Current Behavior**:
```
1. Acquire session lock
2. Write events 1..N sequentially
3. If failure at event K: events 1..K-1 are persisted
4. Session lock prevents other writers
5. Application must decide: retry remaining or reset
```

**Note**: The spec mentions "rollback on failure" but current implementation does not actively delete partial writes. This is acceptable because:
- Session lock prevents reads of incomplete stream
- Retry can continue from where it left off
- Full rollback would require tracking written versions (added complexity)

---

## Technology Best Practices

### Hexalith.KeyValueStorages Integration

1. **Use IKeyValueProvider.Create<TKey, TValue>()** for factory-based store creation
2. **Leverage State<T>** wrapper for ETag and TTL support
3. **Handle ConcurrencyException** from underlying storage operations
4. **Async-first API** with CancellationToken propagation
5. **Disposable pattern** for resource cleanup (IDisposable + IAsyncDisposable)

### .NET 10+ / C# 14+ Features

1. **Primary constructors** for dependency injection
2. **Records** for immutable data (EventMessage, EventState)
3. **Required properties** where appropriate
4. **Field keyword** for lazy initialization (`private string StoreId => field ??= ...`)
5. **File-scoped namespaces** for cleaner code

### Testing Patterns

1. **Arrange-Act-Assert** structure for all tests
2. **XUnit [Fact]** for single scenarios
3. **XUnit [Theory] with [InlineData]** for parameterized tests
4. **Shouldly assertions** for readable failures
5. **Moq** for mocking IKeyValueStore dependencies

---

## Unresolved Items

All technical clarifications from the spec have been resolved:

| Clarification | Resolution |
|---------------|------------|
| Event storage model | Key per event with composite key |
| Multi-event atomicity | Session-based with best-effort consistency |
| Concurrent access | Fail immediately with clear error |
| Observability | Standard .NET ILogger integration |
| Exception handling | Wrap with domain exceptions, preserve InnerException |

---

## Recommendations

1. **Add ILogger injection** to KeyValueEventStore and KeyValueEventStoreProvider
2. **Consider batch operations** if Hexalith.KeyValueStorages adds bulk write support
3. **Add telemetry metrics** for performance monitoring (operation counts, latencies)
4. **Document recovery scenarios** for partial failure handling
5. **Add integration tests** with file-based and in-memory backends
