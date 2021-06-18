using System;
using System.Collections.Generic;

public record User (Guid Id, string Name, string Surname, List<UserGarage> Garage);
public record UserGarage (int[] Bikes);