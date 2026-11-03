using MyBet.Domain.Enums;

namespace MyBet.Tests.Support
{
    public class ExpectedLeg
    {
        public string HomeTeam { get; set; } = "";
        public string AwayTeam { get; set; } = "";
        public string Market { get; set; } = "";
        public string Selection { get; set; } = "";
        public decimal Odds { get; set; }
        public string? Score { get; set; }
        public bool IsBetBuilder { get; set; }
        public LegResult? Result { get; set; }
        public List<ExpectedSubSelection> SubSelections { get; set; } = new();
    }
}