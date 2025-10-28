using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Neo4j.Driver;
using SocialNetwork.BLL.Abstract;
using SocialNetwork.BLL.Concrete;
using SocialNetwork.DAL.MongoDb.Abstract;
using SocialNetwork.DAL.MongoDb.Concrete;
using SocialNetwork.DAL.MongoDb.MgContext;
using SocialNetwork.DAL.Neo4j.Abstract;
using SocialNetwork.DAL.Neo4j.Concrete;
using SocialNetwork.DAL.Neo4j.NeoContext;
using SocialNetwork.DAL.Settings;

namespace SocialNetwork
{
    public class Program
    {
        static async Task Main(string[] args)
        {

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {

                    config.SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {

                    var configuration = hostContext.Configuration;

                    services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));
                    services.Configure<Neo4jSettings>(configuration.GetSection("Neo4jSettings"));

                   
                    services.AddSingleton<IDriver>(provider =>
                    {
                        var settings = provider.GetRequiredService<IOptions<Neo4jSettings>>().Value;
                        return GraphDatabase.Driver(settings.ConnectionString, AuthTokens.Basic(settings.Username, settings.Password));
                    });

                    
                    services.AddScoped<MongoDbContext>(provider =>
                    {
                        var settings = provider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                        return new MongoDbContext(settings.ConnectionString, settings.DatabaseName);
                    });

                    services.AddScoped<Neo4jContext>(provider =>
                    {
                        var driver = provider.GetRequiredService<IDriver>();
                        var dbName = provider.GetRequiredService<IOptions<Neo4jSettings>>().Value.DatabaseName;
                        return new Neo4jContext(driver, dbName);
                    });

                    services.AddScoped<IAsyncSession>(provider =>
                    {
                        var driver = provider.GetRequiredService<IDriver>();
                        var dbName = provider.GetRequiredService<IOptions<Neo4jSettings>>().Value.DatabaseName;
                        return driver.AsyncSession(cfgBuilder =>cfgBuilder.WithDatabase(dbName));
                    });

                    services.AddScoped<INodesRepository, NodesRepository>();
                    services.AddScoped<IRelationshipsRepository, RelationshipsRepository>();

                    services.AddScoped<IUserRepository, UserRepository>();
                    services.AddScoped<IPostRepository, PostRepository>();

                    
                    services.AddScoped<IUserService, UserService>();
                    services.AddScoped<IPostService, PostService>();

                    
                    services.AddScoped<SocialNetworkApp>();

                })
                .Build();


            await RunApplication(host.Services);
        }

        private static async Task RunApplication(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var neo4jContext = scope.ServiceProvider.GetRequiredService<Neo4jContext>();
                var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var app = scope.ServiceProvider.GetRequiredService<SocialNetworkApp>();
                try
                {

                    await neo4jContext.CobectionTest();

                    await userRepo.CreateEmailIndexAsync();
                    Console.WriteLine("MongoDB email index ensured.");

                    await app.RunAsync();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Critical error during startup: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }
    }
}