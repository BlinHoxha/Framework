using System.ComponentModel.DataAnnotations;

namespace Framework.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Provider { get; init; } = "PostgreSql";

    [Range(1, 300)]
    public int CommandTimeoutInSeconds { get; init; } = 30;

    public bool EnableSensitiveDataLogging { get; init; }
}

