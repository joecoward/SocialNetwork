using SocialNetwork.DAL.MongoDb.Abstract;
using SocialNetwork.DAL.Neo4j.Abstract;

namespace SocialNetwork.DAL.MongoDb.Concrete
{
    public class MigrationService : IMigrationService
    {
        private readonly IUserRepository _mongoUserRepo;
        private readonly INodesRepository _neo4jNodesRepo;
        private readonly IRelationshipsRepository _relationshipsRepo;

        public MigrationService(IUserRepository mongoUserRepo, INodesRepository neo4jNodesRepo, IRelationshipsRepository relationshipsRepository)
        {
            _mongoUserRepo = mongoUserRepo;
            _neo4jNodesRepo = neo4jNodesRepo;
            _relationshipsRepo = relationshipsRepository;

        }

        public async Task MigrateRelationshipsAsync()
        {
            var allUsers = await _mongoUserRepo.GetAllAsync();
            foreach (var user in allUsers)
            {
                if(user.Friends.Count!=0)
                {
                    foreach (var friendId in user.Friends)
                    {
                        await _relationshipsRepo.CreateRelationshipAsync(user.Id, friendId, "FRIEND");
                    }
                }
                if(user.Followers.Count!=0)
                {
                    foreach (var followerId in user.Followers)
                    {
                        await _relationshipsRepo.CreateRelationshipAsync(followerId, user.Id, "FOLLOWS");
                    }
                }
            }
        }

        public async Task MigrateUserNodesAsync()
        {

            var allUsers = await _mongoUserRepo.GetAllAsync();

            foreach (var user in allUsers)
            {

                await _neo4jNodesRepo.CreateNodeUserAsync(user);
            }
        }
    }
}
