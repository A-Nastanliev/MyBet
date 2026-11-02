namespace MyBet.Tests.Support
{
    public class TextSimilarityTests
    {
        [Fact]
        public void Identical_text_is_1()
        {
            Assert.Equal(1.0, TextSimilarity.Ratio("Краен Резултат", "Краен Резултат"));
        }

        [Fact]
        public void Case_and_extra_whitespace_are_ignored()
        {
            Assert.Equal(1.0, TextSimilarity.Ratio("  Краен   РЕЗУЛТАТ ", "краен резултат"));
        }

        [Fact]
        public void The_misread_from_the_feasibility_test_stays_above_the_threshold()
        {
            var ratio = TextSimilarity.Ratio("Бодьо/Глимт", "Бодо/Глинт");

            Assert.True(ratio >= SlipComparer.TextThreshold, $"similarity was {ratio:P0}");
        }

        [Fact]
        public void Unrelated_text_is_far_below_the_threshold()
        {
            Assert.True(TextSimilarity.Ratio("Ливърпул", "Арсенал") < 0.5);
        }
    }
}