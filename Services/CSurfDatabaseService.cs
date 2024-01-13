using CSurf.Models;
using CSurf.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CSurf.Database
{
    public class CSurfDatabaseService
    {
        private readonly IMongoDatabase _mongoDatabase;
        private CSurfCache _cache;

        public CSurfDatabaseService(string connectionString, string databaseName)
        {
            try
            {
                var mongoClient = new MongoClient(connectionString);
                _mongoDatabase = mongoClient.GetDatabase(databaseName);

                _cache = new CSurfCache();
                _cache.RegisteredSurfPlayers = _mongoDatabase.GetCollection<CSurfPlayer>("registered_players").Find(new BsonDocument()).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("<[DB-Error]> " + ex.ToString());
            }
        }
        public void UpdateDatabase<T>(List<T> cacheList, string collectionName)
        {
            int toUpdate = 0;
            int toInsert = 0;
            cacheList.ForEach(x => {
                if (_mongoDatabase.GetCollection<T>(collectionName).AsQueryable().ToList().Exists(y => ((dynamic)y).Id == ((dynamic)x).Id)) {
                    toUpdate++;
                    UpdateRecord(collectionName, x);
                }
                else
                {
                    if(((dynamic)x).Id == ObjectId.Empty)
                    {
                        toInsert++;
                        InsertRecord(collectionName, x);
                    }
                }          
            });
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine($"[CSurf] Database updated {toUpdate} Documents and inserted {toInsert} Documents from Cache [{collectionName}]");
            Console.ResetColor();
        }

        public CSurfCache GetCache()
        {
            return _cache;
        }

        public void InsertRecord<T>(string col, T record)
        {
            var collection = _mongoDatabase.GetCollection<T>(col);
            collection.InsertOne(record);
        }

        public void UpdateRecord<T>(string col, T record)
        {
            var collection = _mongoDatabase.GetCollection<T>(col);
            var filter = Builders<T>.Filter.Eq("Id", ((dynamic)record).Id);
            collection.ReplaceOne(filter, record);
        }

        public List<T> LoadRecords<T>(string col)
        {
            var collection = _mongoDatabase.GetCollection<T>(col);
            return collection.Find(new BsonDocument()).ToList();
        }
    }
}