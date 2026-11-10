namespace MyBet.Application.Abstractions
{
    public interface ISettingsStore
    {
        Task<string?> GetFolderAsync(CancellationToken ct = default);

        Task SetFolderAsync(string folder, CancellationToken ct = default);
    }
}