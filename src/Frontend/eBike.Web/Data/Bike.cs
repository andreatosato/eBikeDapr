using System.Collections.Generic;

namespace eBike.Web.Data
{
    public class BikeViewModel
    {
        public string Name { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }

    public class UserBaseViewModel
    {
        public string Name { get; set; }
        public string Surname { get; set; }
    }

    public class UserInsertViewModel : UserBaseViewModel
    {
        public BikeViewModel Bike { get; set; } = new();
    }

    public class UserReadViewModel : UserBaseViewModel
    {
        public List<BikeViewModel> Bikes { get; set; } = new();
    }

    public record AggregatorResponseUserBike (string UserId, string BikeId);
}
