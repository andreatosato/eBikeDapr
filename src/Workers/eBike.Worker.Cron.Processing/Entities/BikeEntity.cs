using MongoDB.Bson.Serialization.Attributes;
using System;

namespace eBike.Workers.Processing.Entities
{
    public class BikeEntity
    {
        [BsonId]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Municipality { get; set; }
        public string PostalCode { get; set; }
    }

    public class BikeAggregation
    {
        [BsonId]
        public string Country { get; set; }
        public int Count { get; set; }
    }
}
