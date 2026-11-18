
using Microsoft.Extensions.DependencyInjection;
using TechGuildEventManager.Interfaces;
using TechGuildEventManager.Services;

namespace TechGuildEventManager;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var services = new ServiceCollection();

        // Register the data service
        services.AddSingleton<IGuildDataService, GuildDataService>();

        // Register the main form
        services.AddTransient<Forms.MainForm>();

        // Build the Dependency Injection container
        using var serviceProvider = services.BuildServiceProvider();

        // Start the application using Dependency Injection
        Application.Run(
            serviceProvider.GetRequiredService<Forms.MainForm>());
    }
}
