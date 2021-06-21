using System;

namespace eBike.Commons.Events
{
    public class UserEvent
    {
        public DateTime EventDate { get; set; }
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
