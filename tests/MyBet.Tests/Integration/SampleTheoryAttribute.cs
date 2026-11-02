using MyBet.Tests.Support;

namespace MyBet.Tests.Integration
{
    public sealed class SampleTheoryAttribute : TheoryAttribute
    {
        public SampleTheoryAttribute()
        {
            if (SampleLoader.Names().Count == 0)
                Skip = "No samples yet. Add <name>.png and <name>.expected.json to tests/MyBet.Tests/Samples.";
        }
    }
}