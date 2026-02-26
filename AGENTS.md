# General

- Prefer simple solutions
- Ask if unsure
- Keep answers concise
- Break solutions into small incremental steps
- Do one step at a time

# Tech stack

- .NET 6
- F#
- YamlDotNet for YAML serialization
- xUnit for testing
- Unquote for test assertions

# Build and test

- Build: `dotnet build`
- Test: `dotnet test`
- Clean: `dotnet clean`

# Structure

Overboard is an F# library providing strongly typed builders over Kubernetes configuration, allowing declarative-style infrastructure-as-code using F#. It outputs standard Kubernetes YAML or JSON resource config files.

- `src/Overboard/` - Main library source code (F# computation expression builders for Kubernetes resources)
- `tests/Overboard.Building.Tests/` - Unit tests for builder DSL
- `tests/Overboard.Acceptance.Tests/` - Acceptance tests
- `tests/Overboard.Communication.Tests/` - Communication/integration tests
- `docs/` - Documentation
- `examples/` - Usage examples
