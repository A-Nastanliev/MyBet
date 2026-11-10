using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyBet.Application;
using MyBet.Cli;
using MyBet.Infrastructure;
using MyBet.Persistence;

var dataDirectory = UserDataDirectory.Resolve();

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    ContentRootPath = AppContext.BaseDirectory
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddUserSettings(dataDirectory);
builder.Services.AddPersistence(Path.Combine(dataDirectory, "mybet.db"));

using var host = builder.Build();

return await RootCommandFactory.Create(host.Services).Parse(args).InvokeAsync();