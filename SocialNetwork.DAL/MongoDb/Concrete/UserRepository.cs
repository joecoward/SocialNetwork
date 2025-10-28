using MongoDB.Driver;
using SocialNetwork.Core.Models;
using SocialNetwork.DAL.MongoDb.Abstract;
using SocialNetwork.DAL.MongoDb.MgContext;

namespace SocialNetwork.DAL.MongoDb.Concrete
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(MongoDbContext context) : base(context, "Users")
        {
        }
        public async Task CreateEmailIndexAsync()
        {
            var indexKeysDefinition = Builders<User>.IndexKeys.Ascending(u => u.Email);
            var indexOptions = new CreateIndexOptions { Unique = true, Collation = new Collation("en", strength: CollationStrength.Secondary) };
            var indexModel = new CreateIndexModel<User>(indexKeysDefinition, indexOptions);
            await _collection.Indexes.CreateOneAsync(indexModel);
        }
        public async Task<bool> UserExistsAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var user = await _collection.Find(filter).FirstOrDefaultAsync();
            return user != null;
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
