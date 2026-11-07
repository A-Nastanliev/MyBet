namespace MyBet.Application.Abstractions
{
    /// <summary>
    /// Stores everything staged through the repositories in one transaction, so a slip
    /// and the record of its file are saved together or not at all.
    /// </summary>
    public interface IUnitOfWork
    {
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
