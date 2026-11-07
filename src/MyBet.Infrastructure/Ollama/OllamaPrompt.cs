namespace MyBet.Infrastructure.Ollama
{
    public static class OllamaPrompt
    {
        public const string Text = """
            Read this sports bet slip. Ignore the individual legs, only read the summary numbers.

            - slipId: the number after № (digits only)
            - stake: the amount after Залог near the bottom
            - totalOdds: the odds number on its own line directly under the line that looks like Двойни, 1 залог × 1.00 € (for example 14.16). It is not the stake.
            - payout: the amount in the bottom block after Печалба or Кешаут. null if there is none.
            - bonusPercent: the percentage in the +N% бонус line, as a number (10 for +10%). null if there is no bonus line.

            Use null for anything not visible
            """;
    }
}