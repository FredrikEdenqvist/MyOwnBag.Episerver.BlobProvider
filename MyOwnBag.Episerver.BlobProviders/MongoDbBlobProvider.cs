using EPiServer.Framework.Blobs;
using System;
using System.Collections.Specialized;
using MyOwnBag.Episerver.BlobProviders.Infrastructure;

namespace MyOwnBag.Episerver.BlobProviders
{
    public class MongoDbBlobProvider : BlobProvider
    {
        private IFileActions _fileActions;

        private const string ConnectionStringKey = "ConnectionString";
        private const string DatabaseKey = "Database";
        private const string BucketKey = "Bucket";

        public override void Initialize(string name, NameValueCollection config)
        {
            _fileActions = FileActions.GetFileAction(config.Get(ConnectionStringKey), config.Get(DatabaseKey), config.Get(BucketKey));
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
            return new MongoDbBlob(id, _fileActions);
        }
    }
}
