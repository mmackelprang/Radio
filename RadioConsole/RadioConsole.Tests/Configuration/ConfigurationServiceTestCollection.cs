using Xunit;

// Disables parallelization for the configuration service tests to avoid file locking
// issues with SQLite temporary database files on Windows.
[CollectionDefinition("ConfigurationServiceTestsCollection", DisableParallelization = true)]
public class ConfigurationServiceTestsCollection
{
  // No code required; the attribute applies to the collection.
}
