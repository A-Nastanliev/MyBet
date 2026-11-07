using MyBet.Application.Abstractions;
using MyBet.Domain.Entities;
using MyBet.Domain.Enums;

namespace MyBet.Application.Import
{
    public class ImportService
    {
        private static readonly string[] Extensions = { ".png", ".jpg", ".jpeg", ".webp" };

        private readonly ISlipExtractor _extractor;
        private readonly ISlipRepository _slips;
        private readonly IScannedFileRepository _scannedFiles;
        private readonly IReviewQueueRepository _reviewQueue;
        private readonly IUnitOfWork _unitOfWork;

        public ImportService(
            ISlipExtractor extractor,
            ISlipRepository slips,
            IScannedFileRepository scannedFiles,
            IReviewQueueRepository reviewQueue,
            IUnitOfWork unitOfWork)
        {
            _extractor = extractor;
            _slips = slips;
            _scannedFiles = scannedFiles;
            _reviewQueue = reviewQueue;
            _unitOfWork = unitOfWork;
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
                var hash = FileHash.Compute(bytes);

                if (await _scannedFiles.ExistsAsync(hash, ct))
                    return new ImportResult(file, ImportOutcome.AlreadyScanned, null, Array.Empty<string>());

                var extracted = await _extractor.ExtractAsync(bytes, ct);
                if (extracted is null)
                    return new ImportResult(file, ImportOutcome.Failed, null, new[] { "The model returned nothing." });

                if (extracted.SlipId.Length > 0 && await _slips.ExistsAsync(extracted.SlipId, ct))
                {
                    await RecordAsync(hash, ScanOutcome.Duplicate, ct);
                    return new ImportResult(file, ImportOutcome.Duplicate, extracted.SlipId, Array.Empty<string>());
                }

                if (extracted.NeedsReview)
                {
                    _reviewQueue.Add(new ReviewItem { FileHash = hash, FilePath = file, Extracted = extracted });
                    await RecordAsync(hash, ScanOutcome.Queued, ct);
                    return new ImportResult(file, ImportOutcome.NeedsReview, SlipIdOrNull(extracted), DescribeIssues(extracted));
                }

                _slips.Add(extracted.ToSlip());
                await RecordAsync(hash, ScanOutcome.Saved, ct);
                return new ImportResult(file, ImportOutcome.Imported, extracted.SlipId, Array.Empty<string>());
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return new ImportResult(file, ImportOutcome.Failed, null, new[] { ex.Message });
            }
        }

        /// <summary>Records the file and stores everything staged for it in one save.</summary>
        private async Task RecordAsync(string hash, ScanOutcome outcome, CancellationToken ct)
        {
            _scannedFiles.Add(new ScannedFile { Hash = hash, ScannedAt = DateTimeOffset.UtcNow, Outcome = outcome });
            await _unitOfWork.SaveChangesAsync(ct);
        }

        private static string? SlipIdOrNull(ExtractedSlip slip) => slip.SlipId.Length > 0 ? slip.SlipId : null;

        private static string[] DescribeIssues(ExtractedSlip slip)
        {
            var issues = new List<string>();

            if (slip.SlipId.Length == 0)
                issues.Add("No slip reference was read.");

            if (slip.Status is null)
                issues.Add($"Status not determined (stake {slip.Stake}, odds {slip.TotalOdds}, payout {slip.Payout}, bonus {slip.BonusPercent}%).");

            return issues.ToArray();
        }
    }
}
