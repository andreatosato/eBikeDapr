using System.Collections.Generic;

namespace eShop.Web.Data
{
    public class BikeViewModel
    {
        public string Name { get; set; }
        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
    }

    public class UserBaseViewModel
    {
        public string Name { get; set; }
        public string Surname { get; set; }
    }

    public class UserInsertViewModel :  UserBaseViewModel
    {
        public BikeViewModel Bike { get; set; }
    }

    public class UserReadViewModel : UserBaseViewModel
    {
        public List<BikeViewModel> Bikes { get; set; } = new List<BikeViewModel>();
    }
}
