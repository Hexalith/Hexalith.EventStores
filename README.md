# Hexalith.EventStores

A lightweight and efficient event store implementation for .NET applications using event sourcing patterns. This library provides a robust foundation for building event-sourced systems with support for event persistence, snapshots, and versioning.

## Build Status

[![License: MIT](https://img.shields.io/github/license/hexalith/hexalith.EventStores)](https://github.com/hexalith/hexalith/blob/main/LICENSE)
[![Discord](https://img.shields.io/discord/1063152441819942922?label=Discord&logo=discord&logoColor=white&color=d82679)](https://discordapp.com/channels/1102166958918610994/1102166958918610997)

[![Coverity Scan Build Status](https://scan.coverity.com/projects/31529/badge.svg)](https://scan.coverity.com/projects/hexalith-hexalith-EventStores)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/d48f6d9ab9fb4776b6b4711fc556d1c4)](https://app.codacy.com/gh/Hexalith/Hexalith.EventStores/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.EventStores&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.EventStores)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.EventStores&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.EventStores)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.EventStores&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.EventStores)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.EventStores&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.EventStores)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.EventStores&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.EventStores)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.EventStores&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.EventStores)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.EventStores&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.EventStores)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.EventStores&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.EventStores)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.EventStores&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.EventStores)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=Hexalith_Hexalith.EventStores&metric=bugs)](https://sonarcloud.io/summary/new_code?id=Hexalith_Hexalith.EventStores)

[![Build status](https://github.com/Hexalith/Hexalith.EventStores/actions/workflows/build-release.yml/badge.svg)](https://github.com/Hexalith/Hexalith.EventStores/actions)
[![NuGet](https://img.shields.io/nuget/v/Hexalith.EventStores.svg)](https://www.nuget.org/packages/Hexalith.EventStores)
[![Latest](https://img.shields.io/github/v/release/Hexalith/Hexalith.EventStores?include_prereleases&label=preview)](https://github.com/Hexalith/Hexalith.EventStores/pkgs/nuget/Hexalith.EventStores)

## Overview

Hexalith.EventStores provides a flexible event storage solution for applications implementing event sourcing patterns. It offers:

- Event stream persistence with versioning
- Snapshot support for performance optimization
- Concurrent access control with session management
- Abstract interfaces for implementing different storage backends
- Thread-safe operations with cancellation support

## Key Components

### Abstractions

- **IEventStore**: Core interface for event store operations (add, get, snapshot, version management)
- **IEventStoreProvider**: Factory interface for opening event store instances
- **EventMessage**: Container for polymorphic event with metadata

### Implementations

- **KeyValueEventStore**: Implementation using key-value storage for persistence
- **KeyValueEventStoreProvider**: Provider for creating KeyValueEventStore instances

## Getting Started

### Installation

```bash
dotnet add package Hexalith.EventStores
```

### Basic Usage

```csharp
// Create event store provider
IKeyValueProvider keyValueProvider = new YourKeyValueProvider();
IEventStoreProvider provider = new KeyValueEventStoreProvider(keyValueProvider);

// Open an event store (creates if not exists)
IEventStore store = await provider.OpenStoreAsync(
    "YourAggregateName",
    "YourAggregateId",
    cancellationToken);

try
{
    // Store events using the helper extension method
    var events = new List<EventMessage>
    {
        new YourDomainEvent { /* event data */ }.CreateMessage()
    };

    long version = await store.AddAsync(events, cancellationToken);

    // Retrieve events
    IEnumerable<EventMessage> storedEvents = await store.GetAsync(cancellationToken);

    // Create a snapshot
    await store.SnapshotAsync(
        version,
        CalculateSnapshot(storedEvents),
        cancellationToken);
}
finally
{
    // Close the store when done
    store.Close();
}
```

## Advanced Features

### Snapshots

Snapshots allow for efficient retrieval of event stream state without replaying all events:

```csharp
// Create a snapshot at the current version
long version = await store.VersionAsync(cancellationToken);
await store.SnapshotAsync(
    version,
    CalculateSnapshot(events),
    cancellationToken);

// Retrieve using snapshot
var events = await store.GetAsync(useSnapshot: true, cancellationToken);
```

### Session Management

The event store implements session-based locking to prevent concurrent access. When using `OpenStoreAsync`, the store is automatically opened. For custom timeout configuration:

```csharp
// Open with custom timeouts (after getting the store)
await store.OpenAsync(
    TimeSpan.FromMinutes(5),  // session timeout
    TimeSpan.FromSeconds(10), // open timeout
    cancellationToken);
```

## Examples

The repository includes examples demonstrating how to use Hexalith.EventStores in practical scenarios:

### Bank Account Example

A simple application that demonstrates core event sourcing concepts using a bank account domain model:

- Creating and using an event store with file-based persistence
- Implementing domain events with polymorphic serialization
- Adding and retrieving events with proper sequencing
- Modeling a domain with C# records and event sourcing patterns

[View Bank Account Example](./src/examples/Hexalith.EventStores.Example/README.md)

### Additional Examples (Planned)

Future releases will include examples covering:

- Domain-Driven Design with event sourcing
- Microservices communication patterns
- Performance optimization techniques
- Custom storage backend implementations

[View Examples Overview](./src/examples/README.md)

## Repository Structure

The repository is organized as follows:

- [src](./src/README.md): Source code for the event store libraries
  - Hexalith.EventStores.Abstractions: Core interfaces and models
  - Hexalith.EventStores: Implementation of the event store
- [test](./test/README.md): Test projects
- [examples](./examples/README.md): Example implementations
- [Hexalith.Builds](./Hexalith.Builds/README.md): Shared build configurations

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
