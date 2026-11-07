using MyBet.Infrastructure.Ollama;

namespace MyBet.Tests.Infrastructure
{
    public class OllamaSlipSchemaTests
    {
        [Fact]
        public void Schema_asks_only_for_the_summary_fields()
        {
            var names = OllamaSlipSchema.Value
                .GetProperty("properties").EnumerateObject().Select(p => p.Name).ToArray();

            Assert.Equal(new[] { "slipId", "stake", "totalOdds", "payout", "bonusPercent" }, names);
        }
    }
}