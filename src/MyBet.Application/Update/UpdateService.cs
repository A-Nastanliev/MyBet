using MyBet.Application.Abstractions;
using MyBet.Application.Import;

namespace MyBet.Application.Update
{
    public class UpdateService
    {
        private readonly ISettingsStore _settings;
        private readonly ImportService _import;
        private readonly IReviewQueueRepository _reviewQueue;

        public UpdateService(ISettingsStore settings, ImportService import, IReviewQueueRepository reviewQueue)
        {
            _settings = settings;
            _import = import;
            _reviewQueue = reviewQueue;
        }

        public async Task<UpdateResult> RunAsync(IProgress<ImportProgress>? progress = null, CancellationToken ct = default)
        {
            var folder = await _settings.GetFolderAsync(ct);

            if (folder is null)
                return new UpdateResult(UpdateStatus.NoHomeFolder, null, Array.Empty<ImportResult>(), 0);

            if (!Directory.Exists(folder))
                return new UpdateResult(UpdateStatus.FolderMissing, folder, Array.Empty<ImportResult>(), 0);

            var imports = await _import.ImportDirectoryAsync(folder, progress, ct);
            var queued = await _reviewQueue.CountAsync(ct);

            return new UpdateResult(UpdateStatus.Completed, folder, imports, queued);
        }
    }
}