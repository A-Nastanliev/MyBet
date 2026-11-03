using System.Text.Encodings.Web;
using System.Text.Json;
using MyBet.Domain.Validation;
using MyBet.Tests.Support;
using Xunit.Abstractions;

namespace MyBet.Tests.Integration
{
    [Trait("Category", "Ollama")]
    [Collection(OllamaCollection.Name)]
    public class OllamaExtractionTests
    {
        private static readonly JsonSerializerOptions OutputJson = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly OllamaFixture _ollama;
        private readonly ITestOutputHelper _output;

        public OllamaExtractionTests(OllamaFixture ollama, ITestOutputHelper output)
        {
            _ollama = ollama;
            _output = output;
        }

        public static IEnumerable<object[]> Samples => SampleLoader.TheoryRows();

        [SampleTheory]
        [MemberData(nameof(Samples))]
        public async Task Extracted_slip_matches_expected(string sample)
        {
            var expected = SampleLoader.LoadExpected(sample);
            var image = await File.ReadAllBytesAsync(SampleLoader.FindImage(sample));
            var extractor = _ollama.Extractor;

            var slip = await extractor.ExtractAsync(image);

            Assert.NotNull(slip);
            _output.WriteLine($"Model: {_ollama.Model}");
            _output.WriteLine(JsonSerializer.Serialize(slip, OutputJson));

            var diffs = SlipComparer.Compare(expected, slip).ToList();

            for (var i = 0; i < slip.Legs.Count && i < expected.Legs.Count; i++)
            {
                var decided = LegResultEvaluator.Evaluate(slip.Legs[i]);
                var want = expected.Legs[i].Result;

                if (decided is not null && want is not null && decided != want)
                    diffs.Add($"leg {i + 1} result: evaluator says {decided}, expected {want}");

                slip.Legs[i].Result = want;
            }

            slip.Status = expected.Status;
            diffs.AddRange(SlipValidator.Validate(slip).Select(issue => "validator: " + issue));

            Assert.True(diffs.Count == 0,
                $"{sample} ({_ollama.Model}):{Environment.NewLine}- " +
                string.Join(Environment.NewLine + "- ", diffs));
        }
    }
}