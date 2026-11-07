namespace MyBet.Application.Abstractions
{
    public interface IDatabaseManager
    {
        Task EnsureCreatedAsync(CancellationToken ct = default);

        Task ResetAsync(CancellationToken ct = default);
    }
}
