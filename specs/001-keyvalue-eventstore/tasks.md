# Tasks: KeyValue Event Store Implementation

**Input**: Design documents from `/specs/001-keyvalue-eventstore/`
**Prerequisites**: plan.md (complete), spec.md (complete), research.md (complete), data-model.md (complete), contracts/ (complete)

**Tests**: Existing tests in `test/Hexalith.EventStores.Tests/` - additional tests will be added as needed.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Abstractions package**: `src/libraries/Hexalith.EventStores.Abstractions/`
- **Implementation package**: `src/libraries/Hexalith.EventStores/`
- **Examples**: `src/examples/Hexalith.EventStores.Example/`
- **Tests**: `test/Hexalith.EventStores.Tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project structure verification and dependency updates

- [ ] T001 Verify project structure matches plan.md layout
- [ ] T002 [P] Ensure NuGet package references are up-to-date in src/libraries/Hexalith.EventStores.Abstractions/Hexalith.EventStores.Abstractions.csproj
- [ ] T003 [P] Ensure NuGet package references are up-to-date in src/libraries/Hexalith.EventStores/Hexalith.EventStores.csproj
- [ ] T004 [P] Verify Hexalith.KeyValueStorages dependency is properly configured

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be fully tested

**Note**: Most foundational components already exist. This phase focuses on verification and any missing pieces.

- [ ] T005 Review and verify EventMessage record in src/libraries/Hexalith.EventStores.Abstractions/EventMessage.cs matches data-model.md
- [ ] T006 [P] Review and verify EventState wrapper in src/libraries/Hexalith.EventStores/EventState.cs matches data-model.md
- [ ] T007 [P] Review and verify EventStoreSettings in src/libraries/Hexalith.EventStores.Abstractions/Configurations/EventStoreSettings.cs matches data-model.md
- [ ] T008 [P] Review and verify EventStoreHelper utilities in src/libraries/Hexalith.EventStores.Abstractions/EventStoreHelper.cs
- [ ] T009 Verify IEventStore interface in src/libraries/Hexalith.EventStores.Abstractions/IEventStore.cs matches contracts/IEventStore.md
- [ ] T010 Verify IEventStoreProvider interface in src/libraries/Hexalith.EventStores.Abstractions/IEventStoreProvider.cs matches contracts/IEventStoreProvider.md

**Checkpoint**: Foundation ready - user story implementation verification can now begin

---

## Phase 3: User Story 1 - Store Domain Events (Priority: P1)

**Goal**: Developers can store domain events with versioning and sequence validation

**Independent Test**: Create event store, add events with sequence validation, verify persistence and retrieval

### Implementation Review for User Story 1

- [ ] T011 [US1] Review AddAsync implementation in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs for version conflict detection
- [ ] T012 [US1] Verify StoreVersionMismatchException in src/libraries/Hexalith.EventStores.Abstractions/Exceptions/StoreVersionMismatchException.cs includes all required properties (Metadata, ExpectedVersion, ActualVersion)
- [ ] T013 [US1] Add unit test for adding single event in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T014 [US1] Add unit test for adding multiple events in sequence in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T015 [US1] Add unit test for version mismatch detection in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T015b [US1] Add unit test for rollback on multi-event append failure in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T016 [US1] Add logging (ILogger) for AddAsync operations in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs

**Checkpoint**: User Story 1 (Store Domain Events) should be fully functional and testable independently

---

## Phase 4: User Story 2 - Retrieve Event History (Priority: P1)

**Goal**: Developers can retrieve all events in chronological order to rebuild aggregate state

**Independent Test**: Store known events, retrieve all events, verify complete ordered sequence

### Implementation Review for User Story 2

- [ ] T017 [US2] Review GetAsync implementations in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs
- [ ] T018 [US2] Add unit test for retrieving events in chronological order in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T019 [US2] Add unit test for retrieving from empty store in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T020 [US2] Add unit test for polymorphic event deserialization in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T021 [US2] Add logging (ILogger) for GetAsync operations in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs

**Checkpoint**: User Story 2 (Retrieve Event History) should be fully functional and testable independently

---

## Phase 5: User Story 3 - Manage Event Store Sessions (Priority: P1)

**Goal**: Safe concurrent access management with session-based locking

**Independent Test**: Attempt concurrent operations, verify session enforcement and expiration handling

### Implementation Review for User Story 3

- [ ] T022 [US3] Review session management (OpenAsync, Close, CheckSession, LockStore) in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs
- [ ] T023 [US3] Review StoreNotOpenException in src/libraries/Hexalith.EventStores.Abstractions/Exceptions/StoreNotOpenException.cs
- [ ] T024 [P] [US3] Review StoreSessionExpiredException in src/libraries/Hexalith.EventStores.Abstractions/Exceptions/StoreSessionExpiredException.cs
- [ ] T025 [P] [US3] Review InvalidStoreSessionException in src/libraries/Hexalith.EventStores.Abstractions/Exceptions/InvalidStoreSessionException.cs
- [ ] T026 [P] [US3] Review OpenStoreFailedException in src/libraries/Hexalith.EventStores.Abstractions/Exceptions/OpenStoreFailedException.cs
- [ ] T026b [US3] Add unit test verifying KeyValueStorage exceptions are wrapped with InnerException preserved in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T027 [US3] Add unit test for operation on closed store in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T028 [US3] Add unit test for session expiration in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T029 [US3] Add unit test for concurrent session rejection in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T030 [US3] Fix OpenAsync retry logic bug (condition should be totalWait >= openTimeout) in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs:209
- [ ] T031 [US3] Add logging (ILogger) for session operations (open, close, expiration) in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs

**Checkpoint**: User Story 3 (Manage Event Store Sessions) should be fully functional and testable independently

---

## Phase 6: User Story 4 - Store and Use Snapshots (Priority: P2)

**Goal**: Store periodic snapshots for efficient state loading without replaying all events

**Independent Test**: Store snapshot, retrieve events with snapshot option, verify only events after snapshot version returned

### Implementation Review for User Story 4

- [ ] T032 [US4] Review SnapshotAsync implementation in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs
- [ ] T033 [US4] Review SnapshotAllAsync implementation in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs
- [ ] T034 [US4] Review ClearSnapshotAsync and ClearSnapshotsAsync implementations in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs
- [ ] T035 [US4] Review LastSnapshotVersionAsync and SnapshotVersionsAsync implementations in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs
- [ ] T036 [US4] Review StreamSnapshotVersionNotFoundException in src/libraries/Hexalith.EventStores.Abstractions/Exceptions/StreamSnapshotVersionNotFoundException.cs
- [ ] T037 [US4] Fix SnapshotAllAsync bug - events list not populated before snapshot in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs:243-244
- [ ] T038 [US4] Add unit test for storing snapshot at version in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T039 [US4] Add unit test for retrieving events with snapshot support in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T040 [US4] Add unit test for clearing snapshots in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T041 [US4] Add unit test for SnapshotAllAsync at regular intervals in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T042 [US4] Add logging (ILogger) for snapshot operations in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs

**Checkpoint**: User Story 4 (Store and Use Snapshots) should be fully functional and testable independently

---

## Phase 7: User Story 5 - Retrieve Event Slices (Priority: P2)

**Goal**: Retrieve specific range of events for batch processing or state rebuild from known point

**Independent Test**: Store known sequence, request version ranges, verify correct events returned

### Implementation Review for User Story 5

- [ ] T043 [US5] Review GetSliceAsync implementation in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs
- [ ] T044 [US5] Review StreamVersionNotFoundException in src/libraries/Hexalith.EventStores.Abstractions/Exceptions/StreamVersionNotFoundException.cs
- [ ] T045 [US5] Add validation for slice boundaries in GetSliceAsync in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs
- [ ] T046 [US5] Add unit test for retrieving middle slice (e.g., versions 50-75) in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T047 [US5] Add unit test for retrieving from version to end in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T048 [US5] Add unit test for slice request beyond stream end returns empty collection in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs

**Checkpoint**: User Story 5 (Retrieve Event Slices) should be fully functional and testable independently

---

## Phase 8: User Story 6 - Query Current Version (Priority: P3)

**Goal**: Query current version without loading all events for optimistic concurrency support

**Independent Test**: Add events, verify version query returns correct sequence number

### Implementation Review for User Story 6

- [ ] T049 [US6] Review VersionAsync implementation in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs
- [ ] T050 [US6] Add unit test for version query with events in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs
- [ ] T051 [US6] Add unit test for version query on empty store in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs

**Checkpoint**: User Story 6 (Query Current Version) should be fully functional and testable independently

---

## Phase 9: Provider Implementation

**Purpose**: Verify factory pattern implementation for creating event stores

- [ ] T052 Review KeyValueEventStoreProvider implementation in src/libraries/Hexalith.EventStores/KeyValueEventStoreProvider.cs
- [ ] T053 Add unit test for OpenStoreAsync with name/id in test/Hexalith.EventStores.Tests/KeyValueEventStoreProviderTests.cs
- [ ] T054 Add unit test for OpenStoreAsync with database/name/id in test/Hexalith.EventStores.Tests/KeyValueEventStoreProviderTests.cs
- [ ] T055 Add unit test for OpenStoreAsync with Metadata in test/Hexalith.EventStores.Tests/KeyValueEventStoreProviderTests.cs
- [ ] T056 Add logging (ILogger) for provider operations in src/libraries/Hexalith.EventStores/KeyValueEventStoreProvider.cs

---

## Phase 10: Idempotency Support

**Purpose**: Implement idempotency ID tracking to prevent duplicate event processing

- [ ] T057 Review StreamIdempotencyIdNotFoundException in src/libraries/Hexalith.EventStores.Abstractions/Exceptions/StreamIdempotencyIdNotFoundException.cs
- [ ] T058 Review DuplicateEventStoreIdempotencyIdException in src/libraries/Hexalith.EventStores.Abstractions/Exceptions/DuplicateEventStoreIdempotencyIdException.cs
- [ ] T059 Add idempotency ID tracking to AddAsync in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs (if required by spec)
- [ ] T060 Add unit test for duplicate idempotency ID detection in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T061 [P] Add ILogger injection to KeyValueEventStore constructor in src/libraries/Hexalith.EventStores/KeyValueEventStore.cs
- [ ] T062 [P] Add ILogger injection to KeyValueEventStoreProvider constructor in src/libraries/Hexalith.EventStores/KeyValueEventStoreProvider.cs
- [ ] T063 Add XML documentation to all public members in src/libraries/Hexalith.EventStores.Abstractions/
- [ ] T064 [P] Add XML documentation to all public members in src/libraries/Hexalith.EventStores/
- [ ] T065 Run all unit tests and fix any failures
- [ ] T066 [P] Update example in src/examples/Hexalith.EventStores.Example/Program.cs to demonstrate all user stories
- [ ] T067 Run quickstart.md validation - verify all code examples compile and work
- [ ] T068 Build packages and verify NuGet output
- [ ] T068b Add unit test for handling corrupted/malformed stored event data in test/Hexalith.EventStores.Tests/KeyValueEventStoreTests.cs

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phases 3-8)**: All depend on Foundational phase completion
  - P1 stories (US1, US2, US3) can proceed in parallel
  - P2 stories (US4, US5) can proceed in parallel after P1
  - P3 story (US6) can proceed after P2
- **Provider (Phase 9)**: Depends on user story implementations
- **Idempotency (Phase 10)**: Can proceed after Foundational
- **Polish (Phase 11)**: Depends on all desired stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational - No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational - No dependencies on other stories
- **User Story 3 (P1)**: Can start after Foundational - No dependencies on other stories
- **User Story 4 (P2)**: Can start after Foundational - Benefits from US1/US2 completion
- **User Story 5 (P2)**: Can start after Foundational - Benefits from US1/US2 completion
- **User Story 6 (P3)**: Can start after Foundational - Independent

### Within Each User Story

- Review existing implementation first
- Add missing functionality
- Add unit tests
- Add logging
- Verify story is independently testable

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel
- P1 user stories (US1, US2, US3) can run in parallel
- P2 user stories (US4, US5) can run in parallel
- Exception review tasks within each story can run in parallel

---

## Parallel Example: P1 User Stories

```bash
# After Foundational phase, launch all P1 stories in parallel:
Phase 3: User Story 1 (Store Domain Events)
Phase 4: User Story 2 (Retrieve Event History)
Phase 5: User Story 3 (Manage Event Store Sessions)
```

---

## Implementation Strategy

### MVP First (P1 User Stories Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (verify existing implementations)
3. Complete Phase 3: User Story 1 (Store Domain Events)
4. Complete Phase 4: User Story 2 (Retrieve Event History)
5. Complete Phase 5: User Story 3 (Manage Sessions)
6. **STOP and VALIDATE**: Test all P1 stories independently
7. Build and verify packages

### Incremental Delivery

1. Complete Setup + Foundational -> Foundation ready
2. Add P1 stories (US1, US2, US3) -> Test independently -> MVP ready!
3. Add P2 stories (US4, US5) -> Test independently -> Enhanced version
4. Add P3 story (US6) -> Test independently -> Full feature set
5. Polish phase for production readiness

### Key Bug Fixes Required

1. **T030**: OpenAsync retry logic has inverted condition (line 209)
2. **T037**: SnapshotAllAsync doesn't populate events list before calling snapshot function (lines 243-244)

---

## Summary

| Metric | Value |
|--------|-------|
| Total Tasks | 71 |
| Phase 1 (Setup) | 4 tasks |
| Phase 2 (Foundational) | 6 tasks |
| Phase 3 (US1 - Store Events) | 7 tasks |
| Phase 4 (US2 - Retrieve History) | 5 tasks |
| Phase 5 (US3 - Sessions) | 11 tasks |
| Phase 6 (US4 - Snapshots) | 11 tasks |
| Phase 7 (US5 - Slices) | 6 tasks |
| Phase 8 (US6 - Version Query) | 3 tasks |
| Phase 9 (Provider) | 5 tasks |
| Phase 10 (Idempotency) | 4 tasks |
| Phase 11 (Polish) | 9 tasks |
| Parallel opportunities | Multiple per phase |
| Bug fixes identified | 2 critical |
| Suggested MVP scope | Phases 1-5 (P1 stories) |

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Most implementation already exists - focus is on review, testing, and bug fixes
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Critical bugs in OpenAsync and SnapshotAllAsync require immediate attention
