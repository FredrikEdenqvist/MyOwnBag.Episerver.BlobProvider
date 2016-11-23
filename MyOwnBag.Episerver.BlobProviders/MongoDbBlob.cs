using EPiServer.Framework.Blobs;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.IO;
using System.Linq;

namespace MyOwnBag.Episerver.BlobProviders
{
    public class MongoDbBlob : Blob
    {
        private readonly IGridFSBucket _fileBucket;
        private ObjectId _objId;

        public MongoDbBlob(Uri id, IGridFSBucket fileBucket) : base(id)
        {
            _fileBucket = fileBucket;
            _objId = ObjectId.Empty;
        }

        private ObjectId GetId()
        {
            if (_objId.IsNotEmpty())
                return _objId;

            // Attempting to matching default index { "filename": 1, "uploadDate": 1 }
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, ID.OriginalString);
            var sort = Builders<GridFSFileInfo>.Sort.Ascending(x => x.UploadDateTime);
            var options = new GridFSFindOptions
            {
                Limit = 1,
                Sort = sort
            };

            using (var cursor = _fileBucket.Find(filter, options))
            {
                _objId = cursor.ToList().FirstOrDefault()?.Id ?? ObjectId.Empty;
            }

            return _objId;
        }

        public virtual void Delete()
        {
            if (GetId().IsNotEmpty())
                _fileBucket.Delete(GetId());
        }

        public override Stream OpenRead()
        {
            if (GetId().IsEmpty())
                throw new FileNotFoundException($"{ID.OriginalString} is not present in database.");

            return _fileBucket.OpenDownloadStream(GetId(), new GridFSDownloadOptions
            {
                Seekable = true
            });
        }

        public override Stream OpenWrite()
        {
            var stream = _fileBucket.OpenUploadStream(ID.OriginalString);
            _objId = stream.Id;

            return stream;
        }

        public override void Write(Stream data)
        {
            using (var streamWrite = OpenWrite())
            {
                const int bufferSize = 4096;
                int bufferRead;
                var buffer = new byte[bufferSize];

                while ((bufferRead = data.Read(buffer, 0, bufferSize)) > 0)
                {
                    streamWrite.Write(buffer, 0, bufferRead);
                }

                streamWrite.Flush();
            }
        }
    }
}
