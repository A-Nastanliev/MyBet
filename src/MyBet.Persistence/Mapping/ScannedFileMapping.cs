using MyBet.Domain.Entities;
using MyBet.Persistence.Models;

namespace MyBet.Persistence.Mapping
{
    internal static class ScannedFileMapping
    {
        public static ScannedFileRecord ToRecord(this ScannedFile file) => new()
        {
            Hash = file.Hash,
            ScannedAt = file.ScannedAt,
            Outcome = file.Outcome
        };
    }
}
