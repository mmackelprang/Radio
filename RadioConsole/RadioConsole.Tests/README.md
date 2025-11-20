# RadioConsole.Tests

## Overview
This project contains unit and conditional integration tests for the Radio Console solution. Tests target configuration storage, audio services (SoundFlow), and UI helpers.

## Hardware-Dependent Audio Tests
Some audio tests require actual playback/capture hardware (e.g., Raspberry Pi or a desktop with active audio devices). Instead of permanently skipping these tests, they now use a runtime helper:

`TestHardwareHelper.AudioHardwareAvailable()`

If hardware is unavailable the test returns early without failing. This preserves CI stability while still allowing full execution locally when hardware is present.

### Forcing Hardware Availability
Set the environment variable before running tests to force execution even if automatic detection fails:

Windows PowerShell:
```powershell
$env:RADIO_FORCE_HW_AVAILABLE=1
dotnet test
```

Linux/macOS (bash):
```bash
export RADIO_FORCE_HW_AVAILABLE=1
dotnet test
```

### Detection Logic Summary
1. Environment override (`RADIO_FORCE_HW_AVAILABLE=1` or `true`).
2. SoundFlow device enumeration via `SoundFlowAudioDeviceManager` (any output devices -> available).
3. OS heuristics:
	- Linux: presence of ALSA device entries in `/proc/asound`.
	- Windows: presence of the Windows system directory.
	- macOS: presence of `/System` directory.

If all checks fail, tests treat audio hardware as unavailable and exit early.

### Adding New Hardware Tests
Wrap hardware-dependent logic at the start of the test method:
```csharp
if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }
```

Keep assertions after hardware initialization calls such as:
```csharp
await player.InitializeAsync("default");
```

## Environment Variables
| Variable | Purpose | Values |
|----------|---------|--------|
| `RADIO_FORCE_HW_AVAILABLE` | Force hardware tests to run | `1`, `true` |

## Running Tests
Standard run:
```powershell
dotnet test
```

With coverage (example):
```powershell
dotnet test --collect:"XPlat Code Coverage"
```

## Notes
- Configuration tests isolate storage by using unique temp directories per test instance.
- Audio tests avoid `[Fact(Skip=...)]` in favor of dynamic availability checks for better observability.
- Consider extending helper to distinguish playback vs capture devices if needed for future input tests.

