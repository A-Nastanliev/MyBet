using MyBet.Domain.Validation;
using MyBet.Tests.Integration;

namespace MyBet.Tests.Support
{
    public class ExpectedSamplesTests
    {
        public static IEnumerable<object[]> Samples => SampleLoader.TheoryRows();

        [SampleTheory]
        [MemberData(nameof(Samples))]
        public void Expected_file_obeys_the_odds_and_payout_rules(string sample)
        {
            var issues = SlipValidator.Validate(SampleLoader.LoadExpected(sample).ToSlip());

            Assert.True(issues.Count == 0, $"{sample}: {string.Join("; ", issues)}");
        }

        [SampleTheory]
        [MemberData(nameof(Samples))]
        public void Expected_file_has_a_screenshot_next_to_it(string sample)
        {
            Assert.True(File.Exists(SampleLoader.FindImage(sample)));
        }
    }
}