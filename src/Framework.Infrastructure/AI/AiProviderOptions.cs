namespace Framework.Infrastructure.AI;

public sealed class AiProviderOptions
{
    public const string SectionName = "AI";

    public string Provider { get; init; } = "Local";
}

