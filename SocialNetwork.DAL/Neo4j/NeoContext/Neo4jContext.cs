using Neo4j.Driver;
using System;

namespace SocialNetwork.DAL.Neo4j.NeoContext
{
    public class Neo4jContext
    {
        private readonly IDriver _driver;
        private readonly string _databaseName;
        public Neo4jContext(IDriver driver, string databaseName)
        {
            _driver = driver;
            _databaseName = databaseName;
        }
        public IDriver Driver => _driver;


        public async Task CobectionTest()
        {
            try
            {
                await _driver.VerifyConnectivityAsync();
                Console.WriteLine("Neo4j connection successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error during startup: {ex.Message}");
                return;
            }
        }

    }
}
