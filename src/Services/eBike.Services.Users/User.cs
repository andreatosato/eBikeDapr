using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

public record User (string Name, string Surname, List<UserGarage> Garage)
{
    [BsonId]
    public Guid Id { get; set; }
};
public record UserGarage (int[] Bikes);