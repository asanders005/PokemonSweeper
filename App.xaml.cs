using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using PokemonSweeper.API;
using PokemonSweeper.Data;
using PokemonSweeper.Game;

namespace PokemonSweeper
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; } = null!;

        private async void Test_Startup(object sender, StartupEventArgs e)
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
            services.AddSingleton<DAL>();
            services.AddTransient<PokemonDetails>();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<PokemonDetails>();
            mainWindow.Show();
        }
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
            }

            // Register application services
            services.AddSingleton<DAL>();
            services.AddTransient<GameWindow>();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<GameWindow>();
            mainWindow.Show();
        }
    }
}