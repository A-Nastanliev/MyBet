using MyBet.Application.Abstractions;

namespace MyBet.Tests.Fakes
{
    public class FakeSettingsStore : ISettingsStore
    {
        public string? Folder { get; set; }

        public Task<string?> GetFolderAsync(CancellationToken ct = default) => Task.FromResult(Folder);

        public Task SetFolderAsync(string folder, CancellationToken ct = default)
        {
            Folder = folder;
            return Task.CompletedTask;
        }
    }
}