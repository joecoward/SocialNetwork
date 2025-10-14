using SocialNetwork.Core.Models;

namespace SocialNetwork.DAL.Abstract
{
    public interface IPostRepository : IRepository<Post>
    {
        public Task<List<Post>> GetPostsByUserIdAsync(string userId);
    }
}
