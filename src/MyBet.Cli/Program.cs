using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyBet.Application.Abstractions;
using MyBet.Domain.Validation;
using MyBet.Infrastructure;
using System.Text.Json.Serialization;

if (args.Length == 0)
{
    Console.WriteLine("Usage: MyBet.Cli <image path>");
    return 1;
}

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});
builder.Services.AddInfrastructure(builder.Configuration);

using var host = builder.Build();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    var extractor = host.Services.GetRequiredService<ISlipExtractor>();
    var slip = await extractor.ExtractAsync(await File.ReadAllBytesAsync(args[0], cts.Token), cts.Token);

    if (slip is null)
    {
        Console.WriteLine("Extraction failed.");
        return 1;
    }

    foreach (var leg in slip.Legs.Where(l => l.Result is null))
        leg.Result = LegResultEvaluator.Evaluate(leg);

    slip.Status ??= LegResultEvaluator.ResolveStatus(slip.Legs);

    var issues = SlipValidator.Validate(slip);
    slip.NeedsReview = issues.Count > 0;

    var json = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };
    Console.WriteLine(JsonSerializer.Serialize(slip, json));

    Console.WriteLine(issues.Count == 0 ? "VALID" : "NEEDS REVIEW:\n- " + string.Join("\n- ", issues));
    return 0;
}
catch (OperationCanceledException)
{
    Console.WriteLine("Cancelled.");
    return 130;
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return 1;
}