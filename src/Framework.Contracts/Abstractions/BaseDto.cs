namespace Framework.Contracts.Abstractions;

public abstract class BaseDto<TId> : IEntityContract<TId>
{
    public TId Id { get; set; } = default!;
}
