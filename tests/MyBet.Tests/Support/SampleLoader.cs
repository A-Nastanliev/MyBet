using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyBet.Tests.Support
{
    public static class SampleLoader
    {
        private const string ExpectedSuffix = ".expected.json";
        private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".webp" };

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        public static string SamplesPath =>
            Environment.GetEnvironmentVariable("MYBET_SAMPLES") is { Length: > 0 } custom
                ? custom
                : Path.Combine(AppContext.BaseDirectory, "Samples");

        public static IReadOnlyList<string> Names()
        {
            if (!Directory.Exists(SamplesPath)) return Array.Empty<string>();

            return Directory.EnumerateFiles(SamplesPath, "*" + ExpectedSuffix)
                .Select(file => Path.GetFileName(file)[..^ExpectedSuffix.Length])
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static IEnumerable<object[]> TheoryRows() => Names().Select(name => new object[] { name });

        public static ExpectedSlip LoadExpected(string name)
        {
            var path = Path.Combine(SamplesPath, name + ExpectedSuffix);
            return JsonSerializer.Deserialize<ExpectedSlip>(File.ReadAllText(path), JsonOptions)
                ?? throw new InvalidOperationException($"{path} is empty.");
        }

        public static string FindImage(string name)
        {
            foreach (var extension in ImageExtensions)
            {
                var path = Path.Combine(SamplesPath, name + extension);
                if (File.Exists(path)) return path;
            }

            throw new FileNotFoundException(
                $"No screenshot for sample '{name}'. Put {name}.png (or .jpg/.jpeg/.webp) next to {name}{ExpectedSuffix}.");
        }
    }
}