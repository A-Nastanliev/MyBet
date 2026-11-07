using System.Text.Encodings.Web;
using System.Text.Json;
using MyBet.Domain.Enums;
using MyBet.Domain.Validation;
using MyBet.Tests.Support;
using System.Text.Json.Serialization;
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
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() }
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

            var slip = await _ollama.Extractor.ExtractAsync(image);

            Assert.NotNull(slip);
            _output.WriteLine($"Model: {_ollama.Model}");
            _output.WriteLine(JsonSerializer.Serialize(slip, OutputJson));

            var diffs = SlipComparer.Compare(expected, slip).ToList();

            if (slip.Status != expected.Status)
                diffs.Add($"status: expected {expected.Status}, got {slip.Status}");

            Assert.True(diffs.Count == 0,
                $"{sample} ({_ollama.Model}):{Environment.NewLine}- " +
                string.Join(Environment.NewLine + "- ", diffs));
        }
    }
}