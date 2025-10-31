# RadioConsole - .NET MAUI Audio System

This is a C# .NET MAUI application for creating a modern audio system on Raspberry Pi 5.

## Building the Project

This project requires the .NET MAUI workload to be installed. 

### Prerequisites

1. Install .NET 9.0 SDK or later
2. Install MAUI workload:
   ```bash
   dotnet workload install maui
   ```

### Building

From the repository root:

```bash
dotnet build RadioConsole.sln
```

Or from the project directory:

```bash
cd src/RadioConsole
dotnet build
```

### Running

To run the application:

```bash
cd src/RadioConsole
dotnet run
```

The application will automatically detect if it's running on a Raspberry Pi or in simulation mode.

## Project Structure

- `Interfaces/` - Core abstractions for the system
- `Modules/` - Input and output module implementations
- `Services/` - Core services (storage, environment detection)
- `ViewModels/` - MVVM view models
- `Views/` - XAML UI views
- `Models/` - Data models
- `Converters/` - Value converters for XAML
- `Resources/` - Application resources (images, fonts, styles)
- `Platforms/` - Platform-specific code

## Development Notes

### Simulation Mode

The application includes a simulation mode that allows development on any platform without Raspberry Pi hardware. The `EnvironmentService` automatically detects the runtime environment and enables simulation mode when not running on a Raspberry Pi.

### Adding New Input Sources

1. Create a new class that inherits from `BaseAudioInput`
2. Implement the required abstract methods
3. Add initialization in `AudioControlViewModel`

### Adding New Output Devices

1. Create a new class that inherits from `BaseAudioOutput`
2. Implement the required abstract methods
3. Add initialization in `AudioControlViewModel`

## Testing

Since MAUI workload may not be available in all CI environments, the project includes comprehensive interface definitions and base implementations that can be reviewed and validated without running the full application.
