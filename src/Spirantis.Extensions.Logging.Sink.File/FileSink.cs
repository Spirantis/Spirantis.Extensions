using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Spirantis.Extensions.Logging.Sink.File;

/// <summary>
/// A logging sink that writes to rolling files asynchronously. Enabled by
/// <c>Logging:File:Enabled</c>; the target directory, encoding, and rolling limits are
/// configurable under the <c>Logging:File</c> section.
/// </summary>
public sealed class FileSink : ILoggingSink
{
    private const long DefaultFileSizeLimitBytes = 10_000_000;
    private const int DefaultRetainedFileCountLimit = 31;

    /// <inheritdoc />
    public void Add(LoggerConfiguration loggerConfiguration, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration);

        var section = configuration.GetEnabledSinkSection("File");

        if (section is null)
        {
            return;
        }

        loggerConfiguration.WriteTo.Async(sink =>
            sink.File(
                path: GetFilePath(section),
                outputTemplate: LoggingSinkOptions.StandardOutputTemplate,
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: GetMaxFileSize(section),
                retainedFileCountLimit: GetMaxFileCount(section),
                encoding: GetFileEncoding(section),
                shared: true
            )
        );
    }

    private static string GetFilePath(IConfiguration section)
    {
        string? configuredPath = section["Path"];

        string directory = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(AppContext.BaseDirectory, "Logs")
            : Path.GetFullPath(configuredPath);

        string serviceName = Assembly.GetEntryAssembly()?.GetName().Name ?? "service";
        directory = directory.Replace("{ServiceName}", serviceName, StringComparison.Ordinal);

        Directory.CreateDirectory(directory);

        return Path.Combine(directory, "log.log");
    }

    private static Encoding GetFileEncoding(IConfiguration section)
    {
        string? encodingName = section["Encoding"];

        try
        {
            return string.IsNullOrWhiteSpace(encodingName)
                ? Encoding.UTF8
                : Encoding.GetEncoding(encodingName);
        }
        catch (ArgumentException)
        {
            return Encoding.UTF8;
        }
    }

    private static int GetMaxFileCount(IConfiguration section)
    {
        int configured = section.GetValue<int>("RetainedFileCountLimit");
        return configured > 0 ? configured : DefaultRetainedFileCountLimit;
    }

    private static long GetMaxFileSize(IConfiguration section)
    {
        long configured = section.GetValue<long>("FileSizeLimit");
        return configured > 0 ? configured : DefaultFileSizeLimitBytes;
    }
}
