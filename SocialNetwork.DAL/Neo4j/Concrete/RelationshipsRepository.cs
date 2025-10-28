using Neo4j.Driver;
using SocialNetwork.DAL.Neo4j.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace SocialNetwork.DAL.Neo4j.Concrete
{
    public class RelationshipsRepository : IRelationshipsRepository
    {
        private readonly IAsyncSession _session;
        public RelationshipsRepository(IAsyncSession session)
        {
            _session = session;
        }
        public Task CreateRelationshipAsync(string fromNodeId, string toNodeId, string relationshipType)
        {
            var query = $@"
            MATCH (a:Person {{id: $fromNodeId}})
            MATCH (b:Person {{id: $toNodeId}})
            MERGE (a)-[r:{relationshipType}]->(b)";
            var parameters = new
            {
                fromNodeId = fromNodeId,
                toNodeId = toNodeId
            };
            return _session.RunAsync(query, parameters);
        }

        public Task DeleteRelationshipAsync(string fromNodeId, string toNodeId, string relationshipType)
        {
           var query = $@"
            MATCH (a:Person {{id: $fromNodeId}})-[r:{relationshipType}]->(b:Person {{id: $toNodeId}})
            DELETE r";
            var parameters = new
            {
                fromNodeId = fromNodeId,
                toNodeId = toNodeId
            };
            return _session.RunAsync(query, parameters);
        }

        public async Task<int> ShortestPath(string fromNodeId, string toNodeId, string relationshipType)
        {
            var query = $@"MATCH
                        (a:Person {{id: $fromNodeId}}),
                        (b:Person {{id: $toNodeId}}),
                        p = shortestPath((a)-[:{relationshipType}*]-(b))
                        RETURN length(p) AS pathLength";
            var parameters = new
            {
                fromNodeId = fromNodeId,
                toNodeId = toNodeId
            };
            var cursor = await _session.RunAsync(query, parameters);

            var record = await cursor.SingleAsync();

            return record["pathLength"].As<int>();
        }
    }
}
