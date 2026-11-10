using MyBet.Application.Abstractions;

namespace MyBet.Application.HomeFolder
{
    public class HomeFolderService
    {
        private readonly ISettingsStore _settings;
        private readonly IDatabaseManager _database;

        public HomeFolderService(ISettingsStore settings, IDatabaseManager database)
        {
            _settings = settings;
            _database = database;
        }

        public Task<string?> GetAsync(CancellationToken ct = default) =>
            _settings.GetFolderAsync(ct);

        public async Task<HomeFolderResult> SetAsync(
            string path, bool wipeConfirmed = false, CancellationToken ct = default)
        {
            var folder = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

            if (!Directory.Exists(folder))
                return new HomeFolderResult(HomeFolderOutcome.NotFound, folder);

            var current = await _settings.GetFolderAsync(ct);

            if (current is not null && string.Equals(current, folder, StringComparison.OrdinalIgnoreCase))
                return new HomeFolderResult(HomeFolderOutcome.Unchanged, current);

            if (current is not null)
            {
                if (!wipeConfirmed)
                    return new HomeFolderResult(HomeFolderOutcome.ConfirmationRequired, folder);

                await _database.ResetAsync(ct);
            }

            await _settings.SetFolderAsync(folder, ct);

            return new HomeFolderResult(
                current is null ? HomeFolderOutcome.Set : HomeFolderOutcome.Wiped, folder);
        }
    }
}