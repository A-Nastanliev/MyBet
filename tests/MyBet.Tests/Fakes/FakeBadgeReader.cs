using MyBet.Application.Abstractions;
using MyBet.Domain.Enums;

namespace MyBet.Tests.Fakes
{
    public class FakeBadgeReader : IBadgeReader
    {
        public IReadOnlyList<LegResult?>? Results { get; set; }

        public IReadOnlyList<LegResult?> ReadLegResults(byte[] imageBytes, int legCount) =>
            Results ?? Enumerable.Repeat<LegResult?>(LegResult.Won, legCount).ToList();
    }
}
    