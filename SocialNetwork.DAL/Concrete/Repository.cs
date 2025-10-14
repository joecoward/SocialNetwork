using MongoDB.Bson;
using MongoDB.Driver;
using SocialNetwork.DAL.Abstract;
using SocialNetwork.DAL.MgContext;

namespace SocialNetwork.DAL.Concrete
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly IMongoCollection<TEntity> _collection;
        public Repository(MongoDbContext context , string collectionName)
        {
            _collection = context.Database.GetCollection<TEntity>(collectionName);
        }
        public virtual async Task CreateAsync(TEntity entity)
        {
           await _collection.InsertOneAsync(entity);
        }

        public virtual async Task DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<List<TEntity>> GetAllAsync()
        {
           return await _collection.Find(_ => true).ToListAsync();
        }

        public virtual async Task<TEntity> GetByIdAsync(string id)
        {
            var filter = Builders<TEntity>.Filter.Eq("Id", new ObjectId(id));
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            var idProperty = entity.GetType().GetProperty("Id");
            if (idProperty == null)
            {
                throw new ArgumentException("Entity must have an Id property");
            }
            var idValue = idProperty.GetValue(entity)?.ToString();
            if (string.IsNullOrEmpty(idValue))
            {
                throw new ArgumentException("Entity Id property cannot be null or empty");
            }
            var filter = Builders<TEntity>.Filter.Eq("Id", new ObjectId(idValue));
            await _collection.ReplaceOneAsync(filter, entity);
        }
    }
}
