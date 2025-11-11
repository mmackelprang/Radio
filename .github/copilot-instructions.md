# Raspberry Pi Project Instructions

## Project Context
This repository contains modern C# (.NET 8+) applications and libraries, focusing on embedded systems, IoT, and audio management for Linux-based Raspberry Pi deployments.

## Preferred Technologies & Frameworks
- **Primary Language:**
  - C# (.NET 8+), no legacy syntax
- **Platform:** Target Linux (Debian/Ubuntu/Raspberry Pi OS), Developed on Windows Computer
- **Secondary:** Bash/Shell scripts for setup, cross-language interop with Python/C++ (as needed)
- **Audio:**  ALSA for mixing, NAudio for decoding/processing
- **Bluetooth:** 
  - Use BlueZ libraries
  - Focus on BLE/HFP protocol handling, direct device interfacing
- **Target Hardware:** 
  - Raspberry Pi 5, Pi 4, relevant ARM platforms
  - Make development and testing on other architectures a priority

## Coding Style & Best Practices
- **Naming Conventions:**
  - Classes, properties, and public methods should use PascalCase.
  - Local variables and private fields should use camelCase.
  - Interfaces should be prefixed with 'I'.
- **Code Formatting:**
  - Use 2 spaces for indentation.
  - Opening braces for classes, methods, and control flow statements should be on a new line.
  - Use `var` keyword when the type is obvious from the right-hand side of the assignment.
- **Error Handling:**
  - Use try-catch blocks for expected exceptions and log errors appropriately.
  - Avoid catching generic `Exception` unless absolutely necessary.
  - Create custom exceptions when the exception makes the logic clear.
- **Dependency Injection:**
  - Prefer constructor injection for dependencies.
  - Register services in `Program.cs` or dedicated extension methods.
- **General C# Instructions**
  - Use clear, concise modern C# (expression-bodied members, async/await, switch expressions)
  - Follow SOLID principles and strong separation of concerns
  - Prefer dependency injection for all services/components
  - Comment internal logic, edge cases, and protocol-specific behaviors
  - Write explicit type annotations; don't rely on implicit typing unless context is obvious
  - When building REST API's, keep the business logic and the API code separate to allow for easier testing of the business logic independently from the API.

## Linux/Raspberry Pi Considerations
- Code must run cross-platform: prioritize Linux compatibility, test under dotnet CLI and VS Code/SSH workflows
- Avoid Windows-only APIs (no WPF, WinForms, etc.)
- For hardware access, use Pi-compatible libraries (preferful .NET IoT, System.Device.Gpio, or cross-platform wrappers)

## Bluetooth & Audio Protocols
- When writing protocol handlers, always:
  - Document packet structure, field mappings, and byte orders
  - Favor efficient, direct binary parsing; avoid unnecessary allocations
  - Make transaction/state flow explicit
- For audio, support multiple sources, concurrent/background playback, volume/pan/gain controls (programmatically adjustable)

## Repository/Code Layout
- Follow modern best practices for new project layouts.
- Place platform-specific code in /src/Platform/<platform type>
- General shared libraries in /src/Common/
- Use /scripts/ for setup/utility scripts (shell, Python, etc.)
- Unit/integration tests in /tests/
- Markdown docs in /docs/

## Documentation & Status
- Avoid documentation sprawl - prefer less documents that contain sections over many different documents.
- For longer and complex projects, keep a PROJECT_STATUS document with phases in progress, phases completed, and planned future phases.  As part of each agent request keep this document up to date.

## Testing & CI
- All new code must be covered by unit tests.  For existing projects, use the frameworks in place.  For new projects use xUnit and FluentAssertions.
- Write unit tests for business logic and integration tests for data access and API endpoints.
- Use GitHub Actions for CI; target Linux runners
- Test code with mocked Bluetooth/audio devices if hardware not present

## Copilot Preferences
- When generating code, prefer modern C# idioms and Linux-friendly APIs
- If unsure of best approach, err toward performance and maintainability
- For protocol implementations, always summarize logic before code
- For troubleshooting, generate sample log/debug output as comments where possible
- Markdown: Always auto-document code with summary tables/samples in /docs/

