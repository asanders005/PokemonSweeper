using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using PokemonSweeper.Game.Field.API;

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
                .AddUserSecrets<App>()
                .Build();

            var services = new ServiceCollection();

            // Register configuration
            services.AddSingleton<IConfiguration>(configuration);

            // Register MongoDB client and database
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
            services.AddSingleton(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase("Pokemon");
            });

            // Register application services
            services.AddSingleton<PokeApiService>();
            services.AddTransient<GameWindow>();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<GameWindow>();
            mainWindow.Show();
        }
    }
}