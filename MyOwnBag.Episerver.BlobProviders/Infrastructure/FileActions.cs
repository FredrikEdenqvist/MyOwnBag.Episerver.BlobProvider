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

            return _fileBucket.OpenDownloadStream(id, new GridFSDownloadOptions
            {
                Seekable = true
            });
        }

        public void Delete(Uri uri)
        {
            var id = GetId(uri);

            if (id.IsNotEmpty())
                _fileBucket.Delete(id);
        }

        private ObjectId GetId(Uri id)
        {
            // Attempting to matching default index { "filename": 1, "uploadDate": 1 }
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, id.OriginalString);
            var options = new GridFSFindOptions
            {
                Limit = 1,
                Sort = Builders<GridFSFileInfo>.Sort.Ascending(x => x.UploadDateTime)
            };

            using (var cursor = _fileBucket.Find(filter, options))
            {
                return cursor.ToList().FirstOrDefault()?.Id ?? ObjectId.Empty;
            }
        }

        public static FileActions GetFileAction(string mongoDbConnectionString, string database, string bucketName)
        {
            var mongoclient = string.IsNullOrEmpty(mongoDbConnectionString)
                                ? new MongoClient()
                                : new MongoClient(mongoDbConnectionString);

            return new FileActions(new GridFSBucket(mongoclient.GetDatabase(database ?? DefaultDatabase), new GridFSBucketOptions { BucketName = bucketName ?? DefaultBucket }));
        }
    }
}
