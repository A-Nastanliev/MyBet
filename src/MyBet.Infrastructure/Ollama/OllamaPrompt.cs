namespace MyBet.Infrastructure.Ollama
{
    public static class OllamaPrompt
    {
        public const string Text = """
        Extract this sports bet slip. Return every leg, in order from top to bottom.

        Slip fields:
        - slipId: the number after № (digits only)
        - placedAt: the date and time under the slip title at the very top, exactly as shown (for example 18:19 ч. 18.10.2026 г.). If the line under the title says На живо, it is not a date: return null. Times and dates inside the legs are match times, never use them for placedAt.
        - type: the slip type in the title (for example Двойни or 9-кратни)
        - stake: the amount after Залог near the bottom
        - totalOdds: the odds number on its own line directly under the line that looks like Двойни, 1 залог × 1.00 € (for example 14.16). It is not the stake.
        - payout: the amount after Печалба: near the bottom
        - bonusPercent: the percentage in the +N% бонус line, as a number (10 for +10%). null if there is no bonus line.
        - bonusAmount: the amount after вкл. бонус at the bottom right. null if there is none.

        Every normal leg has these lines, top to bottom:
        1. the pick in bold on the left, the odds on the right, then a badge
        2. the market name
        3. the match: home team, score, away team
        4. the date and time of the match

        Leg fields, in the order they appear on the slip:
        - selection: the bold pick on line 1 (for example a team name, Да, Над 3.5). The pick is not necessarily one of the two teams.
        - odds: the number on the right of line 1
        - market: line 2, the line below the pick. It is the name of the bet type, never the pick. Copy it exactly as written, from its first character to the end of the line, and never drop the start of it. Examples of market lines: Краен Резултат, Брой Голове 2.5, Хандикап 1:0, Азиатски Хандикап -1.5, Двойна Възможност, or a market that starts with a team name and a dash, like Барселона - Брой Голове 1.5 (keep the team name and the dash). Над and Под are picks, so they are never the start of a market: for the pick Над 2.5 the market line is Брой Голове 2.5.
        - homeTeam: the team to the left of the score on line 3. If the teams are on two lines, the upper team.
        - awayTeam: the team to the right of the score on line 3. If the teams are on two lines, the lower team.
        - score: home score and away score as digits, for example 4 : 3
        - subSelections: an empty list for normal legs

        Bet Builder legs: line 1 reads Създай Залог with the odds. Set market to Създай Залог and selection to null. The match is on line 2. Below it are the nested picks. Each nested pick is two lines: the pick on top (for example Да) and its market name underneath (for example Двата Отбора Да Отбележат). Return exactly one subSelections entry per pair, with selection and market. Never split one pair into two entries.

        Badges (Печалба, Загуба, Изплатени) are never a pick or a market. Ignore them. Use null for anything not visible.
        """;
    }
}