using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace eBike.Workers.Processing
{
    public class AggregateBikes
    {
        private readonly HttpClient httpClient;
        public AggregateBikes (IHttpClientFactory httpClientFactory)
        {
            httpClient = httpClientFactory.CreateClient("AzureMaps");
        }

        public async Task ExecuteAsync ()
        {

        }

        public async Task<GpsInformation> GetAddressAsync (double latitude, double longitude)
        {
            var AZURE_MAPS_KEY = Environment.GetEnvironmentVariable("AZURE_MAPS_KEY");
            var reverseGeocodingResponse = await httpClient.GetAsync(
                $"https://atlas.microsoft.com/search/address/reverse/json?language=en-US&subscription-key={AZURE_MAPS_KEY}&api-version=1.0&query={latitude.ToString().Replace(",", ".")},{longitude.ToString().Replace(",", ".")}");
            if (reverseGeocodingResponse.IsSuccessStatusCode) {
                var reverseGeocodingJson = await reverseGeocodingResponse.Content.ReadAsStringAsync();

                var data = await JsonDocument.ParseAsync(await reverseGeocodingResponse.Content.ReadAsStreamAsync());
                var address = data.RootElement.GetProperty("addresses").EnumerateArray().ToList().FirstOrDefault();
                //var data = JToken.Parse(reverseGeocodingJson);
                //var address = JsonConvert.DeserializeObject<Address>(data["addresses"].FirstOrDefault()["address"].ToString());

                return new GpsInformation();
            }
            return null;
        }
    }

    public class GpsInformation
    {
        public string Country { get; set; }
        public string City { get; set; }
    }
}
