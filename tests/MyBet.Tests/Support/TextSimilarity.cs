namespace MyBet.Tests.Support
{
    public static class TextSimilarity
    {
        public static double Ratio(string? a, string? b)
        {
            var x = Normalize(a);
            var y = Normalize(b);

            if (x.Length == 0 && y.Length == 0) return 1.0;

            return 1.0 - (double)Distance(x, y) / Math.Max(x.Length, y.Length);
        }

        private static string Normalize(string? text) =>
            string.Join(' ', (text ?? "").ToLowerInvariant()
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        private static int Distance(string a, string b)
        {
            var previous = new int[b.Length + 1];
            var current = new int[b.Length + 1];

            for (var j = 0; j <= b.Length; j++) previous[j] = j;

            for (var i = 1; i <= a.Length; i++)
            {
                current[0] = i;
                for (var j = 1; j <= b.Length; j++)
                {
                    var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    current[j] = Math.Min(
                        Math.Min(current[j - 1] + 1, previous[j] + 1),
                        previous[j - 1] + cost);
                }
                (previous, current) = (current, previous);
            }

            return previous[b.Length];
        }
    }
}
