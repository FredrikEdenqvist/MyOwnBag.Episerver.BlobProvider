using System;
using System.IO;

namespace MyOwnBag.Episerver.BlobProviders.Infrastructure
{
    public interface IFileActions
    {
        Stream GetWriter(Uri url);
        Stream GetReader(Uri uri);
        void Delete(Uri uri);
    }
}