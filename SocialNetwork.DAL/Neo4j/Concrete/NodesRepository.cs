using Neo4j.Driver;
using SocialNetwork.Core.Models;
using SocialNetwork.DAL.Neo4j.Abstract;
using SocialNetwork.DAL.Neo4j.NeoContext;

namespace SocialNetwork.DAL.Neo4j.Concrete
{
    public class NodesRepository : INodesRepository
    {
        private readonly IAsyncSession _session;
        public NodesRepository(IAsyncSession session) 
        {
            _session = session;
        }
        public Task CreateNodeUserAsync(User entity)
        {
            var query = @"
            CREATE (n:Person {
                id: $id, 
                email: $email, 
                firstName: $firstName, 
                lastName: $lastName
            })";
            var parameters = new
            {
                id = entity.Id,
                email = entity.Email,
                firstName = entity.FirstName,
                lastName = entity.LastName
            };
            return _session.RunAsync(query, parameters);
        }

        public Task DeleteNodeAsync(string id)
        {
            var query = "MATCH (n {id: $id})" +
                " DETACH DELETE n";
            var parameters = new { id = id };
            return _session.RunAsync(query, parameters);
        }

        public async Task<List<User>> GetAllNodesAsync()
        {
            var nodes = new List<User>();
            var cursor = await _session.RunAsync("MATCH (n) RETURN n");
            await cursor.ForEachAsync(record =>
            {
                var node = record["n"].As<INode>();
                var user = new User
                {
                    Id = node.Properties["id"].As<string>(),
                    Email = node.Properties["email"].As<string>(),
                    FirstName = node.Properties["firstName"].As<string>(),
                    LastName = node.Properties["lastName"].As<string>(),
                };
                nodes.Add(user);
            });
            return nodes;


        }

        public Task<User> GetNodeByIdAsync(string id)
        {
            var query = "MATCH (n {id: $id}) RETURN n";
            var parameters = new { id = id };
            return _session.RunAsync(query, parameters)
                .ContinueWith(async t =>
                {
                    var cursor = await t;
                    var record = await cursor.SingleAsync();
                    var node = record["n"].As<INode>();
                    return new User
                    {
                        Id = node.Properties["id"].As<string>(),
                        Email = node.Properties["email"].As<string>(),
                        FirstName = node.Properties["firstName"].As<string>(),
                        LastName = node.Properties["lastName"].As<string>(),
                    };
                }).Unwrap();
        }

        public async Task UpdateNodeAsync(User entity)
        {
            // 1. Знаходимо вузол за нашим ID
            var query = @"
                MATCH (n:Person {id: $id})
                SET n.email = $email,
                    n.firstName = $firstName,
                    n.lastName = $lastName";

            // 2. Передаємо нові значення як параметри
            var parameters = new
            {
                id = entity.Id,
                email = entity.Email,
                firstName = entity.FirstName,
                lastName = entity.LastName
            };

            await _session.RunAsync(query, parameters);
        }
    }
}
