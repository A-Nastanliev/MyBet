namespace MyBet.Infrastructure.Ollama
{
    public static class OllamaPrompt
    {
        public const string Text = """
            Read this sports bet slip. Ignore the individual legs, only read the summary numbers at the bottom.

            - slipId: the slip reference printed after № or Ref (a number, a GUID like BD480B5B-3547-4518-8CCE-2E1B1895B9F7, or a code ending in ==). Copy it exactly, keeping every character and its upper or lower case. Do not include the № or Ref label.
            - stake: the amount on the line labeled Залог.
            - totalOdds: the odds of the whole slip. It is on the line labeled Общ коефициент, or on its own line directly under a line like Двойни, 1 залог × 1.00 € (for example 14.16). It is not the stake and not the odds of a single leg.
            - payout: the amount on the line labeled Изплащане. If there is no such line, the amount after Печалба or Кешаут in the bottom block. Return 0 only if you can read 0.00 printed on that line. null if there is no such line or it is covered.
            - bonusPercent: the percentage in the +N% бонус line, as a number (10 for +10%). null if there is no bonus line.

            Never guess. If a value is covered, blurred, cut off or only partly visible, use null for it. Do not work a number out from other parts of the slip.
            """;
    }
}