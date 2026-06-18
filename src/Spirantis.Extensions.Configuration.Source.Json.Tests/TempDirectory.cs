namespace Spirantis.Extensions.Configuration.Source.Json.Tests;

/// <summary>A throwaway directory that is deleted when disposed.</summary>
internal sealed class TempDirectory : IDisposable
{
    public TempDirectory()
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"spirantis-json-tests-{Guid.NewGuid():N}"
        );
        Directory.CreateDirectory(Path);
    }

    public string Path { get; }

    public string WriteConfig(string fileName, string content)
    {
        string fullPath = System.IO.Path.Combine(Path, fileName);
        File.WriteAllText(fullPath, content);
        return fullPath;
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Path, recursive: true);
        }
        catch (DirectoryNotFoundException)
        {
            // Already gone — nothing to clean up.
        }
    }
}
