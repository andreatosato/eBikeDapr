using System;

namespace eBike.Commons.Events
{
    public class BikeAggregatorEvent
    {
        public DateTime EventDate { get; set; }
        public string Country { get; set; }
    }
}
