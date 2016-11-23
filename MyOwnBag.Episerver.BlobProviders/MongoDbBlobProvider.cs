using EPiServer.Framework.Blobs;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Specialized;

namespace MyOwnBag.Episerver.BlobProviders
{
    public class MongoDbBlobProvider : BlobProvider
    {
        private IGridFSBucket _fileBucket;
        private string _mongoBbConnectionString;
        private string _bucketName;
        private string _database;

        private const string ConnectionStringKey = "ConnectionString";
        private const string DatabaseKey = "Database";
        private const string BucketKey = "Bucket";

        private const string DefaultDatabase = "EpiServerDatabaseBlob";
        private const string DefaultBucket = "EpiServerBucketBlob";

        public override void Initialize(string name, NameValueCollection config)
        {
            _mongoBbConnectionString = config.Get(ConnectionStringKey) ?? string.Empty;
            _database = config.Get(DatabaseKey) ?? DefaultDatabase;
            _bucketName = config.Get(BucketKey) ?? DefaultBucket;

            var mongoclient = (string.IsNullOrEmpty(_mongoBbConnectionString))
                ? new MongoClient()
                : new MongoClient(_mongoBbConnectionString);

            _fileBucket = new GridFSBucket(mongoclient.GetDatabase(_database), new GridFSBucketOptions { BucketName = _bucketName });

            base.Initialize(name, config);
        }

        public override Blob CreateBlob(Uri id, string extension)
        {
            return GetMongoDbBlob(Blob.NewBlobIdentifier(id, extension));
        }

        public override void Delete(Uri id)
        {
            GetMongoDbBlob(id).Delete();
        }

        public override Blob GetBlob(Uri id)
        {
            return GetMongoDbBlob(id);
        }

        private MongoDbBlob GetMongoDbBlob(Uri id)
        {
            return new MongoDbBlob(id, _fileBucket);
        }
    }
}
