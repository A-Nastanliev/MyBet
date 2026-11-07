using MyBet.Domain.Enums;

namespace MyBet.Persistence.Models
{
    public class ScannedFileRecord
    {
        public int Id { get; set; }
        public string Hash { get; set; } = "";
        public DateTimeOffset ScannedAt { get; set; }
        public ScanOutcome Outcome { get; set; }
    }
}
