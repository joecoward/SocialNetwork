using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetwork.DAL.Neo4j.Abstract
{
    public interface IRelationshipsRepository
    {
        Task CreateRelationshipAsync(string fromNodeId, string toNodeId, string relationshipType);
        Task DeleteRelationshipAsync(string fromNodeId, string toNodeId, string relationshipType);
        Task<int> ShortestPath(string fromNodeId, string toNodeId, string relationshipType);
    }
}
