using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace FovCalibrationTool
{
    internal partial class ToolBootstrap
    {
        static Task<int> Main(params string[] args)
        {
            var command = new RootCommand
            {
                TreatUnmatchedTokensAsErrors = true
            };

            InitDefaultCommand(command);

            return command.InvokeAsync(args);
        }

        static async Task HandleCommandAsync(InvocationContext commandContext, Action<HostBuilder> configureHost)
        {
            try
            {
                var hostBuilder = new HostBuilder();

                ConfigureCommonHost(hostBuilder);
                configureHost(hostBuilder);

                var host = hostBuilder.Build();
                var hostStoppingToken = commandContext.GetCancellationToken();

                // Start generic host
                await host.RunAsync(
                    hostStoppingToken
                );
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
        }

        static void ConfigureCommonHost(HostBuilder hostBuilder)
        {
            hostBuilder.ConfigureHostConfiguration(builder =>
            {
                // File configuration
                builder.AddJsonFile("config.json", true);
            });

            hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddLogging(builder =>
                {
                    // Load configuration from logging section
                    builder.AddConfiguration(context.Configuration.GetSection("Logging"));

                    // Register loggers
                    builder.AddConsole();
                });

                // Configure common services
                ConfigureCommonServices(services);
            });
        }

        static void ConfigureCommonServices(IServiceCollection services)
        {

        }
    }
}