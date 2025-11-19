# PROJECT CONTEXT: Radio Console (Grandpa's Old Console)
**Target Hardware:** 
  - Raspberry Pi 5 (Linux/Raspbian)
  - Development/Testing done on Windows
  - Cross platform is important
**Language:** C# (.NET 8+)
**UI Framework:** Blazor Web App (Server interactive mode preferred for Pi hardware access)
**Audio Engine:** SoundFlow Library (https://github.com/lsxprime/SoundFlow & https://lsxprime.github.io/soundflow-docs/)
**Logging:** Serilog
**Testing:** xUnit
**Persistence:** JSON and SQLite (toggleable via config)

## CORE REQUIREMENTS
1. **Audio Core:** Must wrap SoundFlow logic behind service interfaces (`IAudioService`).
2. **Inputs:** Raddy RF320 (USB Audio), Vinyl (USB ADC), Spotify (API), Local MP3s.
3. **Priority Ducking:** "Events" (TTS, Doorbell, Phone) must lower volume of "Music" (Radio, Spotify).
4. **API:** All core logic must be exposed via REST API with Swagger integration.
5. **UI Layout:** Target display is 12.5" x 3.75" (Ultra-wide). UI must be responsive Material Design 3.
6. **Architecture:** Clean Architecture / Onion Architecture.
   - `RadioConsole.Core` (Interfaces, Entities, Domain Logic)
   - `RadioConsole.Infrastructure` (SoundFlow impl, Database, External APIs)
   - `RadioConsole.API` (Controllers, Minimal API endpoints)
   - `RadioConsole.Web` (Blazor Front-end)

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
- All new code must be covered by unit tests.  For existing projects, use the frameworks in place.  For new projects use xUnit.
- Write unit tests for business logic and integration tests for data access and API endpoints.
- Test code with mocks when hardware is not present

## Copilot Preferences
- If skipping a complex implementation for ease of testing or any other reason **ALWAYS** mark that code section with a clear TODO to make these easy to find.  Note them in the followup documentation as well.
- When generating code, prefer modern C# idioms and Linux-friendly APIs
- If unsure of best approach, err toward performance and maintainability
- For protocol implementations, always summarize logic before code
- For troubleshooting, generate sample log/debug output as comments where possible
- Markdown: Always auto-document code with summary tables/samples in /docs/

## CURRENT SESSION GOAL
[I will type the specific task here after you acknowledge this context.]
