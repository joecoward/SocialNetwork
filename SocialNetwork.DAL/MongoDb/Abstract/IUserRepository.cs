using MongoDB.Driver;
using SocialNetwork.Core.Models;

namespace SocialNetwork.DAL.MongoDb.Abstract
{
    public interface IUserRepository : IRepository<User>
    {
        public Task CreateEmailIndexAsync();
        public Task<bool> UserExistsAsync(string email);
        public Task<User> GetUserByEmailAsync(string email);
    }
}
