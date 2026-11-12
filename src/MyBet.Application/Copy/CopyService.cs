using MyBet.Application.Abstractions;
using MyBet.Application.Import;

namespace MyBet.Application.Copy
{
    public class CopyService
    {
        private readonly ISettingsStore _settings;

        public CopyService(ISettingsStore settings) => _settings = settings;

        public async Task<CopyResult> CopyAsync(string source, CancellationToken ct = default)
        {
            var home = await _settings.GetFolderAsync(ct);
            if (home is null)
                return Without(CopyStatus.NoHomeFolder, "");

            if (!Directory.Exists(home))
                return Without(CopyStatus.HomeFolderMissing, home);

            var fullSource = Path.TrimEndingDirectorySeparator(Path.GetFullPath(source));

            List<string> files;
            if (File.Exists(fullSource))
            {
                if (!ImageExtensions.IsImage(fullSource))
                    return Without(CopyStatus.NotAnImage, fullSource);

                files = new List<string> { fullSource };
            }
            else if (Directory.Exists(fullSource))
            {
                files = Directory.EnumerateFiles(fullSource)
                    .Where(ImageExtensions.IsImage)
                    .Order()
                    .ToList();
            }
            else
            {
                return Without(CopyStatus.SourceMissing, fullSource);
            }

            var known = await HashesInAsync(home, ct);
            var results = new List<CopyFileResult>();

            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();
                results.Add(await CopyFileAsync(file, home, known, ct));
            }

            return new CopyResult(CopyStatus.Done, home, results);
        }

        private static async Task<CopyFileResult> CopyFileAsync(
            string file, string home, HashSet<string> known, CancellationToken ct)
        {
            try
            {
                var bytes = await File.ReadAllBytesAsync(file, ct);
                var hash = FileHash.Compute(bytes);

                if (known.Contains(hash))
                    return new CopyFileResult(file, CopyOutcome.SkippedSameContent, null, null);

                var destination = UniquePath(home, Path.GetFileName(file));
                await File.WriteAllBytesAsync(destination, bytes, ct);
                known.Add(hash);

                var renamed = !string.Equals(
                    Path.GetFileName(destination), Path.GetFileName(file), StringComparison.OrdinalIgnoreCase);

                return new CopyFileResult(file, renamed ? CopyOutcome.Renamed : CopyOutcome.Copied, destination, null);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return new CopyFileResult(file, CopyOutcome.Failed, null, ex.Message);
            }
        }

        private static async Task<HashSet<string>> HashesInAsync(string home, CancellationToken ct)
        {
            var hashes = new HashSet<string>();

            foreach (var file in Directory.EnumerateFiles(home).Where(ImageExtensions.IsImage))
            {
                try
                {
                    hashes.Add(FileHash.Compute(await File.ReadAllBytesAsync(file, ct)));
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                }
            }

            return hashes;
        }

        private static string UniquePath(string folder, string fileName)
        {
            var path = Path.Combine(folder, fileName);
            if (!File.Exists(path)) return path;

            var name = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);

            for (var n = 2; ; n++)
            {
                path = Path.Combine(folder, $"{name} ({n}){extension}");
                if (!File.Exists(path)) return path;
            }
        }

        private static CopyResult Without(CopyStatus status, string location) =>
            new(status, location, Array.Empty<CopyFileResult>());
    }
}