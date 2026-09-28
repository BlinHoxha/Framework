namespace Framework.Contracts.Diagnostics;

public sealed record HealthCheckResponse(
    string Status,
    DateTimeOffset TimestampUtc,
    IReadOnlyDictionary<string, string> Dependencies);

