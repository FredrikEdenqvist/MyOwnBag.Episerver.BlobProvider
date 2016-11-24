using MongoDB.Bson;

namespace MyOwnBag.Episerver.BlobProviders.Infrastructure
{
    public static class ObjectIdExtensions
    {
        public static bool IsEmpty(this ObjectId id)
        {
            return id.Equals(ObjectId.Empty);
        }

        public static bool IsNotEmpty(this ObjectId id)
        {
            return !id.Equals(ObjectId.Empty);
        }
    }
}