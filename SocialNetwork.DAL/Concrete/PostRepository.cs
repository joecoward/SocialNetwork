using MongoDB.Driver;
using SocialNetwork.Core.Models;
using SocialNetwork.DAL.Abstract;
using SocialNetwork.DAL.MgContext;

namespace SocialNetwork.DAL.Concrete
{
    public class PostRepository :Repository<Post> ,IPostRepository
    {
        public PostRepository(MongoDbContext context) : base(context, "Posts")
        {
        }

        public Task<List<Post>> GetPostsByUserIdAsync(string userId)
        {
            var filter = Builders<Post>.Filter.Eq("UserId", userId);
            return _collection.Find(filter).ToListAsync();
        }
    }
}
