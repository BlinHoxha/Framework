namespace Framework.Contracts.Abstractions;

public interface IEntityContract<out TId>
{
    TId Id { get; }
}

