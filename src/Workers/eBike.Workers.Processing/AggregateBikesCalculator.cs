using eBike.Workers.Processing.Entities;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace eBike.Workers.Processing
{
    public class AggregateBikesCalculator
    {
        private readonly HttpClient httpClient;
        private readonly IMongoCollection<BikeEntity> bikeCollection;
        private readonly IMongoCollection<BikeAggregation> bikeAggregationCollection;

        public AggregateBikesCalculator (IHttpClientFactory httpClientFactory,
            IMongoCollection<BikeEntity> bikeCollection,
            IMongoCollection<BikeAggregation> bikeAggregationCollection)
        {
            httpClient = httpClientFactory.CreateClient("AzureMaps");
            this.bikeCollection = bikeCollection;
            this.bikeAggregationCollection = bikeAggregationCollection;
        }

        public async Task ExecuteAsync ()
        {
            // Leggo tutte le bike
            var bikes = await (await bikeCollection.FindAsync(t => string.IsNullOrEmpty(t.Country))).ToListAsync();
            foreach (var b in bikes) {
                var bikeLocalized = await GetAddressAsync(b.Latitude, b.Longitude);
                b.City = bikeLocalized.City;
                b.Country = bikeLocalized.Country;
                b.Municipality = bikeLocalized.Municipality;
                b.PostalCode = bikeLocalized.PostalCode;
                await bikeCollection.FindOneAndReplaceAsync(t => t.Id == b.Id, b);
            }

            // Aggrego
            var aggregate = await bikeCollection.Aggregate().Group(t => t.Country,
                ac => new BikeAggregation {
                    Country = ac.Key,
                    Count = ac.Count()
                })
                .ToListAsync();

            // Salvo per country
            foreach (var c in aggregate) {
                var options = new FindOneAndReplaceOptions<BikeAggregation>() {
                    IsUpsert = true
                };
                await bikeAggregationCollection.FindOneAndReplaceAsync<BikeAggregation>(t => t.Country == c.Country, c, options);
            }
        }

        public async Task<GpsInformation> GetAddressAsync (double latitude, double longitude)
        {
            var AZURE_MAPS_KEY = Environment.GetEnvironmentVariable("AZURE_MAPS_KEY");
            var reverseGeocodingResponse = await httpClient.GetAsync(
                $"https://atlas.microsoft.com/search/address/reverse/json?language=en-US&subscription-key={AZURE_MAPS_KEY}&api-version=1.0&query={latitude.ToString().Replace(",", ".")},{longitude.ToString().Replace(",", ".")}");
            if (reverseGeocodingResponse.IsSuccessStatusCode) {
                var reverseGeocodingJson = await reverseGeocodingResponse.Content.ReadAsStringAsync();

                var data = await JsonDocument.ParseAsync(await reverseGeocodingResponse.Content.ReadAsStreamAsync());
                var address = data.RootElement.GetProperty("addresses").EnumerateArray().ToList().FirstOrDefault().GetProperty("address");
                var information = new GpsInformation();
                if (address.TryGetProperty("countryCodeISO3", out var countryCodeElement)) {
                    information.Country = countryCodeElement.GetString();
                }
                if (address.TryGetProperty("countrySecondarySubdivision", out var countrySecondarySubdivisionElement)) {
                    information.City = countrySecondarySubdivisionElement.GetString();
                }
                if (address.TryGetProperty("municipality", out var municipalityElement)) {
                    information.Municipality = municipalityElement.GetString();
                }
                if (address.TryGetProperty("postalCode", out var postalCodeElement)) {
                    information.PostalCode = postalCodeElement.GetString();
                }

                return information;
            }
            return null;
        }
    }

    public class GpsInformation
    {
        public string Country { get; set; }
        public string City { get; set; }
        public string Municipality { get; set; }
        public string PostalCode { get; set; }
    }
}
