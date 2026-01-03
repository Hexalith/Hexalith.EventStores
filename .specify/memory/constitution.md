<!--
SYNC IMPACT REPORT
==================
Version change: 1.0.1 → 1.0.2 (PATCH - Document persistence layer dependency)

Modified principles: None

Added sections:
- Technology Stack: Added Hexalith.KeyValueStorages as core dependency for persistence

Removed sections: None

Templates requiring updates:
- .specify/templates/plan-template.md: ✅ No changes needed (generic structure)
- .specify/templates/spec-template.md: ✅ No changes needed (generic structure)
- .specify/templates/tasks-template.md: ✅ No changes needed (generic structure)

Follow-up TODOs: None
-->

# Hexalith.EventStores Constitution

## Project Scope

This repository contains **NuGet packages only** for managing and persisting event stores. It does NOT
contain an application. All development focuses on creating reusable library packages that can be
consumed by applications in the Hexalith ecosystem.

## Core Principles

### I. Domain-Driven Design Architecture

All code MUST follow the Hexalith vertical slice architecture with separate NuGet packages per layer.
Each module MUST be organized with clear separation between Domain, Application, Infrastructure, and
Presentation layers. Dependencies MUST flow downward only: Presentation → Infrastructure →
Application → Domain → Abstractions.

**Rationale**: Enforces modularity, testability, and clear boundaries that prevent accidental coupling
and enable independent deployment of packages.

### II. Event Sourcing First

The event store is the source of truth. All state changes MUST be captured as immutable domain events.
Event streams MUST support versioning, snapshots for performance optimization, and session-based
concurrency control. Events MUST be named in past tense (e.g., `OrderPlaced`, `AccountDebited`).

**Rationale**: Event sourcing provides complete audit trails, enables temporal queries, and supports
reliable distributed systems through event-driven communication.

### III. Test-First Development

All new functionality MUST have corresponding tests using XUnit and Shouldly. Tests MUST follow the
Arrange-Act-Assert pattern. Use `[Fact]` for single cases and `[Theory]` with `[InlineData]` for
parameterized tests. Test classes MUST be organized by aggregate in `test/Hexalith.{Module}.Tests/`.

**Rationale**: Tests document expected behavior, catch regressions early, and enable confident
refactoring. Shouldly provides readable assertion failures.

### IV. Code Quality Standards

All code MUST use C# 14+ features including primary constructors for classes and records.
All public, protected, and internal members MUST have XML documentation.
Use `ArgumentNullException.ThrowIfNull()` and `ArgumentException.ThrowIfNullOrWhiteSpace()` for
validation. Follow Hexalith naming conventions: `I` prefix for interfaces, `Async` suffix for
async methods, past tense for events, imperative for commands.

**Rationale**: Consistent code style reduces cognitive load, improves maintainability, and ensures
the codebase remains approachable for all contributors.

### V. Conventional Commits

All commit messages MUST follow the Angular Conventional Commits specification for semantic-release.
Format: `<type>(<scope>): <short description>`. Types include: `feat` (minor), `fix` (patch),
`docs`, `style`, `refactor`, `perf` (patch), `test`, `build`, `ci`, `chore`. Use `BREAKING CHANGE:`
in footer for major version bumps.

**Rationale**: Enables automated versioning, changelog generation, and consistent project history
that is both human and machine readable.

### VI. Simplicity and YAGNI

Avoid over-engineering. Only make changes directly requested or clearly necessary. Do not add
features, refactor code, or make "improvements" beyond scope. Do not add error handling for
scenarios that cannot happen. Three similar lines of code is better than a premature abstraction.
Delete unused code completely; do not leave commented-out or renamed placeholders.

**Rationale**: Unnecessary complexity increases maintenance burden, introduces bugs, and slows
development. Simple code is easier to understand, test, and modify.

### VII. Observability and Diagnostics

All services MUST implement structured logging. Event streams MUST be traceable through correlation
IDs. Errors MUST provide actionable context including aggregate IDs, event versions, and operation
details. Performance-critical operations SHOULD include timing metrics.

**Rationale**: Production systems require visibility into behavior. Good observability enables rapid
diagnosis and reduces mean time to resolution.

## Technology Stack

**Runtime**: .NET 10+ with C# 14+ language features
**Distributed Runtime**: DAPR 1.16+ for microservices communication
**Persistence**: [Hexalith.KeyValueStorages](https://github.com/Hexalith/Hexalith.KeyValueStorages)
packages for key-value storage abstraction
**Testing**: XUnit for test framework, Shouldly for assertions
**Build System**: Centralized configuration via Hexalith.Builds submodule
**Package Management**: Central Package Management with Directory.Packages.props
**Code Quality**: Coverity Scan, Codacy, SonarCloud for static analysis

### Core Dependencies

| Package | Purpose | Repository |
|---------|---------|------------|
| Hexalith.KeyValueStorages | Key-value storage abstraction for event persistence | [GitHub](https://github.com/Hexalith/Hexalith.KeyValueStorages) |

All projects MUST reference shared build configuration:
- `Hexalith.Build.props` for common build properties
- `Hexalith.Package.props` for NuGet package properties
- `Directory.Packages.props` for centralized package versions

## Development Workflow

### Commit Process

1. Stage only related changes for atomic commits
2. Write commit message following Conventional Commits format
3. Keep short description under 50 characters, imperative mood, lowercase start
4. Wrap body at 72 characters
5. Reference issues with `Closes #123` or `Fixes #456`

### Code Review Requirements

- All changes MUST pass automated CI checks before merge
- All changes MUST maintain or improve code coverage
- All changes MUST pass static analysis (Codacy, SonarCloud) quality gates
- Breaking changes MUST be documented in commit footer

### Quality Gates

- Build MUST succeed without warnings (treat warnings as errors)
- All tests MUST pass
- No new security vulnerabilities (Security Rating A)
- No new reliability issues (Reliability Rating A)
- Technical debt ratio MUST remain below threshold

## Governance

This constitution supersedes all other development practices for the Hexalith.EventStores project.
All pull requests and code reviews MUST verify compliance with these principles.

### Amendment Procedure

1. Propose changes via pull request to this file
2. Document rationale for changes in PR description
3. Changes require review and approval
4. Update version according to semantic versioning:
   - MAJOR: Principle removal or incompatible redefinition
   - MINOR: New principle or materially expanded guidance
   - PATCH: Clarifications, wording, or non-semantic refinements
5. Update `LAST_AMENDED_DATE` upon merge

### Compliance Review

- All implementation plans MUST include a Constitution Check section
- Violations MUST be justified in Complexity Tracking with rationale
- Runtime development guidance is available in `Hexalith.Builds/CLAUDE.md`

**Version**: 1.0.2 | **Ratified**: 2026-01-03 | **Last Amended**: 2026-01-03