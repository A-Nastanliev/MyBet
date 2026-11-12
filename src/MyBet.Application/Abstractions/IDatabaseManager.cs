namespace MyBet.Application.Abstractions
{
    public interface IDatabaseManager
    {
        Task MigrateAsync(CancellationToken ct = default);

        Task ResetAsync(CancellationToken ct = default);
    }
}
