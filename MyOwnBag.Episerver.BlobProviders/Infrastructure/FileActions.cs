using System;
using System.IO;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace MyOwnBag.Episerver.BlobProviders.Infrastructure
{
    public class FileActions : IFileActions
    {
        private const string DefaultDatabase = "EpiServerDatabaseBlob";
        private const string DefaultBucket = "EpiServerBucketBlob";

        private readonly IGridFSBucket _fileBucket;

        public FileActions(IGridFSBucket fileBucket)
        {
            _fileBucket = fileBucket;
        }

        public Stream GetWriter(Uri url)
        {
            return _fileBucket.OpenUploadStream(url.OriginalString);
        }

        public Stream GetReader(Uri uri)
        {
            var id = GetId(uri);

            if (id.IsEmpty())
                throw new FileNotFoundException($"{uri.OriginalString} is not present in database.");

            return _fileBucket.OpenDownloadStream(id, new GridFSDownloadOptions { Seekable = true });
        }

        public void Delete(Uri uri)
        {
            var id = GetId(uri);

            if (id.IsNotEmpty())
                _fileBucket.Delete(id);
        }

        private ObjectId GetId(Uri uri)
        {
            // Attempting to match default index { "filename": 1, "uploadDate": 1 }
            // Ref: https://github.com/mongodb/specifications/blob/master/source/gridfs/gridfs-spec.rst#indexes
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, uri.OriginalString);
            var options = new GridFSFindOptions
            {
                Limit = 1,
                Sort = Builders<GridFSFileInfo>.Sort.Ascending(x => x.UploadDateTime)
            };

            using (var cursor = _fileBucket.Find(filter, options))
            {
                return cursor.FirstOrDefault()?.Id ?? ObjectId.Empty;
            }
        }

        public static IFileActions GetFileAction(string mongoDbConnectionString, string database, string bucketName)
        {
            return new FileActions(new GridFSBucket(GetClient(mongoDbConnectionString).GetDatabase(database ?? DefaultDatabase), new GridFSBucketOptions { BucketName = bucketName ?? DefaultBucket }));
        }

        private static IMongoClient GetClient(string mongoDbConnectionString)
        {
            var mongoclient = new MongoClient();

            if (!string.IsNullOrEmpty(mongoDbConnectionString))
            {
                var settings = MongoClientSettings.FromUrl(new MongoUrl(mongoDbConnectionString));
                mongoclient = new MongoClient(settings);
            }

            return mongoclient;
        }
    }
}
