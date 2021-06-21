using System;

namespace eBike.Commons.Events
{
    public class BikeEvent
    {
        public DateTime EventDate { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public BikeStatus Status { get; set; }
    }

    public enum BikeStatus
    {
        Created,
        Updated,
        Deleted,
        Processing,
        Aggregate
    }
}
