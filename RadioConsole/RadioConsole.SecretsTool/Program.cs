using RadioConsole.Core.Enums;
using RadioConsole.Core.Interfaces;
using RadioConsole.Core.Models;
using RadioConsole.Infrastructure.Configuration;

namespace RadioConsole.SecretsTool;

/// <summary>
/// Command-line tool for managing secrets in the Radio Console configuration system.
/// Usage: 
///   RadioConsole.SecretsTool upsert [--storage-type json|sqlite] [--storage-path path] --component Component --category Category --key Key --value Value
///   RadioConsole.SecretsTool list [--storage-type json|sqlite] [--storage-path path]
///   RadioConsole.SecretsTool delete [--storage-type json|sqlite] [--storage-path path] --category Category --key Key
/// </summary>
class Program
{
  static async Task<int> Main(string[] args)
  {
    try
    {
      if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
      {
        ShowHelp();
        return 0;
      }

      var command = args[0].ToLower();
      var options = ParseArguments(args.Skip(1).ToArray());

      var storageType = options.GetValueOrDefault("storage-type", "json").ToLower() == "sqlite" 
        ? StorageType.SQLite 
        : StorageType.Json;
      
      var storagePath = options.GetValueOrDefault("storage-path", 
        storageType == StorageType.SQLite ? "./storage/config.db" : "./storage");

      var service = CreateConfigurationService(storageType, storagePath);

      switch (command)
      {
        case "upsert":
          return await UpsertSecret(service, options);
        
        case "list":
          return await ListSecrets(service);
        
        case "delete":
          return await DeleteSecret(service, options);
        
        default:
          Console.WriteLine($"Unknown command: {command}");
          ShowHelp();
          return 1;
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error: {ex.Message}");
      return 1;
    }
  }

  static void ShowHelp()
  {
    Console.WriteLine("Radio Console Secrets Tool");
    Console.WriteLine("==========================");
    Console.WriteLine();
    Console.WriteLine("Manage secrets in the Radio Console configuration system.");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  upsert    Add or update a secret");
    Console.WriteLine("  list      List all secrets");
    Console.WriteLine("  delete    Delete a secret");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --storage-type <json|sqlite>   Storage type (default: json)");
    Console.WriteLine("  --storage-path <path>          Storage path (default: ./storage for json, ./storage/config.db for sqlite)");
    Console.WriteLine("  --component <name>             Component name (for upsert)");
    Console.WriteLine("  --category <name>              Category name (for upsert/delete)");
    Console.WriteLine("  --key <name>                   Key name (for upsert/delete)");
    Console.WriteLine("  --value <value>                Secret value (for upsert)");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  # Add a secret for TTS Azure RefreshToken");
    Console.WriteLine("  RadioConsole.SecretsTool upsert --component TTS --category Azure --key RefreshToken --value \"my-secret-token\"");
    Console.WriteLine();
    Console.WriteLine("  # List all secrets");
    Console.WriteLine("  RadioConsole.SecretsTool list");
    Console.WriteLine();
    Console.WriteLine("  # Delete a secret");
    Console.WriteLine("  RadioConsole.SecretsTool delete --category TTS_Azure --key RefreshToken");
    Console.WriteLine();
    Console.WriteLine("  # Use with SQLite storage");
    Console.WriteLine("  RadioConsole.SecretsTool upsert --storage-type sqlite --component Spotify --category Auth --key ClientSecret --value \"secret123\"");
    Console.WriteLine();
    Console.WriteLine("Secret Format:");
    Console.WriteLine("  Secrets are stored in a special 'Secrets' component.");
    Console.WriteLine("  The category is automatically concatenated from Component_Category.");
    Console.WriteLine("  To reference a secret in configuration: [SECRET:[Component,Category,Key]]");
    Console.WriteLine();
  }

  static async Task<int> UpsertSecret(IConfigurationService service, Dictionary<string, string> options)
  {
    if (!options.TryGetValue("component", out var component))
    {
      Console.WriteLine("Error: --component is required for upsert command");
      return 1;
    }

    if (!options.TryGetValue("category", out var category))
    {
      Console.WriteLine("Error: --category is required for upsert command");
      return 1;
    }

    if (!options.TryGetValue("key", out var key))
    {
      Console.WriteLine("Error: --key is required for upsert command");
      return 1;
    }

    if (!options.TryGetValue("value", out var value))
    {
      Console.WriteLine("Error: --value is required for upsert command");
      return 1;
    }

    // Concatenate category as Component_Category
    var secretCategory = $"{component}_{category}";

    var secret = new ConfigurationItem
    {
      Component = "Secrets",
      Category = secretCategory,
      Key = key,
      Value = value
    };

    await service.SaveAsync(secret);

    Console.WriteLine($"Secret saved successfully:");
    Console.WriteLine($"  Component: Secrets");
    Console.WriteLine($"  Category: {secretCategory}");
    Console.WriteLine($"  Key: {key}");
    Console.WriteLine($"  Reference: [SECRET:[{component},{category},{key}]]");
    Console.WriteLine();
    Console.WriteLine("You can now use this secret reference in your configuration values.");

    return 0;
  }

  static async Task<int> ListSecrets(IConfigurationService service)
  {
    var secrets = await service.LoadByComponentAsync("Secrets");
    var secretList = secrets.ToList();

    if (!secretList.Any())
    {
      Console.WriteLine("No secrets found.");
      return 0;
    }

    Console.WriteLine($"Found {secretList.Count} secret(s):");
    Console.WriteLine();

    foreach (var secret in secretList.OrderBy(s => s.Category).ThenBy(s => s.Key))
    {
      // Try to parse the concatenated category back to component and category
      var categoryParts = secret.Category.Split('_', 2);
      var component = categoryParts.Length > 0 ? categoryParts[0] : "Unknown";
      var category = categoryParts.Length > 1 ? categoryParts[1] : secret.Category;

      Console.WriteLine($"  Category: {secret.Category}");
      Console.WriteLine($"    Key: {secret.Key}");
      Console.WriteLine($"    Value: {MaskSecret(secret.Value)}");
      Console.WriteLine($"    Reference: [SECRET:[{component},{category},{secret.Key}]]");
      Console.WriteLine($"    Last Updated: {secret.LastUpdated:yyyy-MM-dd HH:mm:ss} UTC");
      Console.WriteLine();
    }

    return 0;
  }

  static async Task<int> DeleteSecret(IConfigurationService service, Dictionary<string, string> options)
  {
    if (!options.TryGetValue("category", out var category))
    {
      Console.WriteLine("Error: --category is required for delete command");
      return 1;
    }

    if (!options.TryGetValue("key", out var key))
    {
      Console.WriteLine("Error: --key is required for delete command");
      return 1;
    }

    var exists = await service.ExistsAsync("Secrets", key);
    if (!exists)
    {
      Console.WriteLine($"Secret not found: Category={category}, Key={key}");
      return 1;
    }

    await service.DeleteAsync("Secrets", key);

    Console.WriteLine($"Secret deleted successfully:");
    Console.WriteLine($"  Category: {category}");
    Console.WriteLine($"  Key: {key}");

    return 0;
  }

  static IConfigurationService CreateConfigurationService(StorageType storageType, string storagePath)
  {
    return ConfigurationServiceFactory.Create(storageType, storagePath);
  }

  static Dictionary<string, string> ParseArguments(string[] args)
  {
    var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    
    for (int i = 0; i < args.Length; i++)
    {
      if (args[i].StartsWith("--"))
      {
        var key = args[i].Substring(2);
        if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
        {
          options[key] = args[i + 1];
          i++; // Skip the value in the next iteration
        }
      }
    }

    return options;
  }

  static string MaskSecret(string value)
  {
    if (string.IsNullOrEmpty(value))
      return "[empty]";
    
    if (value.Length <= 4)
      return "****";
    
    return $"{value.Substring(0, 2)}{'*'.ToString().PadLeft(value.Length - 4, '*')}{value.Substring(value.Length - 2)}";
  }
}

