namespace MyBet.Infrastructure.Ollama.Dtos
{
    public class LegDto
    {
        public string? HomeTeam { get; set; }
        public string? AwayTeam { get; set; }
        public string? Market { get; set; }
        public string? Selection { get; set; }
        public decimal? Odds { get; set; }
        public string? Score { get; set; }
        public List<SubSelectionDto>? SubSelections { get; set; }
    }
}
