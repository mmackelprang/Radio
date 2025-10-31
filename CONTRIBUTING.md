# Contributing to Radio Console

Thank you for your interest in contributing to the Radio Console project! This is primarily a personal project, but contributions, suggestions, and feedback are welcome.

## How to Contribute

### Reporting Issues

If you find a bug or have a suggestion:

1. Check if the issue already exists in the [Issues](https://github.com/mmackelprang/Radio/issues) section
2. If not, create a new issue with:
   - Clear title and description
   - Steps to reproduce (for bugs)
   - Expected vs actual behavior
   - Your environment (OS, .NET version, etc.)
   - Screenshots if applicable

### Suggesting Features

Feature suggestions are welcome! Please:

1. Check the [PROJECT_PLAN.md](PROJECT_PLAN.md) to see if it's already planned
2. Open an issue with the `enhancement` label
3. Describe the feature and its use case
4. Explain how it fits with the project goals

### Code Contributions

While this is a personal project, quality contributions are appreciated:

#### Before You Start

1. Open an issue to discuss the change
2. Wait for approval/feedback
3. Fork the repository
4. Create a feature branch

#### Development Process

1. **Read the documentation:**
   - [README.md](README.md) - Project overview
   - [ARCHITECTURE.md](ARCHITECTURE.md) - Architecture details
   - [DEVELOPMENT.md](DEVELOPMENT.md) - Development guide

2. **Set up your environment:**
   ```bash
   git clone https://github.com/YOUR_USERNAME/Radio.git
   cd Radio
   dotnet restore
   ```

3. **Create a feature branch:**
   ```bash
   git checkout -b feature/your-feature-name
   ```

4. **Make your changes:**
   - Follow existing code style
   - Add comments for complex logic
   - Ensure simulation mode works
   - Update documentation if needed

5. **Test your changes:**
   ```bash
   dotnet build
   dotnet run
   ```

6. **Commit your changes:**
   ```bash
   git add .
   git commit -m "Brief description of changes"
   ```

7. **Push to your fork:**
   ```bash
   git push origin feature/your-feature-name
   ```

8. **Create a Pull Request:**
   - Describe what changed and why
   - Reference any related issues
   - Include screenshots for UI changes

## Code Style Guidelines

### C# Conventions

- Use C# 12.0 features where appropriate
- Follow Microsoft C# coding conventions
- Use `async`/`await` for I/O operations
- Prefer `var` when type is obvious
- Use nullable reference types

### Naming Conventions

```csharp
// Classes: PascalCase
public class AudioControlViewModel

// Methods: PascalCase
public async Task InitializeAsync()

// Private fields: _camelCase
private readonly IStorage _storage;

// Properties: PascalCase
public bool IsAvailable { get; set; }

// Interfaces: IPascalCase
public interface IAudioInput

// Constants: PascalCase
private const int MaxRetries = 3;
```

### MVVM Patterns

- Use `ObservableObject` from CommunityToolkit.Mvvm
- Use `[ObservableProperty]` attribute
- Use `[RelayCommand]` attribute
- Keep ViewModels testable (no UI references)

### Documentation

- Add XML comments to public APIs:
  ```csharp
  /// <summary>
  /// Initializes the audio input asynchronously
  /// </summary>
  public async Task InitializeAsync()
  ```

- Comment complex algorithms
- Update README/docs for new features

## Project Structure

When adding new code:

- **Interfaces** → `Interfaces/`
- **Services** → `Services/`
- **Input modules** → `Modules/Inputs/`
- **Output modules** → `Modules/Outputs/`
- **ViewModels** → `ViewModels/`
- **Views** → `Views/`
- **Models** → `Models/`

## Testing

Currently, the project doesn't have automated tests (planned for Phase 8).

When they're added:
- Write unit tests for new functionality
- Ensure all tests pass before submitting PR
- Include tests for edge cases

## Simulation Mode

All new modules MUST support simulation mode:

```csharp
public override async Task InitializeAsync()
{
    if (_environmentService.IsSimulationMode)
    {
        // Simulated behavior
        IsAvailable = true;
    }
    else
    {
        // Real hardware
        IsAvailable = await DetectHardware();
    }
}
```

## Pull Request Guidelines

### Before Submitting

- [ ] Code builds without errors
- [ ] Code follows style guidelines
- [ ] Simulation mode works
- [ ] Documentation updated
- [ ] Commit messages are clear
- [ ] No unnecessary changes (formatting, whitespace)

### PR Description Should Include

1. **What** - What does this PR do?
2. **Why** - Why is this change needed?
3. **How** - How does it work?
4. **Testing** - How was it tested?
5. **Screenshots** - For UI changes

### Review Process

1. Maintainer reviews the PR
2. Feedback/changes requested if needed
3. Once approved, PR is merged
4. Contributor is credited in CHANGELOG

## Code of Conduct

### Be Respectful

- Be kind and courteous
- Respect differing opinions
- Accept constructive criticism
- Focus on what's best for the project

### Be Collaborative

- Help others when you can
- Ask questions if unclear
- Share knowledge and insights

### Be Professional

- Keep discussions on-topic
- Don't spam or troll
- Respect maintainer decisions

## Getting Help

- Review [DEVELOPMENT.md](DEVELOPMENT.md) for setup help
- Check existing issues and documentation
- Open an issue for questions
- Be patient - this is a side project

## License

By contributing, you agree that your contributions will be licensed under the same license as the project (see [LICENSE](LICENSE)).

## Recognition

Contributors will be acknowledged in:
- Pull request credits
- CHANGELOG.md
- README.md (for significant contributions)

Thank you for contributing to Radio Console! 🎵
