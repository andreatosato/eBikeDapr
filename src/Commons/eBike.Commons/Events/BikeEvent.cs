namespace eBike.Commons.Events
{
    public class BikeEvent
    {
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
