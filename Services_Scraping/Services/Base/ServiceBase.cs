namespace Services_Scraping.Services.Base
{
    public class ServiceBase<T> : IServiceBase<T>
        where T : BaseEntity
    {
        protected readonly MongoClient _client;
        protected readonly IMongoDatabase? _database = null!;
        protected readonly IMongoCollection<T> _collection = null!;

        public ServiceBase(IMongoClient? client)
        {
            var settings = MongoClientSettings.FromUrl(new MongoUrl(ConfigurationManager.AppSettings["ConnectionString"]));
            settings.SocketTimeout = TimeSpan.FromMinutes(5);

            _client = new MongoClient(settings);
            _database = client!.GetDatabase(ConfigurationManager.AppSettings["DatabaseName"]);
            _collection = _database.GetCollection<T>(ConfigurationManager.AppSettings["ContentCollectionName"]);
        }

        public async Task<long> GetCountAsync()
        {
            var result = await _collection.Find(_ => true).CountDocumentsAsync();
            return result;
        }

        public IEnumerable<T> GetAllAsync() => _collection.Find(_ => true).ToListAsync().Result;

        public async Task<T> GetByIdAsync(string id) => await _collection.Find(entity => entity.Id == id).FirstOrDefaultAsync();

        public async Task AddAsync(T entity) => await _collection.InsertOneAsync(entity);

        public void UpdateAsync(T entity) => _collection.ReplaceOneAsync(e => e.Id == entity.Id, entity);

        public async Task DeleteAsync(string id) => await _collection.DeleteOneAsync(entity => entity.Id == id);

        public async Task<DeleteResult> DeleteAllAsync()
        {
            var filter = new BsonDocument();
            return await _collection.DeleteManyAsync(filter);
        }
    }
}
