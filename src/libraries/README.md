# Hexalith.EventStores Source Code

This directory contains the source code for the Hexalith.EventStores packages. The project is structured into multiple libraries to enforce separation of concerns and provide flexible implementation options.

## Project Structure

- **Hexalith.EventStores.Abstractions**: Core interfaces and models that define the contract for event stores
- **Hexalith.EventStores**: Implementation of the event store using key-value storage

## Hexalith.EventStores.Abstractions

This project defines the abstract interfaces and common models for event stores, including:

### Interfaces

- `IEventStore`: The main interface for event store operations
- `IEventStoreProvider`: Factory interface for creating event store instances

### Models

- `EventMessage`: Container for event data with metadata
- `EventStoreHelper`: Utility methods for working with event stores

### Configurations

- Contains configuration classes for setting up event stores

### Exceptions

- Custom exceptions for error handling in event store operations

## Hexalith.EventStores

This project provides the concrete implementation of the event store interfaces using key-value storage:

### Components

- `KeyValueEventStore`: Implementation of `IEventStore` using key-value storage
- `KeyValueEventStoreProvider`: Implementation of `IEventStoreProvider` for creating key-value based event stores
- `EventState`: Internal state representation for stored events

## Development Guidelines

When extending or modifying the Hexalith.EventStores codebase:

1. Keep interfaces in the Abstractions project
2. Place implementations in the main project
3. Maintain backward compatibility for public interfaces
4. Follow C# coding conventions and include XML documentation
5. Add appropriate unit tests for new functionality
