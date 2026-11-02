using MyBet.Infrastructure.Ollama;

namespace MyBet.Tests.Infrastructure
{
    public class OllamaSlipSchemaTests
    {
        [Fact]
        public void Leg_properties_follow_the_order_of_the_screenshot()
        {
            var leg = OllamaSlipSchema.Value
                .GetProperty("properties").GetProperty("legs")
                .GetProperty("items").GetProperty("properties");

            var names = leg.EnumerateObject().Select(p => p.Name).ToArray();

            Assert.Equal(
                new[] { "selection", "odds", "market", "homeTeam", "awayTeam", "score", "subSelections" },
                names);
        }
    }
}