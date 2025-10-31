# Development Guide

## Getting Started with Development

This guide will help you set up your development environment and start working on the Radio Console project.

## Prerequisites

### Required Software
1. **.NET 9.0 SDK** - Download from https://dotnet.microsoft.com/download
2. **Git** - For version control
3. **IDE** - Choose one:
   - Visual Studio 2022 (Windows/Mac) - Recommended for MAUI development
   - Visual Studio Code with C# extension
   - JetBrains Rider

### MAUI Workload Installation

After installing the .NET SDK, install the MAUI workload:

```bash
dotnet workload install maui
```

This may take a while as it downloads platform-specific tools and SDKs.

## Setting Up the Project

### Clone the Repository

```bash
git clone https://github.com/mmackelprang/Radio.git
cd Radio
```

### Restore Dependencies

```bash
dotnet restore
```

### Download Required Fonts

The project requires OpenSans fonts. Download them from:
- https://fonts.google.com/specimen/Open+Sans

Place the following files in `src/RadioConsole/Resources/Fonts/`:
- OpenSans-Regular.ttf
- OpenSans-Semibold.ttf

## Running the Application

### Development Mode (Simulation)

The application automatically detects when it's not running on a Raspberry Pi and enables simulation mode:

```bash
cd src/RadioConsole
dotnet run
```

### Platform-Specific Run Commands

**Android Emulator:**
```bash
dotnet build -t:Run -f net9.0-android
```

**iOS Simulator (Mac only):**
```bash
dotnet build -t:Run -f net9.0-ios
```

**Windows:**
```bash
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

## Project Structure Navigation

### Key Directories

```
Radio/
├── src/RadioConsole/
│   ├── Interfaces/          # Start here to understand the architecture
│   ├── Services/            # Core services (storage, environment)
│   ├── Modules/
│   │   ├── Inputs/          # Audio input implementations
│   │   └── Outputs/         # Audio output implementations
│   ├── ViewModels/          # MVVM view models
│   ├── Views/               # XAML UI views
│   ├── Models/              # Data models
│   └── Resources/           # Images, styles, fonts
```

## Development Workflow

### Making Changes

1. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**
   - Follow existing code patterns
   - Use the base classes when adding new modules
   - Ensure simulation mode works

3. **Test your changes**
   ```bash
   dotnet build
   dotnet run
   ```

4. **Commit and push**
   ```bash
   git add .
   git commit -m "Description of changes"
   git push origin feature/your-feature-name
   ```

### Adding a New Audio Input

1. **Create a new class** in `Modules/Inputs/`
   ```csharp
   public class MyInput : BaseAudioInput
   {
       public override string Id => "my_input";
       public override string Name => "My Input";
       // ... implement abstract methods
   }
   ```

2. **Initialize in ViewModel** - Add to `AudioControlViewModel.InitializeAsync()`:
   ```csharp
   var myInput = new MyInput(_environmentService, _storage);
   await myInput.InitializeAsync();
   AudioInputs.Add(myInput);
   ```

3. **Test in simulation mode**

### Adding a New Audio Output

Follow the same pattern as inputs, but inherit from `BaseAudioOutput`.

### Adding a New UI Page

1. **Create ViewModel** in `ViewModels/`
   ```csharp
   public partial class MyPageViewModel : ObservableObject
   {
       // ViewModel logic
   }
   ```

2. **Create XAML View** in `Views/`
   ```xaml
   <ContentPage ...>
       <!-- UI markup -->
   </ContentPage>
   ```

3. **Register route** in `AppShell.xaml.cs`:
   ```csharp
   Routing.RegisterRoute(nameof(Views.MyPage), typeof(Views.MyPage));
   ```

4. **Add to navigation** in `AppShell.xaml`:
   ```xaml
   <FlyoutItem Title="My Page">
       <ShellContent ContentTemplate="{DataTemplate local:MyPage}" />
   </FlyoutItem>
   ```

## Debugging

### Visual Studio
1. Set RadioConsole as startup project
2. Select target platform (Android, iOS, Windows)
3. Press F5 to debug

### VS Code
1. Use `.NET MAUI` extension
2. Select debug configuration
3. Press F5

### Console Debugging

Use `Console.WriteLine()` or `Debug.WriteLine()` for logging:

```csharp
System.Diagnostics.Debug.WriteLine($"Input initialized: {Name}");
```

## Working with Simulation Mode

### Understanding Simulation Mode

The `EnvironmentService` detects the platform:
- On Raspberry Pi: `IsRaspberryPi = true`
- On other platforms: `IsSimulationMode = true`

### Adding Simulation Support to Modules

```csharp
public override async Task InitializeAsync()
{
    if (_environmentService.IsSimulationMode)
    {
        // Provide mock/simulated behavior
        IsAvailable = true;
        _display.UpdateStatus("Simulated");
    }
    else
    {
        // Real hardware detection
        IsAvailable = await DetectHardware();
    }
}
```

### Testing Hardware-Specific Code

Without a Raspberry Pi, you can:
1. Use simulation mode to test UI and logic
2. Mock hardware responses
3. Test on actual hardware periodically

## Code Style Guidelines

### Naming Conventions
- **Classes**: PascalCase (`AudioControlViewModel`)
- **Methods**: PascalCase (`InitializeAsync`)
- **Private fields**: _camelCase (`_storage`)
- **Properties**: PascalCase (`IsAvailable`)
- **Interfaces**: IPascalCase (`IAudioInput`)

### Async Patterns
- Always use `async`/`await` for I/O operations
- Suffix async methods with `Async`
- Return `Task` or `Task<T>`

### MVVM Patterns
- Use `ObservableObject` from CommunityToolkit.Mvvm
- Use `[ObservableProperty]` for bindable properties
- Use `[RelayCommand]` for commands
- Keep business logic in ViewModels, not Views

### Documentation
- Add XML comments to public interfaces and classes
- Comment complex algorithms
- Keep comments up to date

## Common Development Tasks

### Updating NuGet Packages

```bash
dotnet list package --outdated
dotnet add package PackageName --version x.x.x
```

### Cleaning Build Artifacts

```bash
dotnet clean
rm -rf bin obj
```

### Viewing Build Output

```bash
dotnet build -v detailed
```

## Testing on Raspberry Pi

### Deploying to Raspberry Pi

1. **Build for Linux ARM64:**
   ```bash
   dotnet publish -c Release -r linux-arm64 --self-contained
   ```

2. **Copy to Raspberry Pi:**
   ```bash
   scp -r bin/Release/net9.0-linux-arm64/publish/ pi@raspberrypi:/home/pi/RadioConsole/
   ```

3. **Run on Raspberry Pi:**
   ```bash
   ssh pi@raspberrypi
   cd RadioConsole
   ./RadioConsole
   ```

### Remote Debugging

Set up SSH debugging in Visual Studio or VS Code to debug directly on the Raspberry Pi.

## Troubleshooting

### MAUI Workload Issues

If you get workload errors:
```bash
dotnet workload restore
dotnet workload update
```

### Build Errors

1. Clean and rebuild:
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```

2. Check for missing dependencies
3. Ensure fonts are installed
4. Verify .NET version: `dotnet --version`

### Runtime Errors

1. Check logs in debug output
2. Verify simulation mode is working
3. Check storage permissions
4. Validate module initialization

## Resources

### .NET MAUI
- Documentation: https://learn.microsoft.com/dotnet/maui/
- Samples: https://github.com/dotnet/maui-samples

### CommunityToolkit.Mvvm
- Documentation: https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/

### Material Design 3
- Guidelines: https://m3.material.io/

### Raspberry Pi
- Documentation: https://www.raspberrypi.org/documentation/

## Getting Help

- Open an issue on GitHub
- Review existing documentation (README.md, ARCHITECTURE.md)
- Check the PROJECT_PLAN.md for feature roadmap

Happy coding! 🎵
