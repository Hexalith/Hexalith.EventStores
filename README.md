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
- **IEventStoreProvider**: Factory interface for creating event store instances
- **EventMessage**: Container for event data with metadata

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
IKeyValueStorage storage = new YourKeyValueStorage();
IEventStoreProvider provider = new KeyValueEventStoreProvider(storage);

// Get or create an event store
IEventStore store = await provider.GetOrCreateStoreAsync(
    "YourStreamName", 
    cancellationToken);

// Open the store
await store.OpenAsync(cancellationToken);

try
{
    // Store events
    var events = new List<EventMessage>
    {
        new EventMessage(
            new YourDomainEvent { /* event data */ },
            new Metadata(/* metadata */)
        )
    };
    
    long version = await store.AddAsync(events, cancellationToken);
    
    // Retrieve events
    IEnumerable<EventMessage> storedEvents = await store.GetAsync(cancellationToken);
    
    // Create a snapshot
    await store.SnapshotAsync(
        version,
        new EventMessage(/* snapshot data */),
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

The event store implements session-based locking to prevent concurrent access:

```csharp
// Open with custom timeouts
await store.OpenAsync(
    sessionTimeout: TimeSpan.FromMinutes(5),
    openTimeout: TimeSpan.FromSeconds(10),
    cancellationToken);
```

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
