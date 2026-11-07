using MyBet.Domain.Validation;
using MyBet.Tests.Integration;

namespace MyBet.Tests.Support
{
    public class ExpectedSamplesTests
    {
        public static IEnumerable<object[]> Samples => SampleLoader.TheoryRows();

        [SampleTheory]
        [MemberData(nameof(Samples))]
        public void Expected_status_follows_the_money_numbers(string sample)
        {
            var expected = SampleLoader.LoadExpected(sample);

            Assert.Equal(expected.Status, SlipStatusResolver.Resolve(expected.ToExtracted()));
        }

        [SampleTheory]
        [MemberData(nameof(Samples))]
        public void Expected_file_has_a_screenshot_next_to_it(string sample)
        {
            Assert.True(File.Exists(SampleLoader.FindImage(sample)));
        }
    }
}