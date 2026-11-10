using Microsoft.Extensions.DependencyInjection;
using MyBet.Application.Abstractions;
using MyBet.Application.HomeFolder;
using MyBet.Application.Stats;
using MyBet.Application.Update;
using MyBet.Cli.Commands;
using MyBet.Domain.Enums;
using System.CommandLine;

namespace MyBet.Cli
{
    public static class RootCommandFactory
    {
        public static RootCommand Create(IServiceProvider services)
        {
            var folder = new Option<string?>("--folder", "-f")
            {
                Description = "Set the home folder (a different folder wipes the database). Without a path: show it.",
                Arity = ArgumentArity.ZeroOrOne
            };
            var update = new Option<bool>("--update", "-u") { Description = "Scan the home folder for new screenshots." };
            var add = new Option<string?>("--add", "-a") { Description = "Copy an image or a folder of images into the home folder." };
            var review = new Option<bool>("--review", "-x") { Description = "Go through the review queue." };
            var yes = new Option<bool>("--yes", "-y") { Description = "Skip the confirmation when -f wipes the database." };

            var stakes = new Option<bool>("--stakes", "-s") { Description = "Show total stakes." };
            var returns = new Option<bool>("--returns", "-r") { Description = "Show total money returned." };
            var profit = new Option<bool>("--profit", "-p") { Description = "Show profit (returns minus stakes)." };

            var won = new Option<bool>("--won", "-w") { Description = "Include won slips." };
            var lost = new Option<bool>("--lost", "-l") { Description = "Include lost slips." };
            var cashed = new Option<bool>("--cashed", "-c") { Description = "Include cashed-out slips." };

            var root = new RootCommand("MyBet: bet slip analyzer");
            foreach (var option in new Option[] { folder, update, add, review, yes, stakes, returns, profit, won, lost, cashed })
                root.Options.Add(option);

            root.Validators.Add(result =>
            {
                var metrics = new Option[] { stakes, returns, profit };
                if (metrics.Count(m => result.GetResult(m) is not null) > 1)
                    result.AddError("Use only one of --stakes, --returns and --profit.");
            });

            root.SetAction(async (parseResult, ct) =>
            {
                await using var scope = services.CreateAsyncScope();
                var sp = scope.ServiceProvider;
                await sp.GetRequiredService<IDatabaseManager>().EnsureCreatedAsync(ct);

                if (parseResult.GetResult(folder) is not null)
                    return await HomeFolderCommand.RunAsync(
                        sp.GetRequiredService<HomeFolderService>(),
                        parseResult.GetValue(folder), parseResult.GetValue(yes), ct);

                if (parseResult.GetValue(update))
                    return await UpdateCommand.RunAsync(sp.GetRequiredService<UpdateService>(), ct);

                if (parseResult.GetValue(add) is not null) return NotImplemented("-a");
                if (parseResult.GetValue(review)) return NotImplemented("-x");

                var statuses = new List<SlipStatus>();
                if (parseResult.GetValue(won)) statuses.Add(SlipStatus.Won);
                if (parseResult.GetValue(lost)) statuses.Add(SlipStatus.Lost);
                if (parseResult.GetValue(cashed)) statuses.Add(SlipStatus.CashedOut);

                StatsMetric? metric =
                    parseResult.GetValue(stakes) ? StatsMetric.Stakes :
                    parseResult.GetValue(returns) ? StatsMetric.Returns :
                    parseResult.GetValue(profit) ? StatsMetric.Profit :
                    null;

                return await StatsCommand.RunAsync(sp.GetRequiredService<StatsService>(), metric, statuses, ct);
            });

            return root;
        }

        private static int NotImplemented(string what)
        {
            Console.WriteLine($"{what} is not implemented yet.");
            return 1;
        }
    }
}