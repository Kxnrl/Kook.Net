﻿using Kook;
using Kook.Net.Samples.ReactionRoleBot.Configurations;
using Kook.Net.Samples.ReactionRoleBot.Extensions;
using Kook.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Log.Information("Starting Kook.Net API Helper");

        using IHost host = CreateHostBuilder(args).Build();

        await host.RunAsync();
    }
    
    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
            //.AddGlobalErrorHandler()
            .ConfigureServices(ConfigureServices)
            .UseSerilog((hostingContext, services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .MinimumLevel.Verbose()
                    //.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Verbose);
            });
        return hostBuilder;
    }

    /// <summary>
    ///     配置服务
    /// </summary>
    /// <param name="hostContext"></param>
    /// <param name="services"></param>
    private static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
    {
        // 根配置
        IConfigurationRoot configurationRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false)
            .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true, true)
            .Build();

        // KOOK Webhook 配置
        KookBotConfigurations kookBotConfigurations = new();
        configurationRoot.GetSection(nameof(KookBotConfigurations))
            .Bind(kookBotConfigurations);

        // 服务配置
        services
            // 静态配置
            .AddSingleton(kookBotConfigurations)

            // KOOK 客户端程序
            .AddSingleton<KookBotClientExtension>()
            .AddHostedService(p => p.GetRequiredService<KookBotClientExtension>())
            .AddSingleton(_ =>
                new KookSocketClient(new KookSocketConfig
                {
                    AlwaysDownloadUsers = true,
                    LogLevel = LogSeverity.Debug
                }))

            .AddHttpClient();
    }
}