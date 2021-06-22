using System.Collections.Generic;

namespace eBike.Web.Data
{
    public class BikeViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }

    public class UserViewModel
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
    }

    public class BaseViewModel
    {
        public UserViewModel User { get; set; } = new();
    }

    public class UserInsertViewModel : BaseViewModel
    {
        public BikeViewModel Bike { get; set; } = new();
    }

    public class UserReadViewModel : BaseViewModel
    {
        public List<BikeViewModel> Garage { get; set; } = new();
    }

    public record AggregatorResponseUserBike (string UserId, string BikeId);

    public record AggregatorBikes (string Country, int Count);
}
