using MyBet.Domain.Enums;

namespace MyBet.Domain.Entities
{
    public class ScannedFile
    {
        public int Id { get; set; }
        public string Hash { get; set; } = "";
        public DateTimeOffset ScannedAt { get; set; }
        public ScanOutcome Outcome { get; set; }
    }
}
