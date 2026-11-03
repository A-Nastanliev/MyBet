using MyBet.Domain.Enums;

namespace MyBet.Application.Abstractions
{
    public interface IBadgeReader
    {
        IReadOnlyList<LegResult?> ReadLegResults(byte[] imageBytes, int legCount);
    }
}
