using EPiServer.Framework.Blobs;
using System;
using System.IO;
using MyOwnBag.Episerver.BlobProviders.Infrastructure;

namespace MyOwnBag.Episerver.BlobProviders
{
    public class MongoDbBlob : Blob
    {
        private readonly IFileActions _fileActions;

        public MongoDbBlob(Uri id, IFileActions fileActions) : base(id)
        {
            _fileActions = fileActions;
        }

        public virtual void Delete()
        {
            _fileActions.Delete(ID);
        }

        public override Stream OpenRead()
        {
            return _fileActions.GetReader(ID);
        }

        public override Stream OpenWrite()
        {
            return _fileActions.GetWriter(ID);
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
