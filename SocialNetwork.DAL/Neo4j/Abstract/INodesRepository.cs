using SocialNetwork.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DAL.Neo4j.Abstract
{
    public interface INodesRepository
    {
        Task CreateNodeUserAsync(User entity);
        Task<User> GetNodeByIdAsync(string id);
        Task<List<User>> GetAllNodesAsync();
        Task UpdateNodeAsync(User entity);
        Task DeleteNodeAsync(string id);
    }
}
