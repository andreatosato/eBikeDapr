using MongoDB.Bson.Serialization.Attributes;
using System;

namespace eBike.Services.Bikes.Entities
{
    public class BikeEntity
    {
        [BsonId]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
