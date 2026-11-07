using MyBet.Application.Abstractions;

namespace MyBet.Application.Import
{
    public class ImportService
    {
        private static readonly string[] Extensions = { ".png", ".jpg", ".jpeg", ".webp" };

        private readonly ISlipExtractor _extractor;
        private readonly ISlipRepository _repository;

        public ImportService(ISlipExtractor extractor, ISlipRepository repository)
        {
            _extractor = extractor;
            _repository = repository;
        }

        public async Task<IReadOnlyList<ImportResult>> ImportDirectoryAsync(string directory, CancellationToken ct = default)
        {
            var results = new List<ImportResult>();

            var files = Directory.EnumerateFiles(directory)
                .Where(f => Extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .Order();

            foreach (var file in files)
                results.Add(await ImportFileAsync(file, ct));

            return results;
        }

        private async Task<ImportResult> ImportFileAsync(string file, CancellationToken ct)
        {
            try
            {
                var bytes = await File.ReadAllBytesAsync(file, ct);

                var slip = await _extractor.ExtractAsync(bytes, ct);
                if (slip is null || string.IsNullOrWhiteSpace(slip.SlipId))
                    return new ImportResult(file, ImportOutcome.Failed, null, new[] { "Could not extract a slip number." });

                if (await _repository.ExistsAsync(slip.SlipId, ct))
                    return new ImportResult(file, ImportOutcome.Duplicate, slip.SlipId, Array.Empty<string>());

                await _repository.AddAsync(slip, ct);

                if (!slip.NeedsReview)
                    return new ImportResult(file, ImportOutcome.Imported, slip.SlipId, Array.Empty<string>());

                var issues = new[]
                {
                    $"Status not determined (stake {slip.Stake}, odds {slip.TotalOdds}, payout {slip.Payout}, bonus {slip.BonusPercent}%)."
                };
                return new ImportResult(file, ImportOutcome.NeedsReview, slip.SlipId, issues);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return new ImportResult(file, ImportOutcome.Failed, null, new[] { ex.Message });
            }
        }
    }
}