using AspireTest.Models;

namespace AspireTest.Web;

public class WeatherApiClient(HttpClient httpClient)
{
    public async Task<WeatherForecast[]> GetWeatherAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        List<WeatherForecast>? forecasts = null;

        await foreach (WeatherForecast? forecast in httpClient.GetFromJsonAsAsyncEnumerable<WeatherForecast>("/weatherforecast", cancellationToken))
        {
            if (forecasts?.Count >= maxItems)
            {
                break;
            }
            if (forecast is not null)
            {
                forecasts ??= [];
                forecasts.Add(forecast);
            }
        }

        return forecasts?.ToArray() ?? [];
    }

    public async Task<List<CityDotProductSimilarity>> GetCitySimilaritiesAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        List<CityDotProductSimilarity>? closestCities = [];

        await foreach (CityDotProductSimilarity? citySimilarity in httpClient.GetFromJsonAsAsyncEnumerable<CityDotProductSimilarity>("/qdrantQuery1", cancellationToken))
        {
            if (closestCities.Count >= maxItems)
            {
                break;
            }
            if (citySimilarity is not null)
            {
                closestCities.Add(citySimilarity);
            }
        }

        return closestCities;
    }
}
