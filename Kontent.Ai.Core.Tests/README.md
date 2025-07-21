# Kontent.Ai.Core.Tests

This project contains unit tests for the `Kontent.Ai.Core` library, focusing on the core logic and testable components.

## Project Structure

```
Kontent.Ai.Core.Tests/
├── Configuration/
│   ├── ClientOptionsTests.cs          # ClientOptions validation tests
│   ├── CoreOptionsTests.cs            # CoreOptions and RefitSettings tests
│   └── SdkIdentityTests.cs            # SdkIdentity behavior tests
├── Extensions/
│   ├── HttpRequestHeadersExtensionsTests.cs   # Request header manipulation tests
│   └── HttpResponseHeadersExtensionsTests.cs  # Response header parsing tests
├── Handlers/
│   ├── AuthenticationHandlerTests.cs  # Authentication handler constructor tests
│   ├── TelemetryHandlerTests.cs       # Telemetry handler constructor tests
│   └── TrackingHandlerTests.cs        # Tracking handler constructor tests
├── Attributes/
│   └── SourceTrackingHeaderAttributeTests.cs  # Source tracking attribute tests
├── Modules/
│   └── DefaultApiUsageListenerTests.cs        # Default API usage listener tests
└── TestHelpers/
    ├── FakeLogger.cs                   # Custom logger for testing
    └── TestClientOptions.cs           # Test implementation of ClientOptions
```

## Test Coverage

### ✅ Configuration Tests (6 tests)
- **ClientOptions**: Validation logic for required properties, default values
- **CoreOptions**: Default constructor behavior, RefitSettings creation
- **SdkIdentity**: Constructor validation, Core singleton, tracking string formatting

### ✅ Extension Method Tests (4 tests)
- **HttpRequestHeadersExtensions**: SDK tracking headers, authorization headers
- **HttpResponseHeadersExtensions**: Continuation headers, retry-after parsing

### ✅ Handler Tests (3 tests)
- **AuthenticationHandler**: Constructor validation with null parameters
- **TelemetryHandler**: Constructor validation with dependencies
- **TrackingHandler**: Constructor validation and SDK identity handling

### ✅ Attribute Tests (2 tests)
- **SourceTrackingHeaderAttribute**: Constructor behavior and property setting

### ✅ Module Tests (4 tests)
- **DefaultApiUsageListener**: Singleton behavior, no-op async methods

### ✅ Core Business Logic (5 additional tests)
- Various edge cases and parameter validation across components

## Architecture Notes

This test project focuses on unit testing the business logic that can be tested in isolation. The HTTP handlers (authentication, telemetry, tracking) contain protected methods that are primarily intended for integration testing rather than unit testing.

For comprehensive testing of the HTTP pipeline behavior, consider creating integration tests that exercise the full HTTP client pipeline.

## Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test -v normal

# Run specific test project
dotnet test Kontent.Ai.Core.Tests/

# Run tests from specific namespace
dotnet test --filter "FullyQualifiedName~Configuration"
```

## Adding New Tests

When adding new tests:

1. **Place tests in the appropriate directory** based on the component being tested
2. **Follow the existing naming conventions** (`ComponentTests.cs`)
3. **Use the shared `TestClientOptions`** helper class when needed
4. **Follow AAA pattern** (Arrange-Act-Assert) for test structure
5. **Use descriptive test method names** that clearly indicate what is being tested 