using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PokemonSweeper.Data;
using PokemonSweeper.Game.Field;
using PokemonSweeper.Game.PokemonModels;
using PokemonSweeper.Services;

namespace PokemonSweeper
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; } = null!;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<App>(optional: true)
                .Build();

            var services = new ServiceCollection();

            // Register configuration
            services.AddSingleton<IConfiguration>(configuration);

            // Register MongoDB only if a connection string is provided 
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString))
            {
                services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
                services.AddSingleton<IMongoDatabase>(sp =>
                {
                    var client = sp.GetRequiredService<IMongoClient>();
                    return client.GetDatabase("Pokemon");
                });
                services.AddSingleton<IDal, MongoDal>();
            }
            else
                services.AddSingleton<IDal, LocalDal>();

            // Register application services
            services.AddSingleton<PokemonTeamService>();
            services.AddTransient<MainMenu>();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainMenu>();
            mainWindow.Show();
        }

        // Method for testing new pages without messing up the original Application_Startup
        private void Test_Startup(object sender, StartupEventArgs e)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<App>(optional: true)
                .Build();

            var services = new ServiceCollection();

            // Register configuration
            services.AddSingleton<IConfiguration>(configuration);

            // Register MongoDB only if a connection string is provided 
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString))
            {
                services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
                services.AddSingleton<IMongoDatabase>(sp =>
                {
                    var client = sp.GetRequiredService<IMongoClient>();
                    return client.GetDatabase("Pokemon");
                });
            }

            // Register application services
            services.AddSingleton<PokemonTeamService>();
            services.AddSingleton<IDal>();
            services.AddTransient<PokemonPC>();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<PokemonPC>();
            mainWindow.Show();
        }
    }
}