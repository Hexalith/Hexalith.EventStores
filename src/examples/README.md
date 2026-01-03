# Hexalith.EventStores Examples

This directory contains example applications and code samples demonstrating how to use the Hexalith.EventStores library in real-world scenarios.

## Overview

The examples are designed to showcase different usage patterns and integration approaches for the event store in various application contexts:

- Basic event sourcing patterns
- Integration with different storage backends
- Snapshot strategies for performance optimization
- Event store in distributed applications

## Example Projects

### Available Examples

#### [Bank Account Example](./Hexalith.EventStores.Example/README.md)

A simple application demonstrating core event sourcing concepts using a bank account domain model:

- Creating and using an event store with file-based persistence
- Implementing domain events with polymorphic serialization
- Adding and retrieving events with proper sequencing
- Modeling a domain with C# records and event sourcing patterns

### Planned Examples

1. **Domain-Driven Design**: Implementing event sourcing with DDD patterns
2. **Microservices Communication**: Using event stores for communication between microservices
3. **Performance Optimization**: Advanced snapshot and caching strategies
4. **Custom Storage Backends**: Implementing custom storage backends for specific needs

## Usage

Each example includes:

- Complete runnable code
- README with step-by-step explanations
- Implementation notes highlighting key concepts
- Recommended practices and patterns

## Running the Examples

To run an example project:

1. Navigate to the specific example directory
2. Follow the instructions in the example's README.md
3. Build and run using standard .NET commands:

```bash
cd examples/ExampleName
dotnet build
dotnet run
```

## Contributing Examples

We welcome contributions of additional examples that demonstrate different use cases or integration patterns. Please follow these guidelines:

1. Create a new directory for your example
2. Include a comprehensive README.md explaining the example
3. Keep the code simple and focused on demonstrating specific concepts
4. Include appropriate comments and documentation
5. Submit a pull request with your example