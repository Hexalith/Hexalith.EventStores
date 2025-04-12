# Hexalith.EventStores Tests

This directory contains test projects for verifying the functionality of the Hexalith.EventStores library.

## Test Projects

- **Hexalith.EventStores.Tests**: Unit and integration tests for the event store implementation

## Testing Approach

The tests in this directory follow these key principles:

1. **Comprehensive Coverage**: Tests aim to cover all public APIs and key functionality
2. **Isolation**: Tests are designed to run independently and not depend on external resources
3. **Behavior-Driven**: Tests focus on verifying behavior rather than implementation details
4. **Performance**: Tests include performance checks for critical operations

## Test Structure

- Tests are organized to reflect the structure of the main codebase
- Each test class corresponds to a class in the main project
- Test methods use descriptive names that explain the scenario being tested

## Key Test Classes

- `KeyValueEventStoreTests`: Tests for the KeyValueEventStore implementation
- `KeyValueEventStoreProviderTests`: Tests for the provider implementation

## Running Tests

To run the tests, use the following command from the repository root:

```bash
dotnet test
```

To run tests with code coverage:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## Adding New Tests

When adding new functionality to the library, follow these guidelines for tests:

1. Create a new test class or add methods to existing classes as appropriate
2. Test both success and failure scenarios
3. Verify all public API contracts
4. Include performance tests for functionality that may impact system performance
5. Follow the existing naming and organization conventions