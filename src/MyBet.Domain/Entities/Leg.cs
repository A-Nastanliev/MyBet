using MyBet.Domain.Enums;

namespace MyBet.Domain.Entities
{
    public class Leg
    {
        public int Id { get; set; }
        public string HomeTeam { get; set; } = "";
        public string AwayTeam { get; set; } = "";
        public string Market { get; set; } = "";
        public string Selection { get; set; } = "";
        public decimal Odds { get; set; }
        public LegResult? Result { get; set; }
        public string? Score { get; set; }
        public bool IsBetBuilder { get; set; }
        public List<SubSelection> SubSelections { get; set; } = new();
    }
}