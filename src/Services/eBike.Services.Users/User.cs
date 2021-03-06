using MongoDB.Bson.Serialization.Attributes;
using System;

namespace eBike.Services.Users
{
    public class User
    {
        [BsonId]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public UserGarage Garage { get; set; }
    };

    public class UserGarage
    {
        public int[] Bikes { get; set; } = Array.Empty<int>();
    }
}